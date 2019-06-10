using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;

namespace TomPIT.Services.Context
{
	internal class ContextFeatureService : ContextClient, IContextFeatureService
	{
		public ContextFeatureService(IExecutionContext context) : base(context)
		{
		}

		public bool Exists(string featureSet, string feature)
		{
			return SelectFeature(featureSet, feature) != null;
		}

		public ISettingFeature GetSetting(string featureSet, string feature)
		{
			return SelectFeature(featureSet, feature) as ISettingFeature;
		}

		public IViewFeature GetView(string featureSet, string feature)
		{
			return SelectFeature(featureSet, feature) as IViewFeature;
		}

		private IFeature SelectFeature(string featureSet, string feature)
		{
			var set = SelectFeatureSet(featureSet);

			if (set == null)
				return null;

			var target = set.Features.FirstOrDefault(f => string.Compare(f.Name, feature, true) == 0);

			if (target == null || !target.Enabled)
				return null;

			return target;
		}

		private IFeatureSet SelectFeatureSet(string featureSet)
		{
			var component = Context.Connection().GetService<IComponentService>().SelectComponent("FeatureSet", featureSet);

			if (component == null)
				return null;

			return Context.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) as IFeatureSet;
		}
	}
}
