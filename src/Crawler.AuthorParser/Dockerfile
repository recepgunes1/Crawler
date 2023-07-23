FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Crawler.AuthorParser/Crawler.AuthorParser.csproj", "src/Crawler.AuthorParser/"]
RUN dotnet restore "src/Crawler.AuthorParser/Crawler.AuthorParser.csproj"
COPY . .
WORKDIR "/src/src/Crawler.AuthorParser"
RUN dotnet build "Crawler.AuthorParser.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Crawler.AuthorParser.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Crawler.AuthorParser.dll"]
