namespace TomPIT.UI.Theming.Engine
{
	using System.Collections.Generic;

	public interface ILessEngine
    {
        string TransformToCss(string source, string fileName);
        void ResetImports();
        IEnumerable<string> GetImports();
        bool LastTransformationSuccessful { get; }

        string CurrentDirectory { get; set; }
    }
}