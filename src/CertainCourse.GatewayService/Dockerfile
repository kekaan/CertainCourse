FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CertainCourse.GatewayService.csproj", "src/CertainCourse.GatewayService/"]
RUN dotnet restore "src/CertainCourse.GatewayService/CertainCourse.GatewayService.csproj"
WORKDIR "src/CertainCourse.GatewayService"
COPY . .
RUN dotnet build "CertainCourse.GatewayService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CertainCourse.GatewayService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CertainCourse.GatewayService.dll"]