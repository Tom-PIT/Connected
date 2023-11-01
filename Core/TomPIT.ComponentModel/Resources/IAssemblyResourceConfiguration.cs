namespace TomPIT.ComponentModel.Resources;
public interface IAssemblyResourceConfiguration : ITextConfiguration, IMultiFileElement
{
	AccessModifier AccessModifier { get; }
	string Namespace { get; }
}
