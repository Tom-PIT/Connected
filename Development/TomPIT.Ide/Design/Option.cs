using Newtonsoft.Json;

namespace TomPIT.ComponentModel.Data
{
	public class Option
	{
		public Option(string text, string value)
		{
			Text = text;
			Value = value;
		}

		public Option()
		{

		}

		public Option(string text)
		{
			Text = text;
		}

		[JsonProperty(PropertyName = "value")]
		public string Value { get; set; }

		[JsonProperty(PropertyName = "text")]
		public string Text { get; set; }
	}
}
