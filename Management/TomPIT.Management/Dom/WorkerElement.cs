using TomPIT.ComponentModel;
using TomPIT.Ide;
using TomPIT.Workers;

namespace TomPIT.Dom
{
	internal class WorkerElement : Element
	{
		private IWorker _worker = null;

		public WorkerElement(IEnvironment environment, IDomElement parent, IComponent worker) : base(environment, parent)
		{
			WorkerComponent = worker;

			Glyph = "fal fa-file";
			Title = worker.Name;
			Id = worker.Token.ToString();
		}

		private IComponent WorkerComponent { get; }

		public override bool HasChildren => false;

		private IWorker Worker
		{
			get
			{
				if (_worker == null)
					_worker = Connection.GetService<IComponentService>().SelectConfiguration(WorkerComponent.Token) as IWorker;

				return _worker;
			}
		}
	}
}
