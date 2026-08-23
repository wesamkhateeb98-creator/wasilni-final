# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SoftPro.Wasilni.Presentation/SoftPro.Wasilni.Presentation.csproj", "SoftPro.Wasilni.Presentation/"]
COPY ["SoftPro.Wasilni.Application/SoftPro.Wasilni.Application.csproj", "SoftPro.Wasilni.Application/"]
COPY ["SoftPro.Wasilni.Domain/SoftPro.Wasilni.Domain.csproj", "SoftPro.Wasilni.Domain/"]
COPY ["SoftPro.Wasilni.Infrastructure/SoftPro.Wasilni.Infrastructure.csproj", "SoftPro.Wasilni.Infrastructure/"]
RUN dotnet restore "./SoftPro.Wasilni.Presentation/SoftPro.Wasilni.Presentation.csproj"
COPY . .
WORKDIR "/src/SoftPro.Wasilni.Presentation"
RUN dotnet build "./SoftPro.Wasilni.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SoftPro.Wasilni.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoftPro.Wasilni.Presentation.dll"]