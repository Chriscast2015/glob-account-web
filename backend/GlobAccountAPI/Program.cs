using System.Threading.RateLimiting;
using GlobAccountAPI.Options;
using GlobAccountAPI.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

const string contactRateLimitPolicy = "ContactFormPolicy";

var builder = WebApplication.CreateBuilder(args);

ConfigureLogging(builder);
ConfigureRenderPort(builder);

builder.Services.Configure<CaptchaOptions>(builder.Configuration.GetSection("Captcha"));
builder.Services.Configure<ContactSecurityOptions>(builder.Configuration.GetSection("ContactSecurity"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<EmailApiOptions>(builder.Configuration.GetSection("EmailApi"));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;
    options.ForwardLimit = 1;

    var allowedHosts = GetConfiguredValues(builder.Configuration, "AllowedHosts", Array.Empty<string>());

    if (allowedHosts.Length > 0 && !allowedHosts.Contains("*", StringComparer.Ordinal))
    {
        options.AllowedHosts = allowedHosts;
    }

    if (!builder.Environment.IsDevelopment())
    {
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    }
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = builder.Configuration.GetValue<int?>("HttpsRedirection:HttpsPort") ?? 443;
});

builder.Services.AddControllers();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(item => item.Value?.Errors.Count > 0)
            .ToDictionary(
                item => item.Key,
                item => item.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
        {
            success = false,
            message = "La solicitud contiene campos invalidos.",
            errors
        });
    };
});

builder.Services.AddScoped<SmtpEmailService>();
builder.Services.AddScoped<IEmailService, ConfiguredEmailService>();
builder.Services.AddHttpClient<EmailApiService>((serviceProvider, client) =>
{
    var timeoutSeconds = serviceProvider
        .GetRequiredService<IConfiguration>()
        .GetValue<int?>("EmailApi:TimeoutSeconds") ?? 30;

    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds > 0 ? timeoutSeconds : 30);
});
builder.Services.AddHttpClient<ICaptchaVerifier, TurnstileCaptchaVerifier>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int?>("Captcha:TimeoutSeconds") ?? 5);
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(contactRateLimitPolicy, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue<int?>("RateLimiting:Contact:PermitLimit") ?? 5,
                Window = TimeSpan.FromMinutes(builder.Configuration.GetValue<int?>("RateLimiting:Contact:WindowMinutes") ?? 10),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo { Title = "GlobAccountAPI", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var fallbackOrigins = builder.Environment.IsDevelopment()
            ? new[] { "http://localhost:5173", "http://127.0.0.1:5173" }
            : Array.Empty<string>();
        var allowedOrigins = GetConfiguredValues(builder.Configuration, "Cors:AllowedOrigins", fallbackOrigins);

        policy.WithOrigins(allowedOrigins)
            .WithHeaders("Content-Type")
            .WithMethods("GET", "POST", "OPTIONS");
    });
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseSecurityHeaders();
app.UseConfiguredContactRequestSizeLimit();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.MapControllers();
app.Run();

static string GetClientPartitionKey(HttpContext context)
{
    return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

static void ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
}

static void ConfigureRenderPort(WebApplicationBuilder builder)
{
    var explicitUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
    var commandLineUrls = builder.Configuration["urls"];
    var port = Environment.GetEnvironmentVariable("PORT");

    if (!string.IsNullOrWhiteSpace(explicitUrls)
        || !string.IsNullOrWhiteSpace(commandLineUrls)
        || !int.TryParse(port, out var parsedPort)
        || parsedPort <= 0)
    {
        return;
    }

    builder.WebHost.UseUrls($"http://0.0.0.0:{parsedPort}");
}

static string[] GetConfiguredValues(IConfiguration configuration, string key, string[] fallbackValues)
{
    var sectionValues = configuration.GetSection(key).Get<string[]>();
    var rawValue = configuration[key];
    var values = sectionValues is { Length: > 0 }
        ? sectionValues
        : SplitConfiguredValue(rawValue);

    return values.Length > 0
        ? values
        : fallbackValues;
}

static string[] SplitConfiguredValue(string? value)
{
    return string.IsNullOrWhiteSpace(value)
        ? Array.Empty<string>()
        : value.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

static class ContactRequestSizeLimitExtensions
{
    public static IApplicationBuilder UseConfiguredContactRequestSizeLimit(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if (!HttpMethods.IsPost(context.Request.Method)
                || !context.Request.Path.StartsWithSegments("/api/contact", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var maxRequestBodyBytes = context.RequestServices
                .GetRequiredService<IOptions<ContactSecurityOptions>>()
                .Value
                .MaxRequestBodyBytes;

            if (maxRequestBodyBytes > 0)
            {
                var maxRequestBodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();

                if (maxRequestBodySizeFeature is { IsReadOnly: false })
                {
                    maxRequestBodySizeFeature.MaxRequestBodySize = maxRequestBodyBytes;
                }

                if (context.Request.ContentLength > maxRequestBodyBytes)
                {
                    context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "La solicitud excede el tamano permitido."
                    });

                    return;
                }
            }

            await next();
        });
    }
}

static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            headers.TryAdd("X-Content-Type-Options", "nosniff");
            headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
            headers.TryAdd("X-Frame-Options", "DENY");
            headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

            var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var contentSecurityPolicy = context.RequestServices
                .GetRequiredService<IConfiguration>()["SecurityHeaders:ContentSecurityPolicy"];

            if (!environment.IsDevelopment() && !string.IsNullOrWhiteSpace(contentSecurityPolicy))
            {
                headers.TryAdd("Content-Security-Policy", contentSecurityPolicy);
            }

            await next();
        });
    }
}
