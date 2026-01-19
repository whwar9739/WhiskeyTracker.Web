# 1. Build Stage
# We use the SDK image to compile the code
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file first to cache the Restore step
COPY ["WhiskeyTracker.Web/WhiskeyTracker.Web.csproj", "WhiskeyTracker.Web/"]
RUN dotnet restore "WhiskeyTracker.Web/WhiskeyTracker.Web.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/WhiskeyTracker.Web"
RUN dotnet build "WhiskeyTracker.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 2. Publish Stage
# Publish the optimized dlls to a folder
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WhiskeyTracker.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 3. Runtime Stage
# This is the final, small image that actually runs on your Pi
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Create a non-root user for security (Best Practice)
USER app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WhiskeyTracker.Web.dll"]