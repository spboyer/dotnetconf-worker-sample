FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["worker/worker.csproj", "worker/"]
RUN dotnet restore "worker/worker.csproj"
COPY . .
WORKDIR "/src/worker"
RUN dotnet build "worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "worker.dll"]
