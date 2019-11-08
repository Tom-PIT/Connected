using System;

namespace TomPIT.Security
{
	public class PermissionDescription : IPermissionDescription
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
	}
}
