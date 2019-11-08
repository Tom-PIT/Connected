using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.Ide.Analysis.Suggestions
{
	public class Suggestion : ISuggestion
	{
		public const int Class = 5;
		public const int Color = 19;
		public const int Constant = 14;
		public const int Constructor = 2;
		public const int Customcolor = 22;
		public const int Enum = 15;
		public const int EnumMember = 16;
		public const int Event = 10;
		public const int Field = 3;
		public const int File = 20;
		public const int Folder = 23;
		public const int Function = 1;
		public const int Interface = 7;
		public const int Keyword = 17;
		public const int Method = 0;
		public const int Module = 8;
		public const int Operator = 11;
		public const int Property = 9;
		public const int Reference = 21;
		public const int Snippet = 25;
		public const int Struct = 6;
		public const int Text = 18;
		public const int TypeParameter = 24;
		public const int Unit = 12;
		public const int Value = 13;
		public const int Variable = 4;

		private List<string> _commitCharacters = null;

		public static bool SupportsDescription(int kind)
		{
			return kind == Constant
				|| kind == Field
				|| kind == Keyword
				|| kind == Property
				|| kind == Reference
				|| kind == Snippet
				|| kind == Text
				|| kind == Value
				|| kind == Variable;
		}

		[JsonProperty(PropertyName = "label")]
		public string Label { get; set; }
		[JsonProperty(PropertyName = "kind")]
		public int Kind { get; set; }
		[JsonProperty(PropertyName = "detail")]
		public string Description { get; set; }
		[JsonProperty(PropertyName = "insertText")]
		public string InsertText { get; set; }
		[JsonProperty(PropertyName = "sortText")]
		public string SortText { get; set; }
		[JsonProperty(PropertyName = "filterText")]
		public string FilterText { get; set; }
		[JsonProperty(PropertyName = "commitCharacters")]
		public List<string> CommitCharacters
		{
			get
			{
				if (_commitCharacters == null)
					_commitCharacters = new List<string>();

				return _commitCharacters;
			}
		}
	}
}
