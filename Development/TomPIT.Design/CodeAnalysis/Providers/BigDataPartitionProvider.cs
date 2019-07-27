using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class BigDataPartitionProvider : ComponentAnalysisProvider
	{
		public BigDataPartitionProvider(IExecutionContext context) : base(context)
		{

		}

		protected override string ComponentCategory => "BigDataPartition";
		protected override bool FullyQualified => true;
		protected override bool IncludeReferences => true;
	}
}
