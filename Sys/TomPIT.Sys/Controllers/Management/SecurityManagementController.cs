﻿using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Management
{
	public class SecurityManagementController : SysController
	{
		[HttpPost]
		public PermissionValue SetPermission()
		{
			var body = FromBody();

			var evidence = body.Required<string>("evidence");
			var claim = body.Required<string>("claim");
			var schema = body.Required<string>("schema");
			var descriptor = body.Required<string>("descriptor");
			var primaryKey = body.Required<string>("primaryKey");
			var resourceGroup = body.Optional("resourceGroup", Guid.Empty);
			var component = body.Optional("component", string.Empty);

			var p = DataModel.Permissions.Select(evidence, schema, claim, primaryKey, descriptor);

			if (p == null)
			{
				DataModel.Permissions.Insert(resourceGroup, evidence, schema, claim, descriptor, primaryKey, PermissionValue.Allow, component);

				return PermissionValue.Allow;
			}
			else
			{
				var v = PermissionValue.NotSet;

				switch (p.Value)
				{
					case PermissionValue.NotSet:
						v = PermissionValue.Allow;
						break;
					case PermissionValue.Allow:
						v = PermissionValue.Deny;
						break;
					case PermissionValue.Deny:
						v = PermissionValue.NotSet;
						break;
				}

				if (v == PermissionValue.NotSet)
					DataModel.Permissions.Delete(evidence, schema, claim, primaryKey, descriptor);
				else
					DataModel.Permissions.Update(evidence, schema, claim, primaryKey, descriptor, v);

				return v;
			}
		}

		[HttpPost]
		public void Reset()
		{
			var body = FromBody();

			var primaryKey = body.Required<string>("primaryKey");
			var claim = body.Optional("claim", string.Empty);
			var schema = body.Optional("schema", string.Empty);
			var descriptor = body.Optional("descriptor", string.Empty);

			DataModel.Permissions.Reset(claim, schema, primaryKey, descriptor);
		}

		[HttpGet]
		public ImmutableList<IMembership> QueryMembership(Guid role)
		{
			return DataModel.Membership.QueryForRole(role);
		}

		[HttpPost]
		public void InsertMembership()
		{
			var b = FromBody();

			var user = b.Required<Guid>("user");
			var role = b.Required<Guid>("role");

			if (DataModel.Membership.Select(user, role) != null)
				return;

			DataModel.Membership.Insert(user, role);
		}

		[HttpPost]
		public void DeleteMembership()
		{
			var b = FromBody();

			var user = b.Required<Guid>("user");
			var role = b.Required<Guid>("role");

			DataModel.Membership.Delete(user, role);
		}

		[HttpPost]
		public Guid InsertAuthenticationToken()
		{
			var body = FromBody();

			var resourceGroup = body.Required<Guid>("resourceGroup");
			var user = body.Required<Guid>("user");
			var key = body.Required<string>("key");
			var claims = body.Required<AuthenticationTokenClaim>("claims");
			var status = body.Required<AuthenticationTokenStatus>("status");
			var validFrom = body.Optional("validFrom", DateTime.MinValue);
			var validTo = body.Optional("validTo", DateTime.MinValue);
			var startTime = body.Optional("startTime", TimeSpan.Zero);
			var endTime = body.Optional("endTime", TimeSpan.Zero);
			var ipRestrictions = body.Optional("ipRestrictions", string.Empty);
			var name = body.Required<string>("name");
			var description = body.Optional("description", string.Empty);

			return DataModel.AuthenticationTokens.Insert(resourceGroup, user, name, description, key, claims, status, validFrom, validTo,
				startTime, endTime, ipRestrictions);
		}

		[HttpPost]
		public void UpdateAuthenticationToken()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");
			var user = body.Required<Guid>("user");
			var key = body.Required<string>("key");
			var claims = body.Required<AuthenticationTokenClaim>("claims");
			var status = body.Required<AuthenticationTokenStatus>("status");
			var validFrom = body.Optional("validFrom", DateTime.MinValue);
			var validTo = body.Optional("validTo", DateTime.MinValue);
			var startTime = body.Optional("startTime", TimeSpan.Zero);
			var endTime = body.Optional("endTime", TimeSpan.Zero);
			var ipRestrictions = body.Optional("ipRestrictions", string.Empty);
			var name = body.Required<string>("name");
			var description = body.Optional("description", string.Empty);

			DataModel.AuthenticationTokens.Update(token, user, name, description, key, claims, status, validFrom, validTo,
				startTime, endTime, ipRestrictions);
		}

		[HttpPost]
		public void DeleteAuthenticationToken()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");

			DataModel.AuthenticationTokens.Delete(token);
		}
	}
}