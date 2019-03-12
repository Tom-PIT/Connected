using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.QA;

namespace TomPIT.Application.QA
{
	public class TestSuite : ComponentConfiguration, ITestSuite
	{
		private ListItems<ITestScenario> _scenarios = null;
		private IServerEvent _prepare = null;
		private IServerEvent _complete = null;
		private IServerEvent _error = null;

		[Items("TomPIT.Application.Design.Items.TestScenariosCollection, TomPIT.Application.Design")]
		public ListItems<ITestScenario> Scenarios
		{
			get
			{
				if (_scenarios == null)
					_scenarios = new ListItems<ITestScenario> { Parent = this };

				return _scenarios;
			}
		}
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
	}
}
