using System;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Compilation;
using TomPIT.Middleware;

namespace TomPIT.Ide.Analysis.Analyzers
{
	public abstract class CSharpCodeAnalyzer<T> : CSharpCodeAnalyzerBase where T : IAnalysisArgs
	{
		private Type _argumentsType = null;
		private SourceText _source = null;

		public CSharpCodeAnalyzer(IMiddlewareContext context, T e) : base(context as IMicroServiceContext)
		{
			Args = e;
		}

		protected T Args { get; }

		protected override Type ArgumentsType
		{
			get
			{
				if (_argumentsType == null)
				{
					if (Args.ArgumentsType == null)
						_argumentsType = typeof(ScriptGlobals<>).MakeGenericType(typeof(IMiddlewareContext));
					else
						_argumentsType = typeof(ScriptGlobals<>).MakeGenericType(Args.ArgumentsType);
				}

				return _argumentsType;
			}
		}

		public override SourceText SourceCode
		{
			get
			{
				if (_source == null)
					_source = SourceText.From(Args.Text);

				return _source;
			}
		}
	}
}
