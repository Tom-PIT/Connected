﻿using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Dom;
using TomPIT.Reporting.Security;
using TomPIT.Security;

namespace TomPIT.Reporting.Design.Dom
{
	internal class ReportElement : ComponentPermissionElement
	{
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

		public ReportElement(IDomElement parent, IComponent component) : base(parent, component)
		{
		}

		public override List<string> Claims
		{
			get
			{
				if (_claims == null)
				{
					_claims = new List<string>
					{
						TomPIT.Claims.AccessReport
					};
				}

				return _claims;
			}
		}

		public override IPermissionDescriptor PermissionDescriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new ReportPermissionDescriptor();

				return _descriptor;
			}
		}

		public override bool SupportsInherit => true;

		public override bool Commit(object component, string property, string attribute)
		{
			return base.Commit(component, property, attribute);
		}
	}
}