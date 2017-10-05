using System.Collections.Generic;
using Newtonsoft.Json;

namespace Archon.Rollbar
{
	[JsonConverter(typeof(ArbitraryKeyConverter))]
	class Client : HasArbitraryKeys
	{
		public JavascriptClient Javascript { get; set; }

		protected override void Normalize()
		{
			Javascript = (JavascriptClient)(AdditionalKeys.ContainsKey("javascript") ? AdditionalKeys["javascript"] : Javascript);
			AdditionalKeys.Remove("javascript");
		}

		protected override Dictionary<string, object> Denormalize(Dictionary<string, object> dict)
		{
			if (Javascript != null)
			{
				dict["javascript"] = Javascript;
			}

			return dict;
		}
	}
}
