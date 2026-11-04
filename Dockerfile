
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
USER $APP_UID


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
#build
COPY ["src/Hiflux.Api/Hiflux.Api.csproj", "src/Hiflux.Api/"]
COPY ["src/Hiflux.Core/Hiflux.Core.csproj", "src/Hiflux.Core/"]
COPY ["src/Hiflux.Infrastructure/Hiflux.Infrastructure.csproj", "src/Hiflux.Infrastructure/"]
RUN dotnet restore  "src/Hiflux.Api/Hiflux.Api.csproj"
COPY . .
RUN dotnet publish "src/Hiflux.Api/Hiflux.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hiflux.Api.dll"]

