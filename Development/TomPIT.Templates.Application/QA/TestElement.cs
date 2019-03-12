using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.QA;

namespace TomPIT.Application.QA
{
	public abstract class TestElement : ConfigurationElement, ITestElement
	{
		private IServerEvent _prepare = null;
		private IServerEvent _complete = null;
		private IServerEvent _error = null;

		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		public string Name { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(true)]
		public bool Enabled { get; set; } = true;
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(TestErrorBehavior.Stop)]
		public TestErrorBehavior ErrorBehavior { get; set; } = TestErrorBehavior.Stop;

		[EventArguments(typeof(TestEventArguments))]
		public IServerEvent Prepare
		{
			get
			{
				if (_prepare == null)
					_prepare = new ServerEvent { Parent = this };

				return _prepare;
			}
		}

		[EventArguments(typeof(TestEventArguments))]
		public IServerEvent Complete
		{
			get
			{
				if (_complete == null)
					_complete = new ServerEvent { Parent = this };

				return _complete;
			}
		}

		[EventArguments(typeof(TestEventArguments))]
		public IServerEvent Error
		{
			get
			{
				if (_error == null)
					_error = new ServerEvent { Parent = this };

				return _error;
			}
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? base.ToString() : Name;
		}
	}
}
