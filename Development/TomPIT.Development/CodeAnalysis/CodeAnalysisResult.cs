using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Design;

namespace TomPIT.Development.CodeAnalysis
{
	internal class CodeAnalysisResult : ICodeAnalysisResult
	{
		public string Text {get;set;}
		public string Description {get;set;}
		public string Value {get;set;}
	}
}
