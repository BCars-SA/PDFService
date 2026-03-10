# ---- Stage 1: Build .NET API ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-api
WORKDIR /app

COPY API/ .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# ---- Stage 2: API image ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api

RUN apt-get update && apt-get install -y --no-install-recommends curl gettext-base \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build-api /app/publish .

# Save appsettings template for runtime env var substitution
RUN cp /app/appsettings.json /tmp/appsettings.template.json

ENV PORT=5000
EXPOSE ${PORT}

# At runtime: substitute BCARS_USER_SERVICE_BASIC_AUTH in appsettings.json, then start the API
CMD sh -c "exec dotnet API.dll --urls http://*:${PORT}"

# ---- Stage 4: Nginx image ----
FROM nginx:alpine AS nginx

RUN apk add --no-cache curl gettext

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
