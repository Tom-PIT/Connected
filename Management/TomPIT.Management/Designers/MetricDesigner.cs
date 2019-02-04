using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.ComponentModel;
using TomPIT.Diagnostics;
using TomPIT.Dom;

namespace TomPIT.Designers
{
	public class MetricDesigner : DomDesigner<DomElement>
	{
		private List<IMetric> _data = null;
		private List<ILogEntry> _log = null;

		public MetricDesigner(DomElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Metric.cshtml";
		public override object ViewModel => this;

		private IMetricConfiguration Configuration { get { return Element.Value as IMetricConfiguration; } }
		private Guid ElementId
		{
			get
			{
				if (!(Configuration.Parent is IConfiguration))
					return Configuration.Parent.Id;

				return Guid.Empty;
			}
		}

		private Guid Session { get; set; }
		public List<IMetric> Data
		{
			get
			{
				if (_data == null)
				{

					_data = GetService<IMetricManagementService>().Query(DateTime.UtcNow.Date, Configuration.Configuration().Component, ElementId).OrderByDescending(f => f.Start).ToList();

					var success = Data.Where(f => f.Result == SessionResult.Success);

					if (success.Count() > 0)
					{
						Min = success.Min(f => f.Duration());
						Max = success.Max(f => f.Duration());
						Avg = success.Average(f => f.Duration());

						if (Data.Count > 0)
							SuccessRate = (double)success.Count() / Data.Count;
					}

					ConsumptionIn = Data.Sum(f => f.ConsumptionIn);
					ConsumptionOut = Data.Sum(f => f.ConsumptionOut);
				}

				return _data;
			}
		}

		public double TotalDuration { get { return Data.Sum(f => f.Duration()); } }
		public double Min { get; private set; } = 0;
		public double Avg { get; private set; } = 0;
		public double Max { get; private set; } = 0;
		public double SuccessRate { get; private set; } = 0;
		public double ConsumptionIn { get; private set; } = 0;
		public double ConsumptionOut { get; private set; } = 0;

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "clear", true) == 0)
				return Clear();
			else if (string.Compare(action, "refresh", true) == 0)
				return Refresh();
			else if (string.Compare(action, "log", true) == 0)
				return LoadLog(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult Refresh()
		{
			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/MetricData.cshtml");
		}

		private IDesignerActionResult LoadLog(JObject data)
		{
			Session = data.Required<Guid>("id");

			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/MetricLog.cshtml");
		}

		private IDesignerActionResult Clear()
		{
			GetService<IMetricManagementService>().Clear(Configuration.Configuration().Component, ElementId);
			_data = null;

			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/MetricData.cshtml");
		}

		public List<ILogEntry> Log
		{
			get
			{
				if (_log == null)
					_log = GetService<ILoggingManagementService>().Query(Session).OrderBy(f => f.Created).ToList();

				return _log;
			}
		}
	}
}
