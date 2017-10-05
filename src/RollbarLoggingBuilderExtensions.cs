using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Archon.Rollbar
{
	public static class RollbarLoggingBuilderExtensions
	{
		public static ILoggingBuilder AddRollbar(this ILoggingBuilder builder, string accessToken, string environment)
		{
			return AddRollbar(builder, accessToken, environment, null);
		}

		public static ILoggingBuilder AddRollbar(this ILoggingBuilder builder, string accessToken, string environment, Server server)
		{
			if (builder == null)
				throw new ArgumentNullException(nameof(builder));

			if (String.IsNullOrWhiteSpace(accessToken))
				throw new ArgumentNullException(nameof(accessToken));

			if (String.IsNullOrWhiteSpace(environment))
				throw new ArgumentNullException(nameof(environment));

			builder.Services.AddSingleton<ILoggerProvider>(s => new RollbarLoggerProvider(s.GetService<IHttpContextAccessor>(), accessToken, environment)
			{
				Server = server
			});

			return builder;
		}

		public static ILoggingBuilder AddRollbar(this ILoggingBuilder builder, string accessToken, string environment, Server server, LogLevel threshold)
		{
			AddRollbar(builder, accessToken, environment, server);
			builder.AddFilter<RollbarLoggerProvider>(null, threshold);
			return builder;
		}
	}
}
