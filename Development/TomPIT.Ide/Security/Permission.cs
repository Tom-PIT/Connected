using System;
using TomPIT.Security;

namespace TomPIT.Ide.Security
{
	internal class Permission : IPermission
	{
		public string Evidence { get; set; }
		public string Schema { get; set; }
		public string Claim { get; set; }
		public string Descriptor { get; set; }
		public string PrimaryKey { get; set; }
		public PermissionValue Value { get; set; }
		public Guid ResourceGroup { get; set; }
		public string Component { get; set; }
	}
}
