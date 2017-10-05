using System;
using Microsoft.Extensions.Logging;

namespace Archon.Rollbar
{
	public static class RollbarLoggerFactoryExtensions
	{
		public static ILoggerFactory AddRollbar(this ILoggerFactory factory, string accessToken, string environment)
		{
			return AddRollbar(factory, accessToken, environment, null);
		}

		public static ILoggerFactory AddRollbar(this ILoggerFactory factory, string accessToken, string environment, Server server)
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			if (String.IsNullOrWhiteSpace(accessToken))
				throw new ArgumentNullException(nameof(accessToken));

			if (String.IsNullOrWhiteSpace(environment))
				throw new ArgumentNullException(nameof(environment));

			factory.AddProvider(new RollbarLoggerProvider(null, accessToken, environment)
			{
				Server = server
			});

			return factory;
		}
	}
}
