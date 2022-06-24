namespace TomPIT.UI.Theming.Input
{
	public interface IFileReader
    {
        byte[] GetBinaryFileContents(string fileName);

        string GetFileContents(string fileName);

        bool DoesFileExist(string fileName);

        bool UseCacheDependencies { get; }
    }
}