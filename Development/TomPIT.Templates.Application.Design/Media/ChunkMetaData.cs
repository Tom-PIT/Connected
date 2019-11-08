namespace TomPIT.MicroServices.Design.Media
{
	internal class ChunkMetaData
	{
		public string UploadId { get; set; }
		public string FileName { get; set; }
		public long FileSize { get; set; }
		public long Index { get; set; }
		public long TotalCount { get; set; }
	}
}
