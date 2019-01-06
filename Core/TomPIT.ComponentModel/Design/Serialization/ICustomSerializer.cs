using Newtonsoft.Json.Linq;

namespace TomPIT.Design.Serialization
{
	public interface ICustomSerializer
	{
		void Deserialize(JToken state);
		void Serialize(JToken state);
	}
}
