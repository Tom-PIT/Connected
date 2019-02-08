using TomPIT.IoT.Models;
using TomPIT.Services;

namespace TomPIT.IoT.UI.Stencils.Shapes
{
	public class TextModel : VectorModel<Text>
	{
		public TextModel(IExecutionContext context, IIoTElement element) : base(context, element)
		{
		}
	}
}
