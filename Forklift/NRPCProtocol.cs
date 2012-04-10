using System;
using System.Net.Security;
using System.Text;

namespace Forklift
{
	class NRPCProtocol
	{
		const int MaximumBufferSize = 0x400 * ByteBufferSize;
		const int ByteBufferSize = 0x1000;

		SslStream Stream;

		byte[] ByteBuffer;
		string Buffer;

		public NRPCProtocol(SslStream stream)
		{
			Stream = stream;

			ByteBuffer = new byte[ByteBufferSize];
			Buffer = "";
		}

		//Returns true if the connection is still alive
		void ReadBlock()
		{
			int bytesRead = Stream.Read(ByteBuffer, 0, ByteBufferSize);
			if (bytesRead == 0)
				throw new NRPCException("The server has closed the connection");

			string input = Encoding.UTF8.GetString(ByteBuffer);
			Buffer += input;
		}

		//Returns null if the connection was terminated
		string ReadUnitString()
		{
			int? unitSize = null;
			while(true)
			{
				ReadBlock();
				int offset = Buffer.IndexOf(':');
				if (offset != -1)
				{
					string sizeString = Buffer.Substring(0, offset);
					try
					{
						unitSize = Convert.ToInt32(sizeString);
						if (unitSize > MaximumBufferSize)
							throw new NRPCException("The server has specified an excessively large unit size value");
						Buffer = Buffer.Substring(offset + 1);
						break;
					}
					catch (FormatException)
					{
						throw new NRPCException("Encountered an invalid size string");
					}
				}
			}
			while (Buffer.Length < unitSize)
				ReadBlock();
			string unitString = Buffer.Substring(0, unitSize.Value);
			Buffer = Buffer.Substring(unitSize.Value);

			return unitString;
		}
	}
}
