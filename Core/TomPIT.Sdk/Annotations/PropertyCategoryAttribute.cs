using System;

namespace TomPIT.Annotations
{
	public class PropertyCategoryAttribute : Attribute
	{
		public const string CategoryDesign = "CatDesign";
		public const string CategoryRouting = "CatRouting";
		public const string CategoryAppearance = "CatAppearance";
		public const string CategoryBehavior = "CatBehavior";
		public const string CategoryData = "CatData";
		public const string CategoryGlobalization = "CatGlobalization";
		public const string CategorySecurity = "CatSecurity";
		public const string CategoryCollaboration = "CatCollaboration";
		public const string CategoryDiagnostic = "CatDiagnostic";

		public PropertyCategoryAttribute(string category)
		{
			Category = category;
		}

		public string Category { get; }
	}
}
