using TomPIT.Development;

namespace TomPIT.Design
{
	internal class RepositoriesEndpoint : IRepositoriesEndpoint
	{
		public string Name { get; set; }

		public string Url { get; set; }

		public string UserName { get; set; }

		public byte[] Password { get; set; }
	}
}
