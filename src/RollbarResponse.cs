using Newtonsoft.Json;

namespace Archon.Rollbar
{
	class RollbarResponse
	{
		[JsonProperty("err")]
		public int Error { get; set; }

		public RollbarResult Result { get; set; }
	}
}