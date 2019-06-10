using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace TomPIT
{
	internal class SerializationResolver : DefaultContractResolver
	{
		public SerializationResolver()
		{
			NamingStrategy = new CamelCaseNamingStrategy();
		}
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var props = base.CreateProperties(type, memberSerialization);

			foreach (var i in props)
				i.Ignored = false;

			return props;
		}
	}
}