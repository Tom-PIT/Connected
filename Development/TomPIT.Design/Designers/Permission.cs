using System;
using TomPIT.Security;

namespace TomPIT.Design.Designers
{
	internal class Permission : IPermission
	{
		public Guid Evidence { get; set; }
		public string Schema { get; set; }
		public string Claim { get; set; }
		public string Descriptor { get; set; }
		public string PrimaryKey { get; set; }
		public PermissionValue Value { get; set; }
		public Guid ResourceGroup { get; set; }
		public string Component { get; set; }
	}
}
