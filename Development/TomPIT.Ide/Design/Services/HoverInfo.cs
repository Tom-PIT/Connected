using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.Design.Services
{
	internal class HoverInfo : IHoverInfo
	{
		private List<IHoverLine> _content = null;
		[JsonProperty(PropertyName = "range")]
		public IRange Range
		{
			get; set;
		}
		[JsonProperty(PropertyName = "contents")]
		public List<IHoverLine> Content
		{
			get
			{
				if (_content == null)
					_content = new List<IHoverLine>();

				return _content;
			}
		}
	}
}
