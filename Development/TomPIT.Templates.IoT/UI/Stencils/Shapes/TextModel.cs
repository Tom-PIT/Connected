﻿using System.Linq;
using TomPIT.IoT.Models;
using TomPIT.Services;

namespace TomPIT.IoT.UI.Stencils.Shapes
{
	public class TextModel : VectorModel<Text>
	{
		private string _text = null;

		public TextModel(IExecutionContext context, IIoTElement element) : base(context, element)
		{
		}

		public string Text
		{
			get
			{
				if (_text == null && DataSource != null)
				{
					if (string.IsNullOrWhiteSpace(Stencil.DataMember))
						_text = Stencil.String;
					else
					{
						var field = DataSource.FirstOrDefault(f => string.Compare(f.Field, Stencil.DataMember, true) == 0);

						if (field == null)
							_text = string.Empty;
						else
							_text = field.Value;
					}
				}

				return _text;
			}
		}
	}
}