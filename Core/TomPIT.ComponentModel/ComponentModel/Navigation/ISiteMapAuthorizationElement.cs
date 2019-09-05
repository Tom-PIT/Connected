using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel.Navigation
{
	public interface ISiteMapAuthorizationElement
	{
		bool Authorize(Guid user);
	}
}
