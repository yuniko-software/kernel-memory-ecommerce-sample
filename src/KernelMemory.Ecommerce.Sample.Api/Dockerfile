FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/KernelMemory.Ecommerce.Sample.Api/KernelMemory.Ecommerce.Sample.Api.csproj", "src/KernelMemory.Ecommerce.Sample.Api/"]
RUN dotnet restore "./src/KernelMemory.Ecommerce.Sample.Api/KernelMemory.Ecommerce.Sample.Api.csproj"
COPY . .
WORKDIR "/src/src/KernelMemory.Ecommerce.Sample.Api"
RUN dotnet build "./KernelMemory.Ecommerce.Sample.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./KernelMemory.Ecommerce.Sample.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KernelMemory.Ecommerce.Sample.Api.dll"]