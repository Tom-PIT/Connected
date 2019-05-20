using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace TomPIT.Compilation
{
	internal class SerializationResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var props = base.CreateProperties(type, memberSerialization);

			foreach (var i in props)
				i.Ignored = false;

			return props;
		}
	}
}