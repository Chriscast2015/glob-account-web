using System.Threading.RateLimiting;
using GlobAccountAPI.Options;
using GlobAccountAPI.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

const string contactRateLimitPolicy = "ContactFormPolicy";

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CaptchaOptions>(builder.Configuration.GetSection("Captcha"));
builder.Services.Configure<ContactSecurityOptions>(builder.Configuration.GetSection("ContactSecurity"));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
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

builder.Services.AddScoped<IEmailService, EmailService>();
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
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173" };

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
}

app.UseHttpsRedirection();
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
