FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

COPY . /app

RUN chmod +x /app/docker-entrypoint.sh \
 	&& sed -i 's/\r$//' /app/docker-entrypoint.sh

RUN apt-get update && apt-get install -y --no-install-recommends curl unzip p7zip-full \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 80
EXPOSE 9339

HEALTHCHECK --interval=30s --timeout=5s --start-period=120s --retries=5 CMD curl -fsS http://127.0.0.1/healthz || exit 1

ENTRYPOINT ["/app/docker-entrypoint.sh"]
