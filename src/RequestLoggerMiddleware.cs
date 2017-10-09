using System.IO;
using Microsoft.AspNetCore.Builder;

namespace Archon.Rollbar
{
	public static class RequestLoggerMiddleware
	{
		public static IApplicationBuilder UseRollbarRequestLogger(this IApplicationBuilder app)
		{
			// https://stackoverflow.com/a/31395692/316108
			return app.Use(async (context, next) =>
			{
				// Keep the original stream in a separate
				// variable to restore it later if necessary.
				var stream = context.Request.Body;

				// Optimization: don't buffer the request if
				// there was no stream or if it is rewindable.
				if (stream == Stream.Null || stream.CanSeek)
				{
					await next();
					return;
				}

				try
				{
					using (var buffer = new MemoryStream())
					{
						// Copy the request stream to the memory stream.
						await stream.CopyToAsync(buffer);

						// Rewind the memory stream.
						buffer.Position = 0L;

						// Replace the request stream by the memory stream.
						context.Request.Body = buffer;

						// Invoke the rest of the pipeline.
						await next();
					}
				}
				finally
				{
					// Restore the original stream.
					context.Request.Body = stream;
				}
			});
		}
	}
}