﻿using System.IO;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Design;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class ScriptBundle : ComponentCreateHandler<IScriptBundleConfiguration>
	{
		protected override string Template
		{
			get
			{
				if (Context.Tenant.GetService<IComponentService>().SelectComponent(MicroService.Token, ComponentCategories.View, ComponentName) != null)
					return "TomPIT.MicroServices.Design.CreateHandlers.Templates.ScriptBundleView.txt";
				else if (Context.Tenant.GetService<IComponentService>().SelectComponent(MicroService.Token, ComponentCategories.Partial, ComponentName) != null)
					return "TomPIT.MicroServices.Design.CreateHandlers.Templates.ScriptBundlePartial.txt";
				else
					return "TomPIT.MicroServices.Design.CreateHandlers.Templates.ScriptBundle.txt";
			}
		}
		protected override void OnInitializeNewComponent()
		{
			using var stream = GetType().Assembly.GetManifestResourceStream(Template);
			using var reader = new StreamReader(stream);
			var text = Regex.Replace(reader.ReadToEnd(), OnReplace);

			if (Instance is IScriptBundleInitializer initializer)
				Instance.Scripts.Add(initializer.CreateDefaultFile(Instance));

			Context.Tenant.GetService<IDesignService>().Components.Update(Instance);
			Context.Tenant.GetService<IDesignService>().Components.Update(Instance.Scripts[0] as IText, text);
		}
	}
}
