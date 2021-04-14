using System.Collections.Generic;

namespace TomPIT.Diagnostics
{
	public class ExceptionTraceDescriptor
	{
		private Dictionary<int, string> _sourceCodeLines;
		public string MicroService { get; set; }
		public string Component { get; set; }
		public string FileName { get; set; }
		public int Line { get; set; }
		public string Url { get; set; }
		public Dictionary<int, string> SourceCodeLines => _sourceCodeLines ??= new Dictionary<int, string>();

		public override string ToString()
		{
			return $"{MicroService}/{Component}/{FileName} at line {Line}";
		}
	}
}
