using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.Reflection
{
	internal class ScriptMetaDataDeserializer
	{
		public void Deserialize(IScriptManifest manifest, byte[] state)
		{
			var metaData = CreateMetaData(state);

			foreach(var type in manifest.DeclaredTypes)
			{
				if (metaData.Documentation.Members.FirstOrDefault(f => string.Compare(f.Name, type.Name, false) == 0) is not ScriptMemberDocumentation member)
					continue;

				type.Documentation = member.Documentation;

				foreach(var child in type.Members)
				{
					if (member.Members.FirstOrDefault(f => string.Compare(f.Name, child.Name, false) == 0) is ScriptMemberDocumentation childMember)
						child.Documentation = childMember.Documentation;
				}
			}
		}

		private ScriptMetaData CreateMetaData(byte[] state)
		{
			var result = new ScriptMetaData();
			var container = Serializer.Deserialize<JObject>(Encoding.UTF8.GetString(state));

			DeserializeDocumentation(container, result);

			return result;
		}

		private void DeserializeDocumentation(JObject container, ScriptMetaData metaData)
		{
			if (container.Optional(SerializationOptions.DocumentationProperty, (JObject)null) is not JObject docs)
				return;

			if (docs.Optional(SerializationOptions.MetaDataDeclaredTypes, (JArray)null) is not JArray types)
				return;

			foreach(var type in types)
			{
				if (type is not JObject jtype)
					continue;

				var member = new ScriptMemberDocumentation
				{
					Name = jtype.Optional(SerializationOptions.MetaDataName, string.Empty),
					Documentation = jtype.Optional(SerializationOptions.MetaDataDoc, string.Empty)
				};

				metaData.Documentation.Members.Add(member);

				if (jtype.Optional(SerializationOptions.MetaDataMembers, (JArray)null) is not JArray jmembers)
					continue;

				foreach(var jmember in jmembers)
				{
					if (jmember is not JObject jchild)
						continue;

					member.Members.Add(new ScriptMemberDocumentation
					{
						Name = jchild.Optional(SerializationOptions.MetaDataName, string.Empty),
						Documentation = jchild.Optional(SerializationOptions.MetaDataDoc, string.Empty)
					});
				}
			}
		}
	}
}
