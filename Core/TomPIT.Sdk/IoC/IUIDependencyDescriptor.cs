namespace TomPIT.IoC
{
	public enum UIInjectionMode
	{
		Before = 1,
		Prepend = 2,
		After = 3,
		Append = 4,
		Replace = 5
	}
	public interface IUIDependencyDescriptor
	{
		string Partial { get; set; }
		string Selector { get; set; }
		UIInjectionMode InjectionMode { get; set; }
		int Order { get; set; }
	}
}
