using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.BigData;
using TomPIT.ComponentModel.BigData;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Dom;
using TomPIT.Management.BigData;

namespace TomPIT.Management.Designers
{
	public class BigDataPartitionDesigner : DomDesigner<DomElement>
	{
		private List<IPartitionFile> _files = null;
		private IPartition _partition = null;
		public BigDataPartitionDesigner(DomElement element) : base(element)
		{

		}
		public IPartition Partition
		{
			get
			{
				if (_partition == null)
					_partition = Environment.Context.Tenant.GetService<IBigDataManagementService>().SelectPartition(((IPartitionConfiguration)Element.Component).Component);

				return _partition;
			}
		}

		public override string View => "~/Views/Ide/Designers/BigDataPartition.cshtml";
		public override object ViewModel => this;

		public List<IPartitionFile> Files
		{
			get
			{
				if (_files == null)
					_files = Environment.Context.Tenant.GetService<IBigDataManagementService>().QueryFiles(Partition.Configuration);

				return _files;
			}
		}

		public int OpenFiles => Files.Count(f => f.Status == PartitionFileStatus.Open);
		public long Count
		{
			get
			{
				if (Files == null)
					return 0;

				return Files.Sum(f => f.Count);
			}
		}
		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			Environment.Context.Tenant.GetService<IBigDataManagementService>().FixPartition(Partition.Configuration, Partition.Name);

			return Result.SectionResult(ViewModel, EnvironmentSection.Designer);
		}
	}
}
