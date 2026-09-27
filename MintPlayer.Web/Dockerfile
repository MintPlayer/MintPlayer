# syntax=docker/dockerfile:1
# Build context is the repo root (C:\Repos\MintPlayer). Spark is consumed from NuGet, so the
# image build is single-context — no sibling Spark clone required.
#
# Production image for ghcr.io/mintplayer/mintplayer (deploy/docker-compose.yml, D14). Mirrors
# MintPlayer.Spark apps/CodeCoverage, with one difference: the RUNTIME stage carries Node 22,
# because SpaServices prerendering (D2) runs the Angular server bundle in a Node process.

ARG NODE_MAJOR=22

# Node binary source for the runtime stage. Only the `node` executable is copied (no npm/npx/corepack):
# prerendering runs a pre-built bundle, it never installs packages. Official node builds need
# glibc >= 2.28 + libstdc++, both present in the Ubuntu-based aspnet:10.0 image.
FROM node:${NODE_MAJOR}-bookworm-slim AS node

# ---- Build stage: .NET SDK + Node (the SPA is built during `dotnet publish` via SpaServices) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG NODE_MAJOR
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

# Install the SPA's dependencies EXACTLY from the lockfile before publishing. Without this, the first
# npm step during `dotnet publish` is MintPlayer.Spark.Authorization's `npm i @mintplayer/ng-spark-auth`
# (it fires whenever node_modules/@mintplayer/ng-spark-auth is missing), which resolves on a fresh tree
# with no node_modules, rewrites package.json/-lock to the newest versions and breaks the Angular build
# (found in S8: unresolved @mintplayer/web-components/accordion, missing ng-bootstrap exports). With
# node_modules present, both that step and NodeServices' `npm install` are skipped.
RUN cd MintPlayer.Web/ClientApp && npm ci --no-audit --no-fund

# Copy the rest and publish. SpaServices' PublishRunWebpack target runs `npm ci` + `ng build`
# and copies ClientApp/dist into the publish output (served by AddSpaStaticFilesImproved).
COPY MintPlayer.Web/ MintPlayer.Web/
COPY MintPlayer.Domain/ MintPlayer.Domain/
RUN dotnet publish "MintPlayer.Web/MintPlayer.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
# Node for SpaServices prerendering. Size cost: the node binary only (~115 MB uncompressed,
# ~40 MB compressed in the registry) — see docs/spikes/S8-hetzner-deploy/RESULT.md for the measured figure.
COPY --from=node /usr/local/bin/node /usr/local/bin/node
RUN ["node", "--version"]

# DataProtection key ring (auth cookies, Identity reset/confirm tokens). Pre-created and owned by `app`
# so the compose volume mounted here inherits that ownership on first creation; without a volume every
# redeploy mints a new key ring and signs everybody out (found in S8).
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys && chown -R app:app /home/app/.aspnet

WORKDIR /app
COPY --from=build /app/publish .
# Non-root (UID 1654, shipped by the aspnet image). Files above stay root-owned = read-only to the app.
USER app
# 8080 only, deliberately: Traefik's port detection picks the single exposed port.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MintPlayer.Web.dll"]
