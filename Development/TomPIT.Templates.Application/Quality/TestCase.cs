using TomPIT.Annotations;
using TomPIT.ComponentModel.Quality;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Quality
{
	[Create(DesignUtils.TestCase, nameof(Name))]
	public class TestCase : TestElement, ITestCase
	{
	}
}
