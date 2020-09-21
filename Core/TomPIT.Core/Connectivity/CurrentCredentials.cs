using System;

namespace TomPIT.Connectivity
{
	internal class CurrentCredentials : Credentials, ICurrentCredentials
	{
		public Guid Token { get; set; }
	}
}
