using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.IoC
{
	public class UIDependencyDescriptor : IUIDependencyDescriptor
	{
		[CIP(CIP.PartialProvider)]
		public string Partial { get; set; }
		public string Selector { get; set; }
		public UIInjectionMode InjectionMode { get; set; } = UIInjectionMode.Append;
		public int Order { get; set; }
		public string ContainerCss { get; set; }
	}
}
