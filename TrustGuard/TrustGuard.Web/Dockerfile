FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root

# 1. Ставимо Tesseract і системні ліби
RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    libtesseract-dev \
    libleptonica-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TrustGuard.Web/TrustGuard.Web.csproj", "TrustGuard.Web/"]
COPY ["TrustGuard.Application/TrustGuard.Application.csproj", "TrustGuard.Application/"]
COPY ["TrustGuard.Domain/TrustGuard.Domain.csproj", "TrustGuard.Domain/"]
COPY ["TrustGuard.Infrastructure/TrustGuard.Infrastructure.csproj", "TrustGuard.Infrastructure/"]
RUN dotnet restore "./TrustGuard.Web/TrustGuard.Web.csproj"
COPY . .
WORKDIR "/src/TrustGuard.Web"
RUN dotnet build "./TrustGuard.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TrustGuard.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
# 2. Видаляємо старий шлак від NuGet
RUN rm -rf runtimes/linux-x64/native/*

# 3. СТВОРЮЄМО ПАПКУ x64
RUN mkdir -p /app/x64 \
    && ln -s /usr/lib/x86_64-linux-gnu/libleptonica.so /app/x64/libleptonica-1.82.0.so \
    && ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so /app/x64/libtesseract50.so \
    && ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so /app/x64/libtesseract53.so \
    && ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /app/libdl.so

COPY TrustGuard.Web/tessdata ./tessdata

ENTRYPOINT ["dotnet", "TrustGuard.Web.dll"]
