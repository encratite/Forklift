using System.IO;
using System.Xml.Serialization;

namespace Forklift
{
	public class Serializer<Type>
	{
		private XmlSerializer _SerialiserObject;
		private string _Path;

		public Serializer(string path)
		{
			_SerialiserObject = new XmlSerializer(typeof(Type));
			_Path = path;
		}

		public void Store(Type input)
		{
			StreamWriter stream = new StreamWriter(_Path);
			_SerialiserObject.Serialize(stream, input);
			stream.Close();
		}

		public Type Load()
		{
			StreamReader stream = new StreamReader(_Path);
			Type output = (Type)_SerialiserObject.Deserialize(stream);
			stream.Close();
			return output;
		}
	}
}
