FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5116

ENV ASPNETCORE_URLS=http://+:5116

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["ScheduledJobRunner/ScheduledJobRunner.csproj", "ScheduledJobRunner/"]
RUN dotnet restore "ScheduledJobRunner/ScheduledJobRunner.csproj"
COPY . .
WORKDIR "/src/ScheduledJobRunner"
RUN dotnet build "ScheduledJobRunner.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "ScheduledJobRunner.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScheduledJobRunner.dll"]
