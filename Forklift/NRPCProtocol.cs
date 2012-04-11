using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forklift
{
	class NRPCProtocol
	{
		public delegate void RPCResultCallback(params object[] arguments);

		const int MaximumBufferSize = 0x400 * ByteBufferSize;
		const int ByteBufferSize = 0x1000;

		const string GetNewNotificationsMethod = "getNewNotifications";
		const string GetNotificationCountMethod = "getNotificationCount";
		const string GetNotificationsMethod = "getNotifications";
		const string GenerateNotificationMethod = "generateNotification";

		SslStream Stream;

		INotificationHandler NotificationHandler;

		byte[] ByteBuffer;
		string Buffer;

		int CallCounter;
		Dictionary<int, RPCResultCallback> Callbacks;

		public NRPCProtocol(SslStream stream, INotificationHandler notificationHandler)
		{
			Stream = stream;

			NotificationHandler = notificationHandler;

			ByteBuffer = new byte[ByteBufferSize];
			Buffer = "";

			CallCounter = 1;
			Callbacks = new Dictionary<int, RPCResultCallback>();
		}

		//Returns true if the connection is still alive
		void ReadBlock()
		{
			int bytesRead = Stream.Read(ByteBuffer, 0, ByteBufferSize);
			if (bytesRead == 0)
				throw new NRPCException("The connection has been closed");

			string input = Encoding.UTF8.GetString(ByteBuffer, 0, bytesRead);
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

		public static DeserialisedType DeserialiseObject<DeserialisedType>(object input)
		{
			JsonSerializer serialiser = new JsonSerializer();
			DeserialisedType output = (DeserialisedType)serialiser.Deserialize<DeserialisedType>(new JTokenReader((JObject)input));
			return output;
		}

		public static DeserialisedType DeserialiseNotification<DeserialisedType>(NotificationData baseNotification) where DeserialisedType : Notification
		{
			DeserialisedType output = DeserialiseObject<DeserialisedType>(baseNotification.Content);
			output.Time = baseNotification.Time;
			return output;
		}

		public static Notification GetNotification(object input)
		{
			NotificationData baseNotification = DeserialiseObject<NotificationData>(input);
			if (baseNotification.Type == "queued")
				return DeserialiseNotification<QueuedNotification>(baseNotification);
			else if (baseNotification.Type == "downloaded")
				return DeserialiseNotification<DownloadedNotification>(baseNotification);
			else if (baseNotification.Type == "downloadError")
				return DeserialiseNotification<DownloadError>(baseNotification);
			else if (baseNotification.Type == "downloadDeleted")
				return DeserialiseNotification<DownloadDeletedNotification>(baseNotification);
			else if (baseNotification.Type == "serviceMessage")
				return DeserialiseNotification<ServiceMessage>(baseNotification);
			else
				throw new NRPCException("Encountered an unknown notification type string");
		}

		public void ProcessUnit()
		{
			string unitString = ReadUnitString();
			Unit unit = JsonConvert.DeserializeObject<Unit>(unitString);
			if (unit.Type == "notification")
			{
				Notification notification = GetNotification(unit.Data);
				Type notificationType = notification.GetType();
				if (notificationType == typeof(QueuedNotification))
					NotificationHandler.HandleQueuedNotification((QueuedNotification)notification);
				else if (notificationType == typeof(DownloadedNotification))
					NotificationHandler.HandleDownloadedNotification((DownloadedNotification)notification);
				else if (notificationType == typeof(DownloadError))
					NotificationHandler.HandleDownloadError((DownloadError)notification);
				else if (notificationType == typeof(DownloadDeletedNotification))
					NotificationHandler.HandleDownloadDeletedNotification((DownloadDeletedNotification)notification);
				else if (notificationType == typeof(ServiceMessage))
					NotificationHandler.HandleServiceMessage((ServiceMessage)notification);
				else
					throw new NRPCException("Encountered an unknown notification class type");
			}
			else if (unit.Type == "rpcResult")
			{
				RPCResult result = DeserialiseObject<RPCResult>(unit.Data);
				RPCResultCallback callback;
				if (!Callbacks.TryGetValue(result.Id, out callback))
					throw new NRPCException("Server provided an invalid RPC result ID");
				Callbacks.Remove(result.Id);
				if (result.Error == null)
					callback(result.Result);
				else
					throw new NRPCException(string.Format("RPC error: {0}", result.Error));
			}
			else if (unit.Type == "error")
			{
				string message = (string)unit.Data;
				NotificationHandler.HandleError(message);
			}
			else if (unit.Type == "ping")
				NotificationHandler.HandlePing();
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
			Unit unit = new Unit(call);
			SendUnit(unit);
			Callbacks[CallCounter] = callback;
			CallCounter++;
		}

		public void GetNewNotifications(RPCResultCallback callback)
		{
			PerformRPC(callback, GetNewNotificationsMethod);
		}

		public void GetNotificationCount(RPCResultCallback callback)
		{
			PerformRPC(callback, GetNotificationCountMethod);
		}

		public void GetNotifications(RPCResultCallback callback, int firstIndex, int lastIndex)
		{
			PerformRPC(callback, GetNotificationsMethod, firstIndex, lastIndex);
		}

		public void GenerateNotification(RPCResultCallback callback, string type, object content)
		{
			PerformRPC(callback, GenerateNotificationMethod, type, content);
		}
	}
}
