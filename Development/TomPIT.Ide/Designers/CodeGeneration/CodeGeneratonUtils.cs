using System;
using System.Text;

namespace TomPIT.Designers.CodeGeneration
{
	internal static class CodeGeneratonUtils
	{
		private static readonly string ValidCharacters = "abcdefghijklmnopqrstuvzwxy0123456789_";
		private static readonly string ValidStartCharacters = "abcdefghijklmnopqrstuvzwxy_";

		public static string CreateIdentifierName(string proposedName)
		{
			if (string.IsNullOrWhiteSpace(proposedName))
				return proposedName;

			var firstLetter = proposedName[0].ToString().ToUpperInvariant();

			if (!ValidStartCharacters.Contains(firstLetter.ToLowerInvariant()))
				firstLetter = "@";
			else if (!ValidStartCharacters.Contains(firstLetter.ToLowerInvariant()))
				firstLetter = string.Format("@{0}", firstLetter);

			var remaining = proposedName.Substring(1);
			var sb = new StringBuilder();

			sb.Append(firstLetter);

			foreach (var i in remaining)
			{
				if (ValidCharacters.Contains(i.ToString().ToLowerInvariant()))
					sb.Append(i);
			}

			return sb.ToString();
		}

		public static string DefaultValue(Type type)
		{
			if (type == typeof(Guid))
				return string.Format("new Guid(\"{0}\")", type.DefaultValue());
			else if (type == typeof(DateTime))
				return string.Format("DateTime.MinValue");
			else
			{
				var r = type.DefaultValue();

				if (r == null)
					return null;

				return r.ToString();
			}
		}
	}
}
