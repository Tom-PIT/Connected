using TomPIT.Models;

namespace TomPIT.Models
{
	public class StatusModel : ShellModel
	{
		public string StatusTitle { get { return Title; } set { Title = value; } }
		public string Message { get; set; }
		public string Description { get; set; }

		public bool TryAgainEnabled { get; set; } = true;
	}
}
