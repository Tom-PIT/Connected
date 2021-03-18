using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using TomPIT.Reflection;

namespace TomPIT.Middleware
{
	public class MiddlewareValidationException : ValidationException
	{
		private string _source = null;
		private string _stackTrace = null;

		public MiddlewareValidationException(object instance, string message) : base(message)
		{
			_source = instance?.GetType().ScriptTypeName();
		}

		public override string Source { get => _source ??= base.Source; set => _source = value; }
		
		public bool Logged { get; set; }
		public override string StackTrace
		{
			get
			{
				if(_stackTrace == null)
				{
					var sb = new StringBuilder();
					var st = new StackTrace(this, true);

					if (st.FrameCount == 0)
						return base.StackTrace;

					foreach(var frame in st.GetFrames())
					{
						var method = frame.GetMethod();

						if(method == null || method.DeclaringType==null)
							continue;

						if (!method.DeclaringType.FullName.StartsWith("Submission#"))
							continue;

						var line = frame.GetFileLineNumber();
						var fileName = frame.GetFileName();

						if (string.IsNullOrEmpty(fileName))
							fileName = "?";

						sb.AppendLine($"{method.Name} in {fileName} at line {line}");
					}

					_stackTrace = sb.ToString();
				}

				return _stackTrace;
			}
		}

		public override string ToString()
		{
			return $"{Message}{System.Environment.NewLine}{StackTrace}";
		}
	}
}
