using System;

namespace TomPIT.Globalization
{
	public enum LanguageStatus
	{
		Hidden = 0,
		Visible = 1
	}

	public interface ILanguage
	{
		int Lcid { get; }
		string Name { get; }
		LanguageStatus Status { get; }
		string Mappings { get; }
		Guid Token { get; }
	}
}