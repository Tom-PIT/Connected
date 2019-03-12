using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class SubscriptionEventProvider : ComponentAnalysisProvider
	{
		public SubscriptionEventProvider(IExecutionContext context) : base(context)
		{

		}

		protected override string ComponentCategory => "Subscription";

		protected override void ProvideComponentLiterals(CodeAnalysisArgs e, List<ICodeAnalysisResult> items, IComponent component)
		{
			if (!(Context.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is ISubscription config))
				return;

			foreach(var ev in config.Events)
			{
				if (string.IsNullOrWhiteSpace(ev.Name))
					continue;

				var text = string.Format("{0}/{1}", component.Name, ev.Name);

				items.Add(new CodeAnalysisResult(text, text, null));
			}
		}
	}
}
