using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace TomPIT.Reflection
{
	internal class ScriptManifestSerializer
	{
		private IScriptManifest Manifest { get; set; }
		public JObject Container { get; set; }
		public JObject MetaData { get; set; }

		private JObject Documentation { get; set; }
		public void Serialize(IScriptManifest manifest)
		{
			Manifest = manifest;
			Container = new JObject();
			MetaData = new JObject();
			Documentation = new JObject();
			
			MetaData.Add(SerializationOptions.DocumentationProperty, Documentation);

			Serialize();
		}

		private void Serialize()
		{
			SerializeHead();
			SerializePointers();
			SerializeSourceReferences();
			SerializeScriptReferences();
			SerializeSymbolReferences(); 
			SerializeDeclaredTypes();
		}

		private void SerializeSymbolReferences()
		{
			if (!Manifest.SymbolReferences.Any())
				return;

			var jrefs = new JArray();

			foreach (var symbol in Manifest.SymbolReferences)
			{
				var jsymbol = new JObject
				{
					{SerializationOptions.Address, symbol.Key.Address },
					{SerializationOptions.Identifier, symbol.Key.Identifier },
					{SerializationOptions.Type, (int)symbol.Key.Type },
				};

				var values = new JArray();

				foreach (var value in symbol.Value)
					values.Add(SerializeLocation(value));

				jsymbol.Add(SerializationOptions.Values, values);

				jrefs.Add(jsymbol);
			}

			Container.Add(SerializationOptions.SymbolReferences, jrefs);
		}

		private void SerializeScriptReferences()
		{
			if (!Manifest.ScriptReferences.Any())
				return;

			var jrefs = new JArray();

			foreach (var reference in Manifest.ScriptReferences)
				jrefs.Add(new JValue(reference));

			Container.Add(SerializationOptions.ScriptReferences, jrefs);
		}

		private void SerializeSourceReferences()
		{
			if (!Manifest.ResourceReferences.Any())
				return;

			var jrefs = new JArray();

			foreach (var reference in Manifest.ResourceReferences)
				jrefs.Add(new JValue(reference));

			Container.Add(SerializationOptions.ResourceReferences, jrefs);
		}

		private void SerializeDeclaredTypes()
		{
			if (!Manifest.DeclaredTypes.Any())
				return;

			var jtypes = new JArray();
			var dtypes = new JArray();

			foreach(var type in Manifest.DeclaredTypes)
			{
				var jtype = new JObject();
				var dtype = new JObject();

				SerializeMember(type, jtype, dtype);
				SerializeMembers(type, jtype, dtype);

				jtypes.Add(jtype);
				dtypes.Add(dtype);
			}

			Container.Add(SerializationOptions.DeclaredTypes, jtypes);
			Documentation.Add(SerializationOptions.MetaDataDeclaredTypes, dtypes);
		}

		private static void SerializeDocumentaton(IScriptManifestMember member, JObject container)
		{
			if (string.IsNullOrWhiteSpace(member.Documentation))
				return;

			container.Add(SerializationOptions.MetaDataName, member.Name);
			container.Add(SerializationOptions.MetaDataDoc, member.Documentation);
		}

		private static void SerializeMembers(IScriptManifestType type, JObject container, JObject doc)
		{
			if (!type.Members.Any())
				return;

			var jmembers = new JArray();
			var dmembers = new JArray();

			foreach (var member in type.Members)
			{
				var jmember = new JObject();
				var dmember = new JObject();

				SerializeMember(member, jmember, dmember);

				jmembers.Add(jmember);
				dmembers.Add(dmember);
			}

			container.Add(SerializationOptions.Members, jmembers);
			doc.Add(SerializationOptions.MetaDataMembers, dmembers);
		}

		private static void SerializeMember(IScriptManifestMember member, JObject container, JObject doc)
		{
			var props = member.GetType().GetProperties(BindingFlags.Public| BindingFlags.Instance);

			container.Add(SerializationOptions.Activator, member.GetType().Assembly.InvariantTypeName(member.GetType().FullTypeName()));

			SerializeDocumentaton(member, doc);

			foreach (var property in props)
			{
				if (string.Compare(property.Name, SerializationOptions.DocumentationProperty, false) == 0)
					continue;
				
				if (property.PropertyType.IsTypePrimitive())
					AddNullableProperty(container, property.Name, property.GetValue(member));
			}

			SerializeLocation(container, member.Location);

			if (member is IScriptManifestAttributeMember attributeMember)
				SerializeAttributes(container, attributeMember);
		}

		private static void SerializeAttributes(JObject container, IScriptManifestAttributeMember member)
		{
			if (!member.Attributes.Any())
				return;

			var jatts = new JArray();

			foreach(var attribute in member.Attributes)
			{
				var jatt = new JObject
				{
					{SerializationOptions.Name, attribute.Name },
					{SerializationOptions.Type, attribute.Type }
				};

				AddNullableProperty(jatt, SerializationOptions.Description, attribute.Description);
				AddNullableProperty(jatt, SerializationOptions.IsValidation, attribute.IsValidation);

				SerializeLocation(jatt, attribute.Location);

				jatts.Add(jatt);
			}

			container.Add(SerializationOptions.Attributes, jatts);
		}

		private void SerializeHead()
		{
			Container.Add(SerializationOptions.Id, Manifest.Address);
		}

		private void SerializePointers()
		{
			if (!Manifest.Pointers.Any())
				return;

			var pointers = new JArray();

			foreach(var pointer in Manifest.Pointers)
			{
				var jpointer = new JObject
				{
					{SerializationOptions.Id, pointer.Id }
				};

				AddNullableProperty(jpointer, SerializationOptions.MicroService, pointer.MicroService);
				AddNullableProperty(jpointer, SerializationOptions.Component, pointer.Component);
				AddNullableProperty(jpointer, SerializationOptions.Element, pointer.Element);

				pointers.Add(jpointer);
			}

			Container.Add(SerializationOptions.Pointers, pointers);
		}

		private static void SerializeLocation(JObject container, IScriptManifestSymbolLocation location)
		{
			container.Add(SerializationOptions.Location, SerializeLocation(location));
		}

		private static JObject SerializeLocation(IScriptManifestSymbolLocation location)
		{
			var jlocation = new JObject();

			AddNullableProperty(jlocation, SerializationOptions.StartCharacter, location.Start);
			AddNullableProperty(jlocation, SerializationOptions.EndCharacter, location.End);

			return jlocation;
		}

		private static void AddNullableProperty(JObject container, string name, object value)
		{
			if (value is null)
				return;

			if (value is string s)
				AddNullableProperty(container, name, s);
			else if (value is int i)
				AddNullableProperty(container, name, i);
			else if (value is short sh)
				AddNullableProperty(container, name, sh);
			else if (value is Guid g)
				AddNullableProperty(container, name, g);
			else if (value is bool b)
				AddNullableProperty(container, name, b);
			else
				container.Add(name, value.ToString());
		}

		private static void AddNullableProperty(JObject container, string name, int value)
		{
			if (value == 0)
				return;

			container.Add(name, value);
		}

		private static void AddNullableProperty(JObject container, string name, bool value)
		{
			if (!value)
				return;

			container.Add(name, value);
		}

		private static void AddNullableProperty(JObject container, string name, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return;

			container.Add(name, value);
		}

		private static void AddNullableProperty(JObject container, string name, Guid value)
		{
			if (value == Guid.Empty)
				return;

			container.Add(name, Compress(value));
		}

		private static string Compress(Guid value)
		{
			if (value == Guid.Empty)
				return null;

			return value.ToString().Replace("-", string.Empty);
		}
	}
}
