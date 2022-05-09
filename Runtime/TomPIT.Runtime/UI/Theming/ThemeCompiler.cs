using System.Text;
using System.Text.RegularExpressions;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.UI.Theming
{
	internal class ThemeCompiler : TenantObject
	{
		public ThemeCompiler(ITenant tenant, IMicroService microService, string name) : base(tenant)
		{
			MicroService = microService;
			Name = name;
		}

		private IMicroService MicroService { get; }
		private string Name { get; }

		public CompiledTheme Compile()
		{
			var files = new ThemeFilesProvider(Tenant, MicroService, Name);
			var parser = new ThemeParser(Tenant);
			var fileSet = files.CreateFileSet();

			return new CompiledTheme
			{
				Content = Minify(parser.Parse(fileSet)),
				MicroService = MicroService.Token,
				Name = Name
			};
		}

		private static string Minify(string source)
		{
			source = Regex.Replace(source, @"[a-zA-Z]+#", "#");
			source = Regex.Replace(source, @"[\n\r]+\s*", string.Empty);
			source = Regex.Replace(source, @"\s+", " ");
			source = Regex.Replace(source, @"\s?([:,;{}])\s?", "$1");
			source = source.Replace(";}", "}");
			source = Regex.Replace(source, @"([\s:]0)(px|pt|%|em)", "$1");

			// Remove comments from CSS
			source = Regex.Replace(source, @"/\*[\d\D]*?\*/", string.Empty);

			return source;
		}
	}
}
