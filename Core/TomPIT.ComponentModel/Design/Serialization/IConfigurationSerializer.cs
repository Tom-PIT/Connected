using Newtonsoft.Json.Linq;

namespace TomPIT.Design.Serialization
{
	public interface IConfigurationSerializer
	{
		void Deserialize(JToken state);
		void Serialize(JToken state);
	}
}
