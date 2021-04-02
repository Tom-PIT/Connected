using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.Reflection
{
	internal class ScriptManifestDeserializer
	{
		public IScriptManifest Manifest { get; private set; }

		public bool IsEmpty { get; private set; }
		public void Deserialize(byte[] state)
		{
			var container = Serializer.Deserialize<JObject>(Encoding.UTF8.GetString(state));
			Manifest = new ScriptManifest();

			DeserializeHead(container);
			DeserializePointers(container);
			DeserializeSourceReferences(container);
			DeserializeScriptReferences(container);
			DeserializeSymbolReferences(container);
			DeserializeDeclaredTypes(container);
		}

		private void DeserializeDeclaredTypes(JObject container)
		{
			var types = container.Optional(SerializationOptions.DeclaredTypes, (JArray)null);

			if (types == null)
				return;

			foreach(var type in types)
			{
				if (type is not JObject jtype)
					continue;

				var instance = CreateInstance(jtype) as IManifestType;

				if (instance == null)
					continue;

				DeserializeProperties(jtype, instance);
				DeserializeMembers(jtype, instance);

				Manifest.DeclaredTypes.Add(instance);
			}
		}

		private void DeserializeMembers(JObject container, IManifestType instance)
		{
			var members = container.Optional(SerializationOptions.Members, (JArray)null);

			if (members is null)
				return;

			foreach(var member in members)
			{
				if (member is not JObject jmember)
					continue;

				var property = CreateInstance(jmember);

				if (property == null)
					continue;

				DeserializeAttributes(jmember, property);
				DeserializeProperties(jmember, property);

				instance.Members.Add(property);
			}
		}

		private void DeserializeAttributes(JObject container, IManifestMember instance)
		{
			if (instance is not IManifestAttributeMember attributeMember)
				return;

			if (container.Optional(SerializationOptions.Attributes, (JArray)null) is not JArray jattributes)
				return;

			foreach (var jattribute in jattributes)
			{
				if (jattribute is not JObject jatt)
					continue;

				var attribute = new ManifestAttribute
				{
					Description = jatt.Optional(SerializationOptions.Description, string.Empty),
					Name = jatt.Optional(SerializationOptions.Name, string.Empty),
					IsValidation = jatt.Optional(SerializationOptions.IsValidation, false),
					Type = jatt.Optional(SerializationOptions.Type, string.Empty)
				};

				attributeMember.Attributes.Add(attribute);
			}
		}

		private void DeserializeProperties(JObject container, IManifestMember instance)
		{
			var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach(var property in properties)
			{
				if (!property.PropertyType.IsTypePrimitive() || !property.CanWrite)
					continue;

				if (container.Optional<object>(property.Name, null) is not object value || value is null)
					continue;

				if (Types.TryConvertInvariant(value, out object converted, property.PropertyType))
					property.SetValue(instance, converted);
			}
		}

		private IManifestMember CreateInstance(JObject type)
		{
			var activator = type.Optional(SerializationOptions.Activator, string.Empty);

			if (activator is null)
				return null;

			var typeDefinition = TypeExtensions.GetType(activator);

			if (typeDefinition is null)
				return null;

			return TypeExtensions.CreateInstance(typeDefinition) as IManifestMember;
		}

		private void DeserializeSymbolReferences(JObject container)
		{
			var references = container.Optional(SerializationOptions.SymbolReferences, (JArray)null);

			if (references is null)
				return;

			foreach (var reference in references)
			{
				if (reference is not JObject jreference)
					continue;

				var symbolReference = new ManifestSymbolReference
				{
					Address = jreference.Optional(SerializationOptions.Address, (short)0),
					Identifier = jreference.Optional(SerializationOptions.Identifier, string.Empty),
					Type = jreference.Optional(SerializationOptions.Type, ManifestSourceReferenceType.Other),
				};

				var values = new HashSet<IManifestSymbolLocation>(new ManifestLocationComparer());

				if (jreference.Optional(SerializationOptions.Location, (JObject)null) is JObject jloc)
					DeserializeLocation(jloc, symbolReference.Location);

				DeserializeLocations(jreference, values);

				Manifest.SymbolReferences.Add(symbolReference, values);
			}
		}
		private void DeserializeLocations(JObject container, HashSet<IManifestSymbolLocation> locations)
		{
			var jvalues = container.Optional(SerializationOptions.Values, (JArray)null);

			if (jvalues is null)
				return;

			foreach(var value in jvalues)
			{
				if (value is not JObject jvalue)
					continue;

				var loc = new ManifestSymbolLocation();

				DeserializeLocation(jvalue, loc);

				locations.Add(loc);
			}
		}

		private void DeserializeLocation(JObject container, IManifestSymbolLocation location)
		{
			location.StartCharacter = container.Optional(SerializationOptions.StartCharacter, 0);
			location.StartLine = container.Optional(SerializationOptions.StartLine, 0);
			location.EndCharacter = container.Optional(SerializationOptions.EndCharacter, 0);
			location.EndLine = container.Optional(SerializationOptions.EndLine, 0);
		}

		private void DeserializeScriptReferences(JObject container)
		{
			var references = container.Optional(SerializationOptions.ScriptReferences, (JArray)null);

			if (references is null)
				return;

			foreach (var reference in references)
			{
				if (reference is not JValue value)
					continue;

				Manifest.ScriptReferences.Add((short)value);
			}
		}

		private void DeserializeSourceReferences(JObject container)
		{
			var references = container.Optional(SerializationOptions.ResourceReferences, (JArray)null);

			if (references is null)
				return;

			foreach (var reference in references)
			{
				if (reference is not JValue value)
					continue;

				Manifest.ResourceReferences.Add((short)value);
			}
		}

		private void DeserializePointers(JObject container)
		{
			var pointers = container.Optional(SerializationOptions.Pointers, (JArray)null);

			if (pointers is null)
				return;

			foreach (var pointer in pointers)
			{
				if (pointer is not JObject jpointer)
					continue;

				Manifest.Pointers.Add(new ManifestPointer
				{
					MicroService = jpointer.Optional(SerializationOptions.MicroService, Guid.Empty),
					Component = jpointer.Optional(SerializationOptions.Component, Guid.Empty),
					Element = jpointer.Optional(SerializationOptions.Element, Guid.Empty),
					Id = jpointer.Optional(SerializationOptions.Id, (short)0)
				});
			}
		}

		private void DeserializeHead(JObject container)
		{
			var address = container.Optional(SerializationOptions.Id, (short)-1);

			if (address == -1)
			{
				IsEmpty = true;
				return;
			}

			Manifest.Address = container.Optional(SerializationOptions.Id, address);
		}
	}
}
