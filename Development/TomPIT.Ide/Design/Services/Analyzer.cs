using Microsoft.CodeAnalysis.Text;
using System;
using TomPIT.Compilation;
using TomPIT.Services;

namespace TomPIT.Design.Services
{
	public abstract class Analyzer<T> : AnalyzerBase where T : IAnalysisArgs
	{
		private Type _argumentsType = null;
		private SourceText _source = null;

		public Analyzer(IExecutionContext context, T e) : base(context)
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
						_argumentsType = typeof(ScriptGlobals<>).MakeGenericType(typeof(IExecutionContext));
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
				{
					//if (Args.Configuration is IPartialSourceCode)
					//{
					//	var container = Args.Configuration.Closest<ISourceCodeContainer>();

					//	if (container != null)
					//	{
					//		var refs = container.References(Args.Configuration as IPartialSourceCode);

					//		if (refs != null && refs.Count > 0)
					//		{
					//			var sb = new StringBuilder();

					//			//sb.AppendLine();

					//			foreach (var i in refs)
					//			{
					//				if (!string.IsNullOrWhiteSpace(i))
					//					sb.AppendFormat("#load \"${0}/{1}\"", Args.Component.Token, i);
					//			}

					//			sb.AppendLine();

					//			_source = SourceText.From(string.Format("{0}{1}", sb.ToString(), Args.Text));
					//		}
					//	}
					//}

					//if (_source == null)
					_source = SourceText.From(Args.Text);
				}

				return _source;
			}
		}
	}
}
