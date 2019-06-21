using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel.QA
{
	public class TestEventArguments : DataModelContext
	{
		private Generator _generator = null;

		public TestEventArguments(IExecutionContext sender) : base(sender)
		{
		}

		public Generator Generator
		{
			get
			{
				if (_generator == null)
					_generator = new Generator();

				return _generator;
			}
		}
	}
}
