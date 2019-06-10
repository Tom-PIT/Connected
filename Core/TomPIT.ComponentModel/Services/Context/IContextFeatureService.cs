using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Features;

namespace TomPIT.Services.Context
{
	public interface IContextFeatureService
	{
		IViewFeature GetView(string featureSet, string feature);
		ISettingFeature GetSetting(string featureSet, string feature);

		bool Exists(string featureSet, string feature);
	}
}
