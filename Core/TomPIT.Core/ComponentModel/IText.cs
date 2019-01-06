using System;

namespace TomPIT.ComponentModel
{
	public interface IText : IElement
	{
		Guid TextBlob { get; set; }
	}
}
