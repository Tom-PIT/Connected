using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;

namespace TomPIT.Reflection
{
	public enum PropertyBrowserMode
	{
		Reflection = 1,
		Editor = 2
	}
	public class PropertyBrowser
	{
		public PropertyBrowser(Type type)
		{
			Type = type;
		}

		public PropertyBrowser(object instance)
		{
			Type = instance?.GetType();
		}

		private Type Type { get; }

		public BindingFlags Flags { get; set; } = BindingFlags.Public | BindingFlags.Instance;
		public bool ReadWrite { get; set; } = false;
		public bool SerializableOnly { get; set; } = true;
		public PropertyBrowserMode Mode { get; set; } = PropertyBrowserMode.Editor;
		public List<PropertyInfo> Browse()
		{
			if (Type == null)
				return null;

			var properties = Type.GetProperties(Flags);
			var result = new List<PropertyInfo>();

			foreach (var property in properties)
			{
				if (Mode == PropertyBrowserMode.Editor)
				{
					var att = property.FindAttribute<EditorBrowsableAttribute>();

					if (att != null && att.State != EditorBrowsableState.Always)
						continue;
				}

				if (ReadWrite && !property.CanWrite)
					continue;

				if (SerializableOnly && property.FindAttribute<JsonIgnoreAttribute>() != null)
					continue;

				result.Add(property);
			}

			return result;
		}
	}
}
