using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Models
{
	public interface IViewModel:IRuntimeModel, IUIModel, IComponentModel
	{
		IView ViewConfiguration { get; }
	}
}
