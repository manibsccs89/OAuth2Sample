FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 5050

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Resource.Api/Resource.Api.csproj", "Resource.Api/"]
RUN dotnet restore "Resource.Api/Resource.Api.csproj"
COPY . .
WORKDIR "/src/Resource.Api"
RUN dotnet build "Resource.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Resource.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENV ASPNETCORE_URLS "http://*:5050"
ENTRYPOINT ["dotnet", "Resource.Api.dll"]