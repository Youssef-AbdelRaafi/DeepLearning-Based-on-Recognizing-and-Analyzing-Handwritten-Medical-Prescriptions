FROM mcr.microsoft.com/mssql/server:2022-latest

# Switch to root user for package installation
USER root

# Update package lists and install netcat-openbsd
RUN apt-get update && \
    apt-get install -y netcat-openbsd && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Switch back to mssql user for running SQL Server
USER mssql