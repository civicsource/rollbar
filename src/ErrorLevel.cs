using Newtonsoft.Json;

namespace Archon.Rollbar
{
	[JsonConverter(typeof(ErrorLevelConverter))]
	enum ErrorLevel
	{
		Critical,
		Error,
		Warning,
		Info,
		Debug
	}
}