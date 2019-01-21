using Newtonsoft.Json;

namespace TomPIT.Deployment
{
	public class TableColumn
	{

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "dataType")]
		public string DataType { get; set; }
		[JsonProperty(PropertyName = "identity")]
		public bool Identity { get; set; }
		[JsonProperty(PropertyName = "isNullable")]
		public bool IsNullable { get; set; }
		[JsonProperty(PropertyName = "defaultValue")]
		public string DefaultValue { get; set; }
		[JsonProperty(PropertyName = "ordinal")]
		public int Ordinal { get; set; }
		[JsonProperty(PropertyName = "characterMaximumLength")]
		public int CharacterMaximumLength { get; set; }
		[JsonProperty(PropertyName = "characterOctetLength")]
		public int CharacterOctetLength { get; set; }
		[JsonProperty(PropertyName = "numericPrecision")]
		public int NumericPrecision { get; set; }
		[JsonProperty(PropertyName = "numericPrecisionRadix")]
		public int NumericPrecisionRadix { get; set; }
		[JsonProperty(PropertyName = "numericScale")]
		public int NumericScale { get; set; }
		[JsonProperty(PropertyName = "dateTimePrecision")]
		public int DateTimePrecision { get; set; }
		[JsonProperty(PropertyName = "characterSetName")]
		public string CharacterSetName { get; set; }
	}
}
