﻿using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.Ide.Dom;
using TomPIT.Management.BigData;

namespace TomPIT.Management.Dom
{
	public class BigDataPartitionElement : DomElement
	{
		private List<IPartitionFile> _files = null;
		public BigDataPartitionElement(IDomElement parent, IPartition partition) : base(parent)
		{
			Partition = partition;

			Title = partition.Name;
			Id = partition.Configuration.ToString();
		}

		private IPartition Partition { get; set; }

		public override int ChildrenCount => Files.Count;
		public override bool HasChildren => Files.Count > 0;

		public override void LoadChildren()
		{
			foreach (var file in Files)
				Items.Add(new BigDataFileElement(this, file));
		}

		public override void LoadChildren(string id)
		{
			var file = Files.FirstOrDefault(f => string.Compare(f.FileName.ToString(), id, true) == 0);

			if (file != null)
				Items.Add(new BigDataFileElement(this, file));
		}
		public List<IPartitionFile> Files
		{
			get
			{
				if (_files == null)
					_files = Environment.Context.Tenant.GetService<IBigDataManagementService>().QueryFiles(Partition.Configuration);

				return _files;
			}
		}
	}
}