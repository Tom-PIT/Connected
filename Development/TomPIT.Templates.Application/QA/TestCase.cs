using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.QA;

namespace TomPIT.Application.QA
{
	[Create("TestCase", nameof(Name))]
	[DefaultEvent(nameof(Invoke))]
	public class TestCase : TestElement, ITestCase
	{
		private IServerEvent _invoke = null;

		[EventArguments(typeof(TestEventArguments))]
		public IServerEvent Invoke
		{
			get
			{
				if (_invoke == null)
					_invoke = new ServerEvent { Parent = this };

				return _invoke;
			}
		}
	}
}
