using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Reflection;

namespace TomPIT.Design
{
	internal class NamingService : INamingService
	{
		private const string StandardCharacters = "0123456789abcdefghijklmnoprstuvzqwyx-_";

		public string Create(string prefix, IEnumerable existingItems)
		{
			var existing = ParseExistingNames(existingItems);

			return Create(prefix, existing, true);
		}

		public string Create(Type type, IEnumerable existingItems)
		{
			var prefixAtt = type.FindAttribute<CreateAttribute>();
			var prefix = type.ShortName();

			if (prefixAtt == null || string.IsNullOrWhiteSpace(prefixAtt.Prefix))
				return prefix;

			if (prefixAtt != null && !string.IsNullOrWhiteSpace(prefixAtt.Prefix))
				prefix = prefixAtt.Prefix;

			var existing = ParseExistingNames(existingItems);

			return Create(prefix, existing, true);
		}

		public void Create(object instance, IEnumerable existingItems)
		{
			var prefixAtt = instance.GetType().FindAttribute<CreateAttribute>();

			if (prefixAtt == null || string.IsNullOrWhiteSpace(prefixAtt.Prefix) || string.IsNullOrWhiteSpace(prefixAtt.PropertyName))
				return;

			var pi = instance.GetType().GetProperty(prefixAtt.PropertyName);

			if (pi == null || !pi.CanWrite)
				return;

			var prefix = instance.GetType().ShortName();

			if (prefixAtt != null && !string.IsNullOrWhiteSpace(prefixAtt.Prefix))
				prefix = prefixAtt.Prefix;

			var existing = ParseExistingNames(existingItems);
			var cn = Create(prefix, existing, true);

			if (!string.IsNullOrWhiteSpace(cn))
			{
				if (pi != null && pi.CanWrite)
					pi.SetValue(instance, cn);
			}
		}

		private List<string> ParseExistingNames(IEnumerable items)
		{
			var names = new List<string>();

			if (items != null)
			{
				foreach (var i in items)
					names.Add(i.ToString());
			}

			return names;
		}

		public string Create(string name, IEnumerable<string> existingNames, bool standardCharactersOnly, int initialIndex)
		{
			if (standardCharactersOnly)
			{
				var sb = new StringBuilder();

				for (int i = 0; i < name.Length; i++)
				{
					if (StandardCharacters.Contains(name[i].ToString().ToLowerInvariant()))
						sb.Append(name[i]);
				}

				name = sb.ToString().Trim();
			}

			if (existingNames == null || existingNames.Count() == 0)
				return string.Format("{0}{1}", name, initialIndex);
			else
			{
				int index = initialIndex;

				while (true)
				{
					string proposedName = string.Format("{0}{1}", name, index);

					if (!existingNames.Contains(proposedName))
						return proposedName;

					index++;
					proposedName = string.Format("{0}{1}", name, index);
				}
			}
		}

		public string Create(string name, IEnumerable<string> existingNames, bool standardCharactersOnly)
		{
			return Create(name, existingNames, standardCharactersOnly, 1);
		}
	}
}
