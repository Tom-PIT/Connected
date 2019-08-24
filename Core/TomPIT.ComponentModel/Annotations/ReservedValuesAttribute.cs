using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace TomPIT.Annotations
{
	public class ReservedValuesAttribute : ValidationAttribute
	{
		public ReservedValuesAttribute(params string[] values)
		{
			var sb = new StringBuilder();

			foreach (var i in values)
				sb.AppendFormat("{0},", i);

			Values = sb.ToString().TrimEnd(',');
		}

		public ReservedValuesAttribute(string values)
		{
			Values = values;
		}

		public string Values { get; }
		public override string FormatErrorMessage(string name)
		{
			return string.Format("{0} '{1}'", SR.ValReservedValue, Values);
		}

		public override bool IsValid(object value)
		{
			if (value == null)
				return true;

			return !Tokens.Contains(value.ToString().ToLowerInvariant());
		}

		private string[] Tokens
		{
			get
			{
				var invalidValues = Values.ToLowerInvariant().Split(',');

				for (int i = 0; i < invalidValues.Count(); i++)
					invalidValues[i] = invalidValues[i].Trim();

				return invalidValues;
			}
		}
	}
}
