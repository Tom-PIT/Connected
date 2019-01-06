using System;

namespace TomPIT.ComponentModel
{
	public interface ITemplate : IElement
	{
		Guid TemplateBlob { get; set; }
	}
}
