using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Navigation
{
	public interface ISiteMapAuthorizationElement
	{
		bool Authorize(Guid user);
	}
}
