using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.IO;
using TomPIT.Compilation;
using TomPIT.Services;

namespace TomPIT.Globalization
{
	internal class ClientGlobalizationService : IClientGlobalizationService
	{
		private Lazy<ConcurrentDictionary<string, string>> _locales = new Lazy<ConcurrentDictionary<string, string>>();
		private const string DefaultLocale = "en";
		private const string CldrFolder = "Assets\\Libraries\\Cldr\\0.5.0\\cldr";

		public string LoadData(string locale, ClientGlobalizationSegment segments)
		{
			var key = string.Format("{0}.{1}", locale, (long)segments);

			if (Locales.ContainsKey(key))
				return Locales[key];

			return Load(locale, segments);
		}

		public ConcurrentDictionary<string, string> Locales { get { return _locales.Value; } }

		private string Load(string locale, ClientGlobalizationSegment segments)
		{
			var main = string.Format("{0}\\{1}", MainFolder, locale);
			var supplemental = SupplementalFolder;
			var rbnf = string.Format("{0}\\{1}.json", RbnfFolder, locale);
			var segment = string.Format("{0}\\{1}", SegmentsFolder, locale);

			if (!Directory.Exists(main))
			{
				if (string.Compare(locale, DefaultLocale, true) != 0)
					return Load(DefaultLocale, segments);
			}

			var r = new JObject();
			var settings = new JsonMergeSettings
			{
				MergeArrayHandling = MergeArrayHandling.Concat,
				MergeNullValueHandling = MergeNullValueHandling.Ignore
			};

			if ((segments & ClientGlobalizationSegment.Characters) == ClientGlobalizationSegment.Characters)
				r.Merge(LoadJson(string.Format("{0}\\characters.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.ContextTransforms) == ClientGlobalizationSegment.ContextTransforms)
				r.Merge(LoadJson(string.Format("{0}\\contextTransforms.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.Currencies) == ClientGlobalizationSegment.Currencies)
				r.Merge(LoadJson(string.Format("{0}\\currencies.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.Delimiters) == ClientGlobalizationSegment.Delimiters)
				r.Merge(LoadJson(string.Format("{0}\\delimiters.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.Languages) == ClientGlobalizationSegment.Languages)
				r.Merge(LoadJson(string.Format("{0}\\languages.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.Layout) == ClientGlobalizationSegment.Layout)
				r.Merge(LoadJson(string.Format("{0}\\layout.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.ListPatterns) == ClientGlobalizationSegment.ListPatterns)
				r.Merge(LoadJson(string.Format("{0}\\listPatterns.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.LocaleDisplayNames) == ClientGlobalizationSegment.LocaleDisplayNames)
				r.Merge(LoadJson(string.Format("{0}\\localeDisplayNames.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.MeasurementSystemNames) == ClientGlobalizationSegment.MeasurementSystemNames)
				r.Merge(LoadJson(string.Format("{0}\\measurementSystemNames.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.Numbers) == ClientGlobalizationSegment.Numbers)
				r.Merge(LoadJson(string.Format("{0}\\numbers.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.Posix) == ClientGlobalizationSegment.Posix)
				r.Merge(LoadJson(string.Format("{0}\\posix.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.Scripts) == ClientGlobalizationSegment.Scripts)
				r.Merge(LoadJson(string.Format("{0}\\scripts.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.Territories) == ClientGlobalizationSegment.Territories)
				r.Merge(LoadJson(string.Format("{0}\\territories.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.Units) == ClientGlobalizationSegment.Units)
				r.Merge(LoadJson(string.Format("{0}\\units.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.Variants) == ClientGlobalizationSegment.Variants)
				r.Merge(LoadJson(string.Format("{0}\\variants.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.Aliases) == ClientGlobalizationSegment.Aliases)
				r.Merge(LoadJson(string.Format("{0}\\aliases.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.CalendarData) == ClientGlobalizationSegment.CalendarData)
				r.Merge(LoadJson(string.Format("{0}\\calendarData.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.CalendarPreferenceData) == ClientGlobalizationSegment.CalendarPreferenceData)
				r.Merge(LoadJson(string.Format("{0}\\calendarPreferenceData.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.CharacterFallbacks) == ClientGlobalizationSegment.CharacterFallbacks)
				r.Merge(LoadJson(string.Format("{0}\\characterFallbacks.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.CodeMappings) == ClientGlobalizationSegment.CodeMappings)
				r.Merge(LoadJson(string.Format("{0}\\codeMappings.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.CurrencyData) == ClientGlobalizationSegment.CurrencyData)
				r.Merge(LoadJson(string.Format("{0}\\currencyData.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.DayPeriods) == ClientGlobalizationSegment.DayPeriods)
				r.Merge(LoadJson(string.Format("{0}\\dayPeriods.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.Gender) == ClientGlobalizationSegment.Gender)
				r.Merge(LoadJson(string.Format("{0}\\gender.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.LanguageData) == ClientGlobalizationSegment.LanguageData)
				r.Merge(LoadJson(string.Format("{0}\\languageData.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.LanguageGroups) == ClientGlobalizationSegment.LanguageGroups)
				r.Merge(LoadJson(string.Format("{0}\\languageGroups.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.LanguageMatching) == ClientGlobalizationSegment.LanguageMatching)
				r.Merge(LoadJson(string.Format("{0}\\languageMatching.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.LikelySubtags) == ClientGlobalizationSegment.LikelySubtags)
				r.Merge(LoadJson(string.Format("{0}\\likelySubtags.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.MeasurementData) == ClientGlobalizationSegment.MeasurementData)
				r.Merge(LoadJson(string.Format("{0}\\measurementData.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.MetaZones) == ClientGlobalizationSegment.MetaZones)
				r.Merge(LoadJson(string.Format("{0}\\metaZones.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.NumberingSystems) == ClientGlobalizationSegment.NumberingSystems)
				r.Merge(LoadJson(string.Format("{0}\\numberingSystems.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.Ordinals) == ClientGlobalizationSegment.Ordinals)
				r.Merge(LoadJson(string.Format("{0}\\ordinals.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.ParentLocales) == ClientGlobalizationSegment.ParentLocales)
				r.Merge(LoadJson(string.Format("{0}\\parentLocales.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.Plurals) == ClientGlobalizationSegment.Plurals)
				r.Merge(LoadJson(string.Format("{0}\\plurals.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.PrimaryZones) == ClientGlobalizationSegment.PrimaryZones)
				r.Merge(LoadJson(string.Format("{0}\\primaryZones.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.References) == ClientGlobalizationSegment.References)
				r.Merge(LoadJson(string.Format("{0}\\references.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.TerritoryContainment) == ClientGlobalizationSegment.TerritoryContainment)
				r.Merge(LoadJson(string.Format("{0}\\territoryContainment.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.TerrirotyInfo) == ClientGlobalizationSegment.TerrirotyInfo)
				r.Merge(LoadJson(string.Format("{0}\\terrirotyInfo.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.TimeData) == ClientGlobalizationSegment.TimeData)
				r.Merge(LoadJson(string.Format("{0}\\timeData.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.UnitPreferenceData) == ClientGlobalizationSegment.UnitPreferenceData)
				r.Merge(LoadJson(string.Format("{0}\\unitPreferenceData.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.WeekData) == ClientGlobalizationSegment.WeekData)
				r.Merge(LoadJson(string.Format("{0}\\weekData.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.WindowsZones) == ClientGlobalizationSegment.WindowsZones)
				r.Merge(LoadJson(string.Format("{0}\\windowsZones.json", supplemental)), settings);

			if ((segments & ClientGlobalizationSegment.Rbnf) == ClientGlobalizationSegment.Rbnf)
				r.Merge(LoadJson(rbnf), settings);

			if ((segments & ClientGlobalizationSegment.Suppressions) == ClientGlobalizationSegment.Suppressions)
				r.Merge(LoadJson(string.Format("{0}\\suppressions.json", segment)), settings);

			if ((segments & ClientGlobalizationSegment.Calendar) == ClientGlobalizationSegment.Calendar)
				r.Merge(LoadJson(string.Format("{0}\\ca-gregorian.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.CalendarGeneric) == ClientGlobalizationSegment.CalendarGeneric)
				r.Merge(LoadJson(string.Format("{0}\\ca-generic.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.DateFields) == ClientGlobalizationSegment.DateFields)
				r.Merge(LoadJson(string.Format("{0}\\dateFields.json", main)), settings);

			if ((segments & ClientGlobalizationSegment.TimeZoneNames) == ClientGlobalizationSegment.TimeZoneNames)
				r.Merge(LoadJson(string.Format("{0}\\timeZoneNames.json", main)), settings);

			var content = Types.Serialize(r);

			Locales.TryAdd(string.Format("{0}.{1}", locale, (long)segments), content);

			return content;
		}

		private string MainFolder { get { return string.Format("{0}\\{1}\\main", RootFolder, CldrFolder); } }
		private string SupplementalFolder { get { return string.Format("{0}\\{1}\\supplemental", RootFolder, CldrFolder); } }
		private string RbnfFolder { get { return string.Format("{0}\\{1}\\rbnf", RootFolder, CldrFolder); } }
		private string SegmentsFolder { get { return string.Format("{0}\\{1}\\segments", RootFolder, CldrFolder); } }

		private string RootFolder { get { return Shell.GetService<IRuntimeService>().WebRoot; } }

		private JObject LoadJson(string fileName)
		{
			if (!File.Exists(fileName))
				return new JObject();

			var s = File.ReadAllText(fileName);

			return Types.Deserialize<JObject>(s);
		}
	}
}
