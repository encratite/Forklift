using System.Collections.Generic;

namespace Forklift
{
	public class RPCResult
	{
		public int Id;
		public string Error;
		public object Result;

		public RPCResult(Dictionary<string, object> input)
		{
			Id = (int)input["id"];
			Error = (string)input["error"];
			Result = input["result"];
		}
	}
}
