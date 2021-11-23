namespace TomPIT.UI
{
	internal class GraphicsService : IGraphicsService
	{
		private IAvatars _avatars = null;
		private IImaging _imaging = null;

		public IAvatars Avatars => _avatars ??= new Avatars();
		public IImaging Imaging => _imaging ??= new Imaging();
	}
}
