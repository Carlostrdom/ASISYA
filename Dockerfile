FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Asisya.API/Asisya.API.csproj", "Asisya.API/"]
COPY ["Asisya.Application/Asisya.Application.csproj", "Asisya.Application/"]
COPY ["Asisya.Domain/Asisya.Domain.csproj", "Asisya.Domain/"]
COPY ["Asisya.Infrastructure/Asisya.Infrastructure.csproj", "Asisya.Infrastructure/"]

RUN dotnet restore "Asisya.API/Asisya.API.csproj"

COPY . .

RUN dotnet publish "Asisya.API/Asisya.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Asisya.API.dll"]
