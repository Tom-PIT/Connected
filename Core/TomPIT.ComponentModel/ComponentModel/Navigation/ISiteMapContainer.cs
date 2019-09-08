using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Navigation
{
	public interface ISiteMapContainer : ISiteMapElement
	{
		string Key { get; }
		ConnectedList<ISiteMapRoute, ISiteMapContainer> Routes { get; }
	}
}
