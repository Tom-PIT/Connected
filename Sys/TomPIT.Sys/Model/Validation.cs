using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TomPIT.Sys.Model
{
	internal class Validator
	{
		private List<ValidationResult> _results = null;

		public void Unique<T>(T existing, string value, string propertyName, ImmutableList<T> items)
		{
			if (string.IsNullOrWhiteSpace(value))
				return;

			foreach (var i in items)
			{
				if (i.Equals(existing))
					continue;

				var pi = i.GetType().GetProperty(propertyName);

				if (pi == null)
					throw new SysException(string.Format("{0} ({1})", SR.ErrPropertyNotFound, propertyName));

				var v = Types.Convert<string>(pi.GetValue(i));

				if (string.Compare(v, value, true) == 0)
					Results.Add(new ValidationResult(string.Format("{0} ({1})", SR.ValExists, value)));
			}
		}

		public List<ValidationResult> Results
		{
			get
			{
				if (_results == null)
					_results = new List<ValidationResult>();

				return _results;
			}
		}

		public bool IsValid { get { return Results.Count == 0; } }

		public string ErrorMessage
		{
			get
			{
				if (IsValid)
					return null;

				var sb = new StringBuilder();

				foreach (var i in Results)
					sb.AppendLine(i.ErrorMessage);

				return sb.ToString();
			}
		}
	}
}
