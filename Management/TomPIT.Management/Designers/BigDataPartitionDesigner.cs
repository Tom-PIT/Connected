using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.BigData;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Designers;
using TomPIT.Dom;
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
					_partition = GetService<IBigDataManagementService>().SelectPartition(((IPartitionConfiguration)Element.Component).Component);

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
					_files = GetService<IBigDataManagementService>().QueryFiles(Partition.Configuration);

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
			GetService<IBigDataManagementService>().FixPartition(Partition.Configuration, Partition.Name);
			return Result.SectionResult(ViewModel, Annotations.EnvironmentSection.Designer);
		}
	}
}
