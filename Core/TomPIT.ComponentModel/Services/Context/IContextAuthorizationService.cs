using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel.Services.Context
{
	public interface IContextAuthorizationService
	{
		bool Authorize(string claim, string primaryKey);
		bool Authorize(string claim, string primaryKey, Guid user);
	}
}
