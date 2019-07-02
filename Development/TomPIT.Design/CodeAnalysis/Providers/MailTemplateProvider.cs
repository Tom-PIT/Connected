using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class MailTemplateProvider : ComponentAnalysisProvider
	{
		public MailTemplateProvider(IExecutionContext context) : base(context)
		{

		}

		protected override string ComponentCategory => "MailTemplate";
		protected override bool FullyQualified => true;
	}
}
