using System.Data;

namespace TomPIT.Data
{
	public interface ICommandTextParameter
	{
		string Name { get; set; }
		DbType Type { get; set; }
		object Value { get; set; }
	}
}
