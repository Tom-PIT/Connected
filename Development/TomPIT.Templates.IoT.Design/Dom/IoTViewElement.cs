﻿using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Dom;
using TomPIT.IoT.Security;
using TomPIT.Security;

namespace TomPIT.IoT.Dom
{
	internal class IoTViewElement : ComponentPermissionElement
	{
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

		public IoTViewElement(IDomElement parent, IComponent component) : base(parent, component)
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
						TomPIT.Claims.AccessUserInterface
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
					_descriptor = new IoTViewPermissionDescriptor();

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