using TomPIT.BigData;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Dom;

namespace TomPIT.Management.Dom
{
	internal class BigDataFileElement : DomElement
	{
		public BigDataFileElement(IDomElement parent, IPartitionFile file) : base(parent)
		{
			File = file;

			Title = File.FileName.ToString();
			Id = File.FileName.ToString();
		}

		private IPartitionFile File { get; set; }
	}
}
