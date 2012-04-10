using System;
using System.Net.Security;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

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

		public void Run()
		{
			while (true)
				ProcessUnit();
		}

		//Returns true if the connection is still alive
		void ReadBlock()
		{
			int bytesRead = Stream.Read(ByteBuffer, 0, ByteBufferSize);
			if (bytesRead == 0)
				throw new NRPCException("The connection has been closed");

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

		void ProcessUnit()
		{
			string unitString = ReadUnitString();
			Unit unit = JsonConvert.DeserializeObject<Unit>(unitString);
			if (unit.Type == "notification")
			{
				NotificationData baseNotification = new NotificationData(unit.Data);
				if (baseNotification.Type == "queued")
				{
					QueuedNotification notification = new QueuedNotification(baseNotification);
				}
				else if (baseNotification.Type == "downloaded")
				{
					DownloadedNotification notification = new DownloadedNotification(baseNotification);
				}
				else if (baseNotification.Type == "downloadError")
				{
					DownloadError notification = new DownloadError(baseNotification);
				}
				else if (baseNotification.Type == "downloadDeleted")
				{
					DownloadDeletedNotification notification = new DownloadDeletedNotification(baseNotification);
				}
				else if (baseNotification.Type == "serviceMessage")
				{
					ServiceMessage notification = new ServiceMessage(baseNotification);
				}
				else
					throw new NRPCException("Encountered an unknown notification type");
			}
			else
				throw new NRPCException("Encountered an unknown unit type");
		}

		void SendUnitString(string unitString)
		{
			string packet = unitString.Length.ToString() + ":" + unitString;
			Stream.Write(Encoding.UTF8.GetBytes(packet));
		}

		void SendUnit(object unit)
		{
			SendUnitString(JsonConvert.SerializeObject(unit));
		}
	}
}
