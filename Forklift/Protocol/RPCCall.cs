using Newtonsoft.Json;

namespace Forklift
{
	public class RPCCall
	{
		[JsonProperty("id")]
		public int Id { get; private set; }

		[JsonProperty("method")]
		public string Method { get; private set; }

		[JsonProperty("params")]
		public object[] Arguments { get; private set; }

		public RPCCall(int id, string method, params object[] arguments)
		{
			Id = id;
			Method = method;
			Arguments = arguments;
		}
	}
}
