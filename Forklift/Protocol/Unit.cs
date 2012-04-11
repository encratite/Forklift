using Newtonsoft.Json;

namespace Forklift
{
	public class Unit
	{
		[JsonProperty("type")]
		public string Type;

		[JsonProperty("data")]
		public object Data;

		public Unit()
		{
		}

		public Unit(RPCCall call)
		{
			Type = "rpc";
			Data = call;
		}
	}
}
