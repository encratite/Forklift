using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

namespace Forklift
{
	class NRPCProtocol
	{
		public delegate void RPCResultCallback(params object[] arguments);

		const int MaximumBufferSize = 0x400 * ByteBufferSize;
		const int ByteBufferSize = 0x1000;

		const string GetNewNotificationsMethod = "getNewNotifications";
		const string GetNotificationCountMethod = "getNotificationCount";
		const string GetOldNotificationsMethod = "getOldNotifications";
		const string GenerateNotificationMethod = "generateNotification";

		SslStream Stream;

		byte[] ByteBuffer;
		string Buffer;

		int CallCounter;
		Dictionary<int, RPCResultCallback> Callbacks;

		public NRPCProtocol(SslStream stream)
		{
			Stream = stream;

			ByteBuffer = new byte[ByteBufferSize];
			Buffer = "";

			CallCounter = 1;
			Callbacks = new Dictionary<int, RPCResultCallback>();
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

		void NotImplemented()
		{
			throw new NRPCException("This feature has not been implemented yet");
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
					NotImplemented();
				}
				else if (baseNotification.Type == "downloaded")
				{
					DownloadedNotification notification = new DownloadedNotification(baseNotification);
					NotImplemented();
				}
				else if (baseNotification.Type == "downloadError")
				{
					DownloadError notification = new DownloadError(baseNotification);
					NotImplemented();
				}
				else if (baseNotification.Type == "downloadDeleted")
				{
					DownloadDeletedNotification notification = new DownloadDeletedNotification(baseNotification);
					NotImplemented();
				}
				else if (baseNotification.Type == "serviceMessage")
				{
					ServiceMessage notification = new ServiceMessage(baseNotification);
					NotImplemented();
				}
				else
					throw new NRPCException("Encountered an unknown notification type");
			}
			else if (unit.Type == "rpcResult")
			{
				RPCResult result = new RPCResult(unit.Data);
				RPCResultCallback callback;
				if (!Callbacks.TryGetValue(result.Id, out callback))
					throw new NRPCException("Server provided an invalid RPC result ID");
				Callbacks.Remove(result.Id);
				if (result.Error == null)
					callback(result.Result);
				else
					throw new NRPCException(string.Format("RPC error: {0}", result.Error));
			}
			else if (unit.Type == "ping")
			{
				//Ignore pings
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

		void PerformRPC(RPCResultCallback callback, string method, params object[] arguments)
		{
			RPCCall call = new RPCCall(CallCounter, method, arguments);
			SendUnit(call);
			Callbacks[CallCounter] = callback;
			CallCounter++;
		}

		void GetNewNotifications(RPCResultCallback callback)
		{
			PerformRPC(callback, GetNewNotificationsMethod);
		}

		void GetNotificationCount(RPCResultCallback callback)
		{
			PerformRPC(callback, GetNotificationCountMethod);
		}

		void GetOldNotifications(RPCResultCallback callback, int firstIndex, int lastIndex)
		{
			PerformRPC(callback, GetOldNotificationsMethod, firstIndex, lastIndex);
		}

		void GenerateNotification(RPCResultCallback callback, string type, string content)
		{
			PerformRPC(callback, GenerateNotificationMethod, type, content);
		}
	}
}
