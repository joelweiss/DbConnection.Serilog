[![Build status](https://ci.appveyor.com/api/projects/status/3u0ch6pkmcxffk96/branch/master?svg=true)](https://ci.appveyor.com/project/joelweiss/dbconnection-serilog/branch/master)

#DbConnection.Serilog

Logs your DbConnection activity to Serilog.

# Installation
```
PM> Install-Package DbConnection.Serilog -Pre 
```

# Usage
```csharp
using DbConnection.Serilog;

new LoggingConnectionWrapper(dbConnection, _Logger);
```