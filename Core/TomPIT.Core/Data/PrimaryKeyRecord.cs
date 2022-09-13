using System.ComponentModel;

namespace TomPIT.Data
{
	public class PrimaryKeyRecord : IPrimaryKeyRecord
	{
		public PrimaryKeyRecord()
		{

		}

		public PrimaryKeyRecord(IPrimaryKeyRecord item)
		{
			Id = item.Id;
		}

		[Browsable(false)]
		public int Id { get; set; }
	}
}
