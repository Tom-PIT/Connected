using System;
using TomPIT.Globalization;

namespace TomPIT.Globalization
{
	internal class Language : ILanguage
	{
		public int Lcid { get; set; }
		public string Name { get; set; }
		public LanguageStatus Status { get; set; }
		public string Mappings { get; set; }
		public Guid Token { get; set; }
	}
}
