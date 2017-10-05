using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Archon.Rollbar
{
	class RollbarLogger : ILogger
	{
		const string OriginalFormatPropertyName = "{OriginalFormat}";

		readonly HttpContext context;
		readonly string category, accessToken, environment;
		readonly HttpClient client;

		public Server Server { get; set; }

		public RollbarLogger(HttpContext context, HttpClient client, string category, string accessToken, string environment)
		{
			this.context = context;
			this.category = category;
			this.accessToken = accessToken;
			this.environment = environment;
			this.client = client;
		}

		public IDisposable BeginScope<TState>(TState state) => new NothingDisposer();
		class NothingDisposer : IDisposable { public void Dispose() { } }

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, System.Exception exception, Func<TState, System.Exception, string> formatter)
		{
			if (logLevel == LogLevel.None || !IsEnabled(logLevel))
				return;

			var structuredData = ExtractStateStructure(state);
			string msg = formatter(state, exception);

			var body = BuildBody(eventId, msg, exception, structuredData);

			// explicitly don't await the result of the operation here
			// aka fire & forget (don't want to block the application on logging)
			SendBody(body, eventId, Translate(logLevel), structuredData);
		}

		IDictionary<string, object> ExtractStateStructure(object state)
		{
			var dic = new Dictionary<string, object>();

			var structure = state as IEnumerable<KeyValuePair<string, object>>;
			if (structure != null)
			{
				foreach (var property in structure)
				{
					bool isMessageTemplate = property.Key == OriginalFormatPropertyName && property.Value is string;
					if (!isMessageTemplate)
					{
						dic.Add(property.Key, property.Value);
					}
				}
			}

			return dic;
		}

		Body BuildBody(EventId eventId, string message, System.Exception exception, IDictionary<string, object> structuredData)
		{
			structuredData.Add("category", category);

			if (!String.IsNullOrWhiteSpace(eventId.Name))
				structuredData.Add("event", eventId.Name);

			if (exception != null)
			{
				if (!String.IsNullOrWhiteSpace(message))
					structuredData.Add("message", message);

				if (exception is AggregateException ae)
					return new Body(ae);

				return new Body(exception);
			}

			return new Body(new Message(message));
		}

		Task SendBody(Body body, EventId eventId, ErrorLevel level, IDictionary<string, object> custom)
		{
			var payload = new Payload(accessToken, new Data(environment, body)
			{
				GuidUuid = Guid.NewGuid(),
				Custom = custom,
				Level = level,
				Person = CurrentPerson(),
				Server = Server
			});

			if (eventId.Id != 0)
			{
				// https://rollbar.com/docs/grouping-algorithm/
				payload.Data.Fingerprint = eventId.Id.ToString();
			}

			string json = JsonConvert.SerializeObject(payload);

			return client.PostAsync("item/", new StringContent(json, Encoding.UTF8, "application/json"));
		}

		Person CurrentPerson()
		{
			string username = context?.User?.FindFirst(ClaimTypes.Name)?.Value;

			if (String.IsNullOrWhiteSpace(username))
				return null;

			return new Person(username)
			{
				UserName = username,
				Email = context?.User?.FindFirst(ClaimTypes.Email)?.Value
			};
		}

		static ErrorLevel Translate(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Debug: return ErrorLevel.Debug;
				case LogLevel.Information: return ErrorLevel.Info;
				case LogLevel.Warning: return ErrorLevel.Warning;
				case LogLevel.Error: return ErrorLevel.Error;
				case LogLevel.Critical: return ErrorLevel.Critical;
				default: return ErrorLevel.Debug;
			}
		}
	}
}