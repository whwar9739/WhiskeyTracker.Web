# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file first to cache the Restore step
COPY ["WhiskeyTracker.Web/WhiskeyTracker.Web.csproj", "WhiskeyTracker.Web/"]
RUN dotnet restore "WhiskeyTracker.Web/WhiskeyTracker.Web.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/WhiskeyTracker.Web"

# 2. Publish Stage
# This handles building AND publishing the optimized files
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WhiskeyTracker.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 3. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Switch to non-root user for security
USER app
COPY --from=publish --chown=app:app /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1


ENTRYPOINT ["dotnet", "WhiskeyTracker.Web.dll"]
