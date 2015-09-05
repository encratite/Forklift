using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forklift
{
	class NRPCProtocol
	{
		public delegate void RPCResultCallback(params object[] arguments);

		private const int MaximumBufferSize = 0x400 * ByteBufferSize;
		private const int ByteBufferSize = 0x1000;

		private const string GetNewNotificationsMethod = "getNewNotifications";
		private const string GetNotificationCountMethod = "getNotificationCount";
		private const string GetNotificationsMethod = "getNotifications";
		private const string GenerateNotificationMethod = "generateNotification";

		private SslStream _Stream;

		private INotificationHandler _NotificationHandler;
		private IOutputHandler _OutputHandler;

		private byte[] _ByteBuffer;
		private string _Buffer;

		private int _CallCounter;
		private Dictionary<int, RPCResultCallback> _Callbacks;

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
			Notification output = GetUninitialisedNotification(input);
			output.Initialise(true);
			return output;
		}

		public NRPCProtocol(SslStream stream, INotificationHandler notificationHandler, IOutputHandler outputHandler)
		{
			_Stream = stream;

			_NotificationHandler = notificationHandler;
			_OutputHandler = outputHandler;

			_ByteBuffer = new byte[ByteBufferSize];
			_Buffer = "";

			_CallCounter = 1;
			_Callbacks = new Dictionary<int, RPCResultCallback>();
		}

		public void ProcessUnit()
		{
			string unitString = ReadUnitString();
			Unit unit = JsonConvert.DeserializeObject<Unit>(unitString);
			if (unit.Type == "notification")
			{
				var notification = GetNotification(unit.Data);
				if (notification is QueuedNotification)
					_NotificationHandler.HandleQueuedNotification((QueuedNotification)notification);
				else if (notification is DownloadedNotification)
					_NotificationHandler.HandleDownloadedNotification((DownloadedNotification)notification);
				else if (notification is DownloadError)
					_NotificationHandler.HandleDownloadError((DownloadError)notification);
				else if (notification is DownloadDeletedNotification)
					_NotificationHandler.HandleDownloadDeletedNotification((DownloadDeletedNotification)notification);
				else if (notification is ServiceMessage)
					_NotificationHandler.HandleServiceMessage((ServiceMessage)notification);
				else
					throw new NRPCException("Encountered an unknown notification class type");
			}
			else if (unit.Type == "rpcResult")
			{
				RPCResult result = DeserialiseObject<RPCResult>(unit.Data);
				RPCResultCallback callback;
				if (!_Callbacks.TryGetValue(result.Id, out callback))
					throw new NRPCException("Server provided an invalid RPC result ID");
				_Callbacks.Remove(result.Id);
				if (result.Error == null)
					callback(result.Result);
				else
					throw new NRPCException(string.Format("RPC error: {0}", result.Error));
			}
			else if (unit.Type == "error")
			{
				string message = (string)unit.Data;
				_NotificationHandler.HandleError(message);
			}
			else if (unit.Type == "ping")
				_NotificationHandler.HandlePing();
			else
				throw new NRPCException("Encountered an unknown unit type");
		}

		public void GetNewNotifications(RPCResultCallback callback)
		{
			PerformRPC(callback, GetNewNotificationsMethod);
		}

		public void GetNotificationCount(RPCResultCallback callback)
		{
			PerformRPC(callback, GetNotificationCountMethod);
		}

		public void GetNotifications(RPCResultCallback callback, long firstIndex, long lastIndex)
		{
			PerformRPC(callback, GetNotificationsMethod, firstIndex, lastIndex);
		}

		public void GenerateNotification(RPCResultCallback callback, string type, object content)
		{
			PerformRPC(callback, GenerateNotificationMethod, type, content);
		}

		private static Notification GetUninitialisedNotification(object input)
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

		private void ReadBlock()
		{
			try
			{
				int bytesRead = _Stream.Read(_ByteBuffer, 0, ByteBufferSize);
				if (bytesRead == 0)
					throw new NRPCException("The connection has been closed");

				string input = Encoding.UTF8.GetString(_ByteBuffer, 0, bytesRead);
				_Buffer += input;
			}
			catch (IOException exception)
			{
				throw new NRPCException(string.Format("IO exception: {0}", exception));
			}
		}

		// Returns null if the connection was terminated
		private string ReadUnitString()
		{
			int? unitSize = null;
			while(true)
			{
				ReadBlock();
				int offset = _Buffer.IndexOf(':');
				if (offset != -1)
				{
					string sizeString = _Buffer.Substring(0, offset);
					try
					{
						unitSize = Convert.ToInt32(sizeString);
						if (unitSize > MaximumBufferSize)
							throw new NRPCException("The server has specified an excessively large unit size value");
						_Buffer = _Buffer.Substring(offset + 1);
						break;
					}
					catch (FormatException)
					{
						throw new NRPCException("Encountered an invalid size string");
					}
				}
			}
			while (_Buffer.Length < unitSize)
				ReadBlock();
			string unitString = _Buffer.Substring(0, unitSize.Value);
			_Buffer = _Buffer.Substring(unitSize.Value);

			return unitString;
		}

		private void NotImplemented()
		{
			throw new NRPCException("This feature has not been implemented yet");
		}

		private void SendUnitString(string unitString)
		{
			string packet = unitString.Length.ToString() + ":" + unitString;
			_Stream.Write(Encoding.UTF8.GetBytes(packet));
		}

		private void SendUnit(object unit)
		{
			SendUnitString(JsonConvert.SerializeObject(unit));
		}

		private void PerformRPC(RPCResultCallback callback, string method, params object[] arguments)
		{
			var call = new RPCCall(_CallCounter, method, arguments);
			var unit = new Unit(call);
			SendUnit(unit);
			_Callbacks[_CallCounter] = callback;
			_CallCounter++;
		}
	}
}
