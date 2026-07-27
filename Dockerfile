# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/UniversityCompanyAppointmentSystem/UniversityCompanyAppointmentSystem.csproj src/UniversityCompanyAppointmentSystem/
RUN dotnet restore src/UniversityCompanyAppointmentSystem/UniversityCompanyAppointmentSystem.csproj

COPY src/UniversityCompanyAppointmentSystem/ src/UniversityCompanyAppointmentSystem/
RUN dotnet publish src/UniversityCompanyAppointmentSystem/UniversityCompanyAppointmentSystem.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render sets PORT; bind Kestrel to it (default 8080 for local Docker runs)
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet UniversityCompanyAppointmentSystem.dll --urls http://0.0.0.0:${PORT:-8080}"]
