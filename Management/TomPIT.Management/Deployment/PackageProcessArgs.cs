using System.ComponentModel;

namespace TomPIT.Deployment
{
	public enum PackageEntity
	{
		Folder = 1,
		Component = 2,
		DatabaseTable = 3,
		DatabaseView = 4,
		DatabaseRoutine = 5
	}

	public class PackageProcessArgs : CancelEventArgs
	{
		public PackageProcessArgs(PackageEntity entity, string id)
		{
			Entity = entity;
			Id = id;
		}

		public PackageEntity Entity { get; }
		public string Id { get; }
	}
}
