using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;

namespace TomPIT.Reflection
{
	internal class ManifestType : ManifestMember, IManifestType
	{
		private List<IManifestMember> _members = null;

		public List<IManifestMember> Members => _members ??= new List<IManifestMember>();

		public static ManifestType FromScript(ITenant tenant, IScriptManifestType type, IScriptManifest manifest)
		{
			var result = new ManifestType
			{
				Documentation = type.Documentation,
				Name = QualifiedTypeName(type),
				Type = type.Type
			};

			FillProperties(tenant, result, type, manifest);

			return result;
		}

		private static string QualifiedTypeName(IScriptManifestType type)
        {
			if (string.IsNullOrWhiteSpace(type.ContainingType))
				return type.Name;

			return $"{type.ContainingType}.{type.Name}";
        }

		private static void ResolveBaseProperties(ITenant tenant, ManifestType type, IScriptManifestType scriptType, IScriptManifest manifest)
		{
			if (string.IsNullOrEmpty(scriptType.BaseType))
				return;

			var reference = manifest.SymbolReferences.FirstOrDefault(f => string.Compare(f.Key.Identifier, scriptType.BaseTypeMetaDataName, false) == 0);

			if (reference.Value is null )
			{
				//TODO: try framework types
				return;
			}

			if (manifest.GetPointer(reference.Key.Address) is not IScriptManifestPointer pointer)
				return;

			if (tenant.GetService<IDiscoveryService>().Manifests.SelectScript(pointer.MicroService, pointer.Component, pointer.Element) is not IScriptManifest baseManifest)
				return;

			if (baseManifest.DeclaredTypes.FirstOrDefault(f => string.Compare(scriptType.BaseTypeMetaDataName, f.MetaDataName) == 0) is not IScriptManifestType baseType)
				return;

			FillProperties(tenant, type, baseType, baseManifest);
		}

		private static void FillProperties(ITenant tenant, ManifestType type, IScriptManifestType scriptType, IScriptManifest manifest)
		{
			foreach (var member in scriptType.Members)
			{
				if (FromScript(member) is IManifestMember instance)
					type.Members.Add(instance);
			}

			if (string.Compare(scriptType.BaseType, "object", true) != 0)
				ResolveBaseProperties(tenant, type, scriptType, manifest);
		}

		private static ManifestMember FromScript(IScriptManifestMember member)
		{
			ManifestMember result = null;

			if (member is IScriptManifestProperty property && property.IsPublic)
				result = FromScript(property);
			else if (member is IScriptManifestField field && field.IsPublic)
				result = FromScript(field);
			else
				return null;

			if (result is IManifestAttributeMember attributeMember)
				SetAttributes(attributeMember, member as IScriptManifestAttributeMember);

			return result;
		}

		private static ManifestProperty FromScript(IScriptManifestProperty property)
		{
			return new ManifestProperty
			{
				CanRead = property.CanRead,
				CanWrite = property.CanWrite,
				IsPublic = property.IsPublic,
				Name = property.Name,
				Type = property.Type,
				Documentation = property.Documentation
			};
		}

		private static ManifestField FromScript(IScriptManifestField field)
		{
			return new ManifestField
			{
				IsConstant = field.IsConstant,
				IsPublic = field.IsPublic,
				Name = field.Name,
				Type = field.Type,
				Documentation = field.Documentation
			};
		}
		private static void SetAttributes(IManifestAttributeMember member, IScriptManifestAttributeMember script)
		{
			if (script is null)
				return;

			foreach (var attribute in script.Attributes)
			{
				member.Attributes.Add(new ManifestAttribute
				{
					Description = attribute.Description,
					Documentation = attribute.Documentation,
					IsValidation = attribute.IsValidation,
					Name = attribute.Name,
					Type = attribute.Type
				});
			}
		}
	}
}
