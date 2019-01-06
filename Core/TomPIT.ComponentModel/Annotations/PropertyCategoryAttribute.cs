namespace TomPIT.Annotations
{
	public class PropertyCategoryAttribute : ResourceAttribute
	{
		public const string CategoryDesign = "CatDesign";
		public const string CategoryRouting = "CatRouting";
		public const string CategoryAppearance = "CatAppearance";
		public const string CategoryBehavior = "CatBehavior";
		public const string CategoryData = "CatData";
		public const string CategoryGlobalization = "CatGlobalization";
		public const string CategorySecurity = "CatSecurity";
		public const string CategoryCollaboration = "CatCollaboration";

		public PropertyCategoryAttribute(string category) : base(category)
		{
		}
	}
}
