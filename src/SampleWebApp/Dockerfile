#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SampleWebApp/SampleWebApp.csproj", "src/SampleWebApp/"]
RUN dotnet restore "./src/SampleWebApp/./SampleWebApp.csproj"
COPY . .
WORKDIR "/src/src/SampleWebApp"
RUN dotnet build "./SampleWebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SampleWebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Copy AppDynamicsConfig.json into /opt/appdynamics folder
COPY ["AppDynamicsConfig.json", "/opt/appdynamics/"]

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleWebApp.dll"]


	# Copy agent binaries to the image from current folder
RUN mkdir -p /opt/appdynamics/dotnet
ADD SampleWebApp/libappdprofiler.so /opt/appdynamics/dotnet/
ADD SampleWebApp/libappdprofiler_glibc.so /opt/appdynamics/dotnet/
ADD SampleWebApp/libappdprofiler_musl.so /opt/appdynamics/dotnet/
ADD SampleWebApp/AppDynamics.Agent.netstandard.dll /opt/appdynamics/dotnet/
