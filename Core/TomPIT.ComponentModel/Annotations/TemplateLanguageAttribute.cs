using System;

namespace TomPIT.Annotations
{
	public class TemplateLanguageAttribute : Attribute
	{
		public TemplateLanguageAttribute(string language)
		{
			Language = language;
		}

		public string Language { get; }
	}
}
