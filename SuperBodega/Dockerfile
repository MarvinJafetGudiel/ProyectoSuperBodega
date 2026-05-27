FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SuperBodega.API/SuperBodega.API.csproj SuperBodega.API/

RUN dotnet restore SuperBodega.API/SuperBodega.API.csproj

COPY . .

WORKDIR /src/SuperBodega.API
RUN dotnet publish SuperBodega.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SuperBodega.API.dll"]