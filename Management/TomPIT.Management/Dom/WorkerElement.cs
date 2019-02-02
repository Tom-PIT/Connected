using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Workers;
using TomPIT.Designers;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	internal class WorkerElement : Element
	{
		private IWorker _worker = null;
		private IDomDesigner _designer = null;

		public WorkerElement(IEnvironment environment, IDomElement parent, IComponent worker) : base(environment, parent)
		{
			WorkerComponent = worker;

			Glyph = "fal fa-file";
			Title = worker.Name;
			Id = worker.Token.ToString();
		}

		private IComponent WorkerComponent { get; }

		public override bool HasChildren => false;

		public IWorker Worker
		{
			get
			{
				if (_worker == null)
					_worker = Connection.GetService<IComponentService>().SelectConfiguration(WorkerComponent.Token) as IWorker;

				return _worker;
			}
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new ScheduleDesigner(this);

				return _designer;
			}
		}
	}
}
