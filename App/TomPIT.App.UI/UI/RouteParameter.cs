namespace TomPIT.App.UI
{
	internal class RouteParameter
	{
		public RouteParameter(string value)
		{
			Parse(value);
		}

		private void Parse(string value)
		{
			if (!IsParameter(value))
				return;

			value = value.Substring(1, value.Length - 2);

			if (value.StartsWith("?"))
			{
				value = value.Substring(1);
				Name = value;
				IsOptional = true;
			}

			if (value.Contains("="))
			{
				var tokens = value.Split('=');

				Name = tokens[0];
				DefaultValue = tokens[1];
			}
		}

		public string DefaultValue { get; private set; }
		public bool IsOptional { get; private set; }
		public string Name { get; private set; }

		public static bool IsParameter(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return false;

			return value.StartsWith("{") && value.EndsWith("}");
		}
	}
}
