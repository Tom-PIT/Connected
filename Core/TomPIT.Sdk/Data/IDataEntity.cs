using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TomPIT.Data
{
	public interface IDataEntity
	{
		string Serialize();
		void Deserialize(JObject state);
		void DataSource(JObject state);

		T Evolve<T>() where T : class, IDataEntity;
		void Merge(Dictionary<string, object> properties);
	}
}
