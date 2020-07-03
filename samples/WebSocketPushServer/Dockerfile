# final stage/image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY /bin/release/netcoreapp3.1/publish .
ENTRYPOINT ["dotnet", "WebSocketPushServer.dll"]