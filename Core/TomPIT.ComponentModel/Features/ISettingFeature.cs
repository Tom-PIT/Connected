using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel.Features
{
	public interface ISettingFeature : IFeature
	{
		string Value { get; }
	}
}
