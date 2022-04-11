namespace TomPIT.UI.Theming.Plugins
{
	public interface IPluginParameter
    {
        string Name { get; }

        bool IsMandatory { get; }

        object Value { get; }

        string TypeDescription { get; }

        void SetValue(string value);
    }
}
