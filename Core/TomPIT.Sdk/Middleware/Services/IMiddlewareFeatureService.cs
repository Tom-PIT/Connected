using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Features;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareFeatureService
	{
		IViewFeature GetView(string featureSet, string feature);
		ISettingFeature GetSetting(string featureSet, string feature);

		bool Exists(string featureSet, string feature);
	}
}
