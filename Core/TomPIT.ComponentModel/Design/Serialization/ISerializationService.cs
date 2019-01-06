using System;

namespace TomPIT.Design.Serialization
{
	public interface ISerializationService
	{
		object Deserialize(byte[] state, Type type);
		byte[] Serialize(object component);

		T Clone<T>(object instance);
		void RegisterReplacement(string obsolete, string replacement);
	}
}
