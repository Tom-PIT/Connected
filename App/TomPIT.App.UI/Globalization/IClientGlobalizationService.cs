using System;

namespace TomPIT.App.Globalization
{
	[Flags]
	public enum ClientGlobalizationSegment : long
	{
		None = 0,
		Characters = 1,
		ContextTransforms = 2,
		Currencies = 4,
		Delimiters = 8,
		Languages = 16,
		Layout = 32,
		ListPatterns = 64,
		LocaleDisplayNames = 128,
		MeasurementSystemNames = 256,
		Numbers = 512,
		Posix = 1024,
		Scripts = 2048,
		Territories = 4096,
		Units = 8192,
		Variants = 16384,
		Aliases = 32768,
		CalendarData = 65536,
		CalendarPreferenceData = 131072,
		CharacterFallbacks = 262144,
		CodeMappings = 524288,
		CurrencyData = 1048576,
		DayPeriods = 2097152,
		Gender = 4194304,
		LanguageData = 8388608,
		LanguageGroups = 16777216,
		LanguageMatching = 33554432,
		LikelySubtags = 67108864,
		MeasurementData = 134217728,
		MetaZones = 268435456,
		NumberingSystems = 536870912,
		Ordinals = 1073741824,
		ParentLocales = 2147483648,
		Plurals = 4294967296,
		PrimaryZones = 8589934592,
		References = 17179869184,
		TerritoryContainment = 34359738368,
		TerrirotyInfo = 68719476736,
		TimeData = 137438953472,
		UnitPreferenceData = 274877906944,
		WeekData = 549755813888,
		WindowsZones = 1099511627776,
		Rbnf = 2199023255552,
		Suppressions = 4398046511104,
		Calendar = 8796093022208,
		CalendarGeneric = 17592186044416,
		DateFields = 35184372088832,
		TimeZoneNames = 70368744177664,
		All = 140737488355327
	}

	public interface IClientGlobalizationService
	{
		string LoadData(string locale, ClientGlobalizationSegment segments);
	}
}
