using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Archon.Rollbar
{
	public class RollbarLoggerProvider : ILoggerProvider
	{
		readonly IHttpContextAccessor context;
		readonly string accessToken, environment;
		readonly HttpClient client;

		public Server Server { get; set; }

		public RollbarLoggerProvider(IHttpContextAccessor context, string accessToken, string environment)
		{
			this.context = context;
			this.accessToken = accessToken;
			this.environment = environment;

			// use one HttpClient for life of application
			client = new HttpClient()
			{
				BaseAddress = new Uri("https://api.rollbar.com/api/1/")
			};
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new RollbarLogger(context, client, categoryName, accessToken, environment)
			{
				Server = Server
			};
		}

		public void Dispose()
		{
			client?.Dispose();
		}
	}
}