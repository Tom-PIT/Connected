using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TomPIT.ComponentModel.UI
{
	internal class ViewBagDictionary : DynamicObject
	{
		private readonly Func<ViewDataDictionary> _viewDataThunk;

		public ViewBagDictionary(Func<ViewDataDictionary> viewDataThunk)
		{
			_viewDataThunk = viewDataThunk;
		}

		private ViewDataDictionary ViewData
		{
			get
			{
				ViewDataDictionary viewData = _viewDataThunk();

				return viewData;
			}
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return ViewData.Keys;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = ViewData[binder.Name];
			// since ViewDataDictionary always returns a result even if the key does not exist, always return true
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			ViewData[binder.Name] = value;
			// you can always set a key in the dictionary so return true
			return true;
		}
	}
}