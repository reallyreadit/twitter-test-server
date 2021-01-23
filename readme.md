# reallyread.it twitter-test-server
## Configuration
Create the following configuration files and modify as required:

    appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5003"
      }
    }
  }
}
```