using TomPIT.Services;

namespace TomPIT.ComponentModel.Apis
{
	public class OperationManifestArguments : OperationArguments
	{
		private OperationManifest _manifest = null;

		public OperationManifestArguments(IExecutionContext sender, IApiOperation operation) : base(sender, operation, null)
		{
		}

		public OperationManifest Manifest
		{
			get
			{
				if (_manifest == null)
				{
					var api = Operation.Closest<IApi>().ComponentName(Connection);
					var ms = GetService<IMicroServiceService>().Select(Operation.MicroService(Connection));

					_manifest = new OperationManifest
					{
						Name = $"{api}/{Operation.Name}",
						MicroService=ms.Name
					};
				}

				return _manifest;
			}
		}
	}
}
