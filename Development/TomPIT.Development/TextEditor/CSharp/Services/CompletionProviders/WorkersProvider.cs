using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class WorkersProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.HostedWorker;
		protected override bool IncludeReferences => true;
	}
}
