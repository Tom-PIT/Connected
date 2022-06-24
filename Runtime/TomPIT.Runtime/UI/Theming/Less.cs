namespace TomPIT.UI.Theming
{
	using System;
	using TomPIT.UI.Theming.Configuration;

	public static class Less
	{
		public static string Parse(string less)
		{
			return Parse(less, LessConfiguration.GetDefault());
		}

		public static string Parse(string less, LessConfiguration config)
		{
			if (config.Web)
			{
				throw new Exception("Please use dotless.Core.LessWeb.Parse for web applications. This makes sure all web features are available.");
			}
			return new EngineFactory(config).GetEngine().TransformToCss(less, null);
		}
	}
}
