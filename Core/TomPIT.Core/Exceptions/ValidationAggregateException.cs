using System.Collections.Generic;
using System.Text;

namespace TomPIT.Exceptions
{
	public class ValidationAggregateException : TomPITException
	{
		public ValidationAggregateException(List<string> errors)
		{
			Errors = errors;
		}

		public ValidationAggregateException(string error)
		{
			Errors = new List<string>
			{
				error
			};
		}

		public List<string> Errors { get; }

		public override string Message
		{
			get
			{
				var sb = new StringBuilder();

				foreach (var i in Errors)
					sb.AppendLine(i);

				return sb.ToString();
			}
		}
	}
}
