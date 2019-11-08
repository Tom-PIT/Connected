using Newtonsoft.Json.Serialization;

namespace TomPIT.Serialization
{
	internal class SerializationResolver : DefaultContractResolver
	{
		public SerializationResolver()
		{
			NamingStrategy = new CamelCaseNamingStrategy();
		}
	}
}