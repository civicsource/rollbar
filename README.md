# Rollbar Logging

> A Rollbar implementation of `Microsoft.Extensions.Logging.ILogger` to be used in .net core applications

## How to Use

### Install via Nuget

```
dotnet add Archon.Rollbar
```

### ASP.NET Core

Configure it [like you would any other logger](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging). In your `Program.cs`:

```cs
var webHost = new WebHostBuilder()
	.ConfigureLogging(logging => logging.AddRollbar(
		"my_rollbar_access_token",
		"rollbar_environment",
		new Server { CodeVersion = "v1.0.0" } // this param is optional
		LogLevel.Warning // optional log level threshold: only logs at this level or higher will be sent to rollbar (e.g. Warning, Error, Critical)
	));
```

And then in your `Startup.Configure`, setup the rollbar request logger middleware:

```cs
app.UseRollbarRequestLogger();
```

Note: The `UseRollbarRequestLogger` extension method will [setup a middleware that will buffer the request body stream](https://stackoverflow.com/a/31395692/316108) so that the logger can read it & send it to rollbar. It is not required to use the logging integration if you do not care about providing any `POST` bodies to rollbar.

You can then use `ILogger` or `ILogger<T>` [like you normally would](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging#how-to-create-logs) to create some logs that will show up in your rollbar account.

### .NET Core Console App

If you want to use the logging infrastructure outside of ASP.Net Core, you will need to instantiate the `IServiceCollection` yourself:

```cs
var serviceProvider = new ServiceCollection()
	.AddLogging(logging => logging.AddRollbar("my_rollbar_access_token", "rollbar_environment"))
	.BuildServiceProvider();

ILogger<Whatever> logger = lg.CreateLogger<Whatever>();
logger.LogDebug("All done!");
```

If you don't want to use the built-in `Microsoft.Extensions.DependencyInjection`, you will have to create the `LoggerFactory` yourself:

```cs
ILoggerFactory lg = new LoggerFactory();
lg.AddRollbar("my_rollbar_access_token", "rollbar_environment");

ILogger<Whatever> logger = lg.CreateLogger<Whatever>();
logger.LogInformation("Woo hoo!");
```

In this case, if you want to specify a threshold for the rollbar logger, you will have to add the filter manually:

```cs
var lg = new LoggerFactory(
	new ILoggerProvider[0],
	new LoggerFilterOptions().AddFilter<RollbarLoggerProvider>(null, threshold)
);

lg.AddRollbar("my_rollbar_access_token", "rollbar_environment");
```

This is all the `ILoggingBuilder` extension method is doing internally.

### Event IDs

The rollbar logger will honor any [log event IDs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging#log-event-id) you use when you create your logs. It will pass the event ID as the [rollbar log fingerprint](https://rollbar.com/docs/custom-grouping/#fingerprint). This means any logs logged with the same event ID will be grouped together in rollbar as a single item. This makes it easy to filter noise out of your logs.

If you don't pass an event ID to your log, Rollbar will use its built-in fingerprinting algorithm.

### Structured Logging

The `Microsoft.Extensions.Logging` infrastructure [allows for structured logs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging#log-message-format-string). This means, if you create a log like this:

```cs
string thing = "poop";
string stuff = "toilets";
logger.LogInformation("Doing a {thing} with {stuff}", thing, stuff);
```

This will send the correct log message to Rollbar while also including the structured data so that you can query it using [Rollbar's RQL](https://rollbar.com/docs/rql/):

```json
{
	"message": "Doing a poop with toilets",
	"thing": "poop",
	"stuff": "toilets"
}
```

### User Data

The `RollbarLogger` makes use of `ClaimsPrincipal` to retrieve username, email, and a user ID to send to rollbar. As long as the current user is authenticated and provides a `ClaimsPrincipal`, that user information will be sent to Rollbar.

## Why Not Use the [Official Integration](https://github.com/rollbar/Rollbar.NET)?

Most of the implementation here is copy pasta'd from [the official repo](https://github.com/rollbar/Rollbar.NET) with a few notable differences:

1. Use `HttpClient` instead of `WebRequest` because it is not 2004 anymore.
2. No static `Rollbar` class. Use dependency injection with `ILogger`s. Again, not 2004 anymore.
3. The official integration isn't compiled to .net standard (I did [try to help](https://github.com/rollbar/Rollbar.NET/pull/26) 😒).

## How to Build

To build, clone this repo and run:

```
dotnet restore
dotnet build
```
