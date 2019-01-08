using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	internal class WorkersApiOperationElement : Element
	{
		private ScheduleDesigner _designer = null;

		public WorkersApiOperationElement(IEnvironment environment, IDomElement parent, IApiOperation operation) : base(environment, parent)
		{
			Component = operation;

			Glyph = "fal fa-exchange";
			Title = operation.Name;
			Id = operation.Id.ToString();
		}

		public override object Component { get; }

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new ScheduleDesigner(Environment, this);

				return _designer;
			}
		}
	}
}
