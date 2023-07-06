FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Crawler.Classifier/Crawler.Classifier.csproj", "src/Crawler.Classifier/"]
COPY ["src/Crawler.Shared/Crawler.Shared.csproj", "src/Crawler.Shared/"]
RUN dotnet restore "src/Crawler.Classifier/Crawler.Classifier.csproj"
COPY . .
WORKDIR "/src/src/Crawler.Classifier"
RUN dotnet build "Crawler.Classifier.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Crawler.Classifier.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Crawler.Classifier.dll"]
