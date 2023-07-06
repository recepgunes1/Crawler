FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Crawler.BookParser/Crawler.BookParser.csproj", "src/Crawler.BookParser/"]
RUN dotnet restore "src/Crawler.BookParser/Crawler.BookParser.csproj"
COPY . .
WORKDIR "/src/src/Crawler.BookParser"
RUN dotnet build "Crawler.BookParser.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Crawler.BookParser.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Crawler.BookParser.dll"]
