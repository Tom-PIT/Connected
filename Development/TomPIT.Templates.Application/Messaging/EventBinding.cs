using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Messaging;
using TomPIT.MicroServices.Design;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Messaging
{
	[Create(DesignUtils.EventBinding)]
	public class EventBinding : ConfigurationElement, IEventBinding
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(DesignUtils.EventItems)]
		public string Event { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Event) ? GetType().ShortName() : Event;
		}
	}
}
