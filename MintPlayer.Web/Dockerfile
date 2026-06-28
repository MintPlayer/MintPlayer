# syntax=docker/dockerfile:1
# Build context is the repo root (C:\Repos\MintPlayer). Spark is consumed from NuGet, so the
# image build is single-context — no sibling Spark clone required.

# ---- Build stage: .NET SDK + Node (the SPA is built during `dotnet publish` via SpaServices) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG NODE_MAJOR=22
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates gnupg \
    && curl -fsSL https://deb.nodesource.com/setup_${NODE_MAJOR}.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Restore first (cached layer) — only the project files + SPA manifest.
COPY ["MintPlayer.Web/MintPlayer.Web.csproj", "MintPlayer.Web/"]
COPY ["MintPlayer.Domain/MintPlayer.Domain.csproj", "MintPlayer.Domain/"]
COPY ["MintPlayer.Web/ClientApp/package.json", "MintPlayer.Web/ClientApp/package-lock.json", "MintPlayer.Web/ClientApp/"]
RUN dotnet restore "MintPlayer.Web/MintPlayer.Web.csproj"

# Copy the rest and publish. SpaServices' PublishRunWebpack target runs `npm ci` + `ng build`
# and copies ClientApp/dist into the publish output (served by AddSpaStaticFilesImproved).
COPY MintPlayer.Web/ MintPlayer.Web/
COPY MintPlayer.Domain/ MintPlayer.Domain/
RUN dotnet publish "MintPlayer.Web/MintPlayer.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MintPlayer.Web.dll"]
