FROM mcr.microsoft.com/dotnet/runtime:2.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:2.1 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "Luna.Net.DDNS.Aliyun.csproj" --ignore-failed-sources

WORKDIR "/src/"
RUN dotnet build "Luna.Net.DDNS.Aliyun.csproj" -c Release -o /app --no-restore

FROM build AS publish
RUN dotnet publish "Luna.Net.DDNS.Aliyun.csproj" -c Release -o /app --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Luna.Net.DDNS.Aliyun.dll"]
