FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ECommerce.slnx", "./"]
COPY ["src/", "src/"]

RUN dotnet restore "src/Host/ECommerce.API/ECommerce.API.csproj"
RUN dotnet publish "src/Host/ECommerce.API/ECommerce.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ECommerce.API.dll"]

