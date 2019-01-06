using System.ComponentModel;

namespace TomPIT.Data
{
	public class PrimaryKeyRecord : IPrimaryKeyRecord
	{
		[Browsable(false)]
		public int Id { get; set; }
	}
}
