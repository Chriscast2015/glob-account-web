FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY GlobAccountAPI.csproj ./
RUN dotnet restore GlobAccountAPI.csproj

COPY . ./
RUN dotnet publish GlobAccountAPI.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080
COPY --from=build /app/publish ./

ENTRYPOINT ["sh", "-c", "dotnet GlobAccountAPI.dll --urls http://0.0.0.0:${PORT:-8080}"]
