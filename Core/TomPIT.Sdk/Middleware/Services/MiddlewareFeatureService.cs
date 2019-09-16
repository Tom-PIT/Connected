using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareFeatureService : MiddlewareObject, IMiddlewareFeatureService
	{
		public MiddlewareFeatureService(IMiddlewareContext context) : base(context)
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

		private IFeatureSetConfiguration SelectFeatureSet(string featureSet)
		{
			var component = Context.Tenant.GetService<IComponentService>().SelectComponent("FeatureSet", featureSet);

			if (component == null)
				return null;

			return Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) as IFeatureSetConfiguration;
		}
	}
}
