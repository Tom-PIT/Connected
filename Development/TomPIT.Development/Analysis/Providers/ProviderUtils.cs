namespace TomPIT.Development.Analysis.Providers
{
	internal static class ProviderUtils
	{
		public static string Attribute(string name, string value)
		{
			return string.Format("**{0}** *{1}*", name.Trim(), Escape(value));
		}

		public static string Header(string value)
		{
			return string.Format("**{0}**", Escape(value));
		}

		public static string ListItem(string value)
		{
			return string.Format("- {0}", Escape(value));
		}

		private static string Escape(string value)
		{
			value = value.Trim();

			if (value.StartsWith("["))
				return string.Format(@"\{0}", value);

			return value;
		}
	}
}
