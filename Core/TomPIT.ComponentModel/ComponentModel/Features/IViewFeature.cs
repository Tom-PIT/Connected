using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel.Features
{
	public interface IViewFeature : IFeature
	{
		string Url { get; }
	}
}
