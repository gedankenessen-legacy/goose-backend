FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Goose.API/Goose.API.csproj", "Goose.API/"]
COPY ["Goose.Domain/Goose.Domain.csproj", "Goose.Domain/"]
COPY ["Goose.Data/Goose.Data.csproj", "Goose.Data/"]
RUN dotnet restore "Goose.API/Goose.API.csproj"
COPY . .
WORKDIR "/src/Goose.API"
RUN dotnet build "Goose.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Goose.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Goose.API.dll"]