using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class QueueWorkerProvider : ComponentAnalysisProvider
	{
		public QueueWorkerProvider(IExecutionContext context) : base(context)
		{

		}

		protected override string ComponentCategory => "Queue";
		protected override bool FullyQualified => true;
	}
}
