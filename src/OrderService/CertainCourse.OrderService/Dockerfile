FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/OrderService
COPY ["CertainCourse.OrderService.csproj", "src/CertainCourse.OrderService/"]
RUN dotnet restore "src/CertainCourse.OrderService/CertainCourse.OrderService.csproj"
WORKDIR "src/CertainCourse.OrderService"
COPY . .
RUN dotnet build "CertainCourse.OrderService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CertainCourse.OrderService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CertainCourse.OrderService.dll"]