using System.Collections.Generic;

namespace TomPIT.Reflection
{
	internal class ManifestType : ManifestMember, IManifestType
	{
		private List<IManifestMember> _members = null;

		public List<IManifestMember> Members => _members ??= new List<IManifestMember>();

		public static ManifestType FromScript(IScriptManifestType type)
		{
			var result = new ManifestType
			{
				Documentation = type.Documentation,
				Name = type.Name,
				Type = type.Type
			};

			foreach (var member in type.Members)
			{
				if (FromScript(member) is IManifestMember instance)
					result.Members.Add(instance);
			}

			return result;
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

			foreach(var attribute in script.Attributes)
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
