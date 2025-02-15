﻿using System;

namespace TomPIT.Security
{
	public class AuthorizationArgs : EventArgs
	{
		private AuthorizationSchema _schema = null;

		public AuthorizationArgs(Guid user, string claim, string primaryKey, string permissionDescriptor)
		{
			User = user;
			PrimaryKey = primaryKey;
			Claim = claim;
			PermissionDescriptor = permissionDescriptor;
		}

		public IAuthorizationSchema Schema
		{
			get
			{
				if (_schema == null)
					_schema = new AuthorizationSchema();

				return _schema;
			}
		}


		public Guid User { get; }
		public string PrimaryKey { get; }
		public string Claim { get; }
		public string PermissionDescriptor { get; }
	}
}
