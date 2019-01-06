namespace TomPIT.Models
{
	public interface IUIModel : IModel
	{
		IModelNavigation Navigation { get; }
		string Title { get; }
	}
}
