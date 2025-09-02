# =========================
# Build Stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything and restore
COPY . .
RUN dotnet restore

# Publish the application
RUN dotnet publish -c Release -o /app/publish

# =========================
# Runtime Stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# ? Install ICU for globalization (fixes $ showing as ¤)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libc6 \
    libicu-dev \
    locales \
    && rm -rf /var/lib/apt/lists/*

# ? Enable full globalization support
RUN locale-gen en_US.UTF-8  

# Set environment variables
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LANG=en_US.UTF-8
ENV LANGUAGE=en_US:en
ENV LC_ALL=en_US.UTF-8

# Copy published output from build stage
COPY --from=build /app/publish .

# Auto-detect the main DLL (in case project name changes)
CMD ["sh", "-c", "dotnet $(ls *.dll | grep -v Test | head -n 1)"]
