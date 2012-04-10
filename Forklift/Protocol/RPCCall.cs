using Newtonsoft.Json;

namespace Forklift
{
	public class RPCCall
	{
		[JsonProperty("id")]
		public readonly int Id;

		[JsonProperty("method")]
		public readonly string Method;

		[JsonProperty("params")]
		public readonly object[] Arguments;

		public RPCCall(int id, string method, params object[] arguments)
		{
			Id = id;
			Method = method;
			Arguments = arguments;
		}
	}
}
