using System;

namespace Forklift
{
	class NRPCException : Exception
	{
		public NRPCException(string message)
			: base(message)
		{
		}
	}
}
