﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;

namespace TomPIT.Globalization
{
	internal class StringsCache : ClientRepository<IStringTable, string>
	{
		public StringsCache(ISysConnection connection) : base(connection, "servicestrings")
		{
			Connection.GetService<IComponentService>().ComponentChanged += OnComponentChanged;
			Connection.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
		}

		private void OnConfigurationChanged(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, "StringTable", true) != 0)
				return;

			Invalidate(e.MicroService, e.Component);
		}

		private void OnComponentChanged(ISysConnection sender, ComponentEventArgs e)
		{
			if (string.Compare(e.Category, "StringTable", true) != 0)
				return;

			Invalidate(e.MicroService, e.Component);
		}

		private void Invalidate(Guid microService, Guid component)
		{
			var ms = Connection.GetService<IMicroServiceService>().Select(microService);
			var c = Connection.GetService<IComponentService>().SelectComponent(component);

			var key = $"{ms.Name}{c.Name}";

			Remove(key);
		}

		public string GetString(string microService, string stringTable, string key, int lcid, bool throwException)
		{
			var cacheKey = $"{microService}{stringTable}";
			var table = Get(cacheKey);

			if(table == null)
			{
				var ms = Connection.GetService<IMicroServiceService>().Select(microService);

				if (ms == null)
					throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({microService})");

				table = Connection.GetService<IComponentService>().SelectConfiguration(ms.Token, "StringTable", stringTable) as IStringTable;

				Set(key, table, TimeSpan.Zero);
			}

			var str = table.Strings.FirstOrDefault(f => string.Compare(f.Key, key, true) == 0);

			if (str == null)
			{
				if (throwException)
					throw new RuntimeException($"{SR.ErrStringResourceNotFound} ({microService}/{stringTable}/{key})");
				else
					return null;
			}

			if (!str.IsLocalizable)
				return str.DefaultValue;

			if (lcid == 0)
				lcid = CultureInfo.InvariantCulture.LCID;

			if (lcid == CultureInfo.InvariantCulture.LCID)
				return str.DefaultValue;

			var translation = str.Translations.FirstOrDefault(f => f.Lcid == lcid);

			return translation == null
				? GetFallbackString(str, lcid)
				: translation.Value;

		}

		private string GetFallbackString(IStringResource str, int lcid)
		{
			var culture = CultureInfo.GetCultureInfo(lcid);

			if (culture.LCID == CultureInfo.InvariantCulture.LCID || culture.Parent == null)
				return str.DefaultValue;

			var parent = culture.Parent;

			var translation = str.Translations.FirstOrDefault(f => f.Lcid == parent.LCID);

			if (translation != null)
				return translation.Value;

			return GetFallbackString(str, parent.LCID);
		}
	}
}