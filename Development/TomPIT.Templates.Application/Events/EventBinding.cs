using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Application.Events
{
	[Create("EventBinding")]
	public class EventBinding : Element, IEventBinding
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.Application.Items.EventItems, TomPIT.Templates.Application")]
		public string Event { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Event) ? GetType().ShortName() : Event;
		}
	}
}
