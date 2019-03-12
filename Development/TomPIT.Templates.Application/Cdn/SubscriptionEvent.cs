using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Application.Cdn
{
	[Create("Event", nameof(Name))]
	[DefaultEvent(nameof(Invoke))]
	public class SubscriptionEvent : ConfigurationElement, ISubscriptionEvent
	{
		private IServerEvent _invoke = null;

		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		[EventArguments(typeof(SubscriptionEventInvokeArguments))]
		public IServerEvent Invoke
		{
			get
			{
				if (_invoke == null)
					_invoke = new ServerEvent { Parent = this };

				return _invoke;
			}
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? base.ToString() : Name;
		}
	}
}
