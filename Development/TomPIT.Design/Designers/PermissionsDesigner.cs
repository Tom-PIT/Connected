using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Dom;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Design.Designers
{
	public class PermissionsDesigner : DomDesigner<IPermissionElement>
	{
		private List<IPermissionSchemaDescriptor> _descriptors = null;
		private JArray _permissionSet = null;
		private List<IItemDescriptor> _claims = null;
		private List<IPermission> _permissions = null;
		private List<IItemDescriptor> _schema = null;
		private List<IAuthorizationProvider> _providers = null;

		public PermissionsDesigner(IDomElement element) : base(element as IPermissionElement)
		{
			if (Providers != null && Providers.Count > 0)
				SelectedSchema = Providers[0].Id;

			if (Claims != null && Claims.Count > 0)
				SelectedClaim = Claims[0].Value.ToString();
		}

		public override string View => "~/Views/Ide/Designers/Permissions.cshtml";
		public override object ViewModel => this;

		private List<IAuthorizationProvider> Providers
		{
			get
			{
				if (_providers == null)
					_providers = Environment.Context.Tenant.GetService<IAuthorizationService>().QueryProviders().ToList();

				return _providers;
			}
		}

		public List<IItemDescriptor> Schemas
		{
			get
			{
				if (_schema == null)
				{
					_schema = new List<IItemDescriptor>();

					foreach (var i in Providers)
						_schema.Add(new ItemDescriptor(string.Format("{0} ({1})", i.Id, Permissions.Count(f => string.Compare(f.Schema, i.Id, true) == 0 && f.Value != PermissionValue.NotSet)), i.Id));
				}

				return _schema;
			}
		}

		public bool IsEmpty { get { return Permissions.Count(f => f.Value != PermissionValue.NotSet) == 0; } }

		public List<IItemDescriptor> Claims
		{
			get
			{
				if (_claims == null)
				{
					var claims = Owner.Claims;

					_claims = new List<IItemDescriptor>();

					foreach (var i in claims)
						_claims.Add(new ItemDescriptor(string.Format("{0} ({1})", i, Permissions.Count(f => string.Compare(f.Claim, i, true) == 0 && f.Value != PermissionValue.NotSet)), i));
				}

				return _claims;
			}
		}
		public string SelectedClaim { get; private set; }
		public string SelectedSchema { get; private set; }

		public List<IPermissionSchemaDescriptor> Descriptors
		{
			get
			{
				if (_descriptors == null && !string.IsNullOrWhiteSpace(SelectedSchema))
				{
					var schema = Providers.FirstOrDefault(f => f.Id.Equals(SelectedSchema, StringComparison.OrdinalIgnoreCase));

					if (schema != null)
						_descriptors = schema.QueryDescriptors(Environment.Context);

					_descriptors = _descriptors.OrderBy(f => f.Title).ToList();
				}

				return _descriptors;
			}
		}

		public PermissionValue GetPermissionValue(IPermissionSchemaDescriptor d)
		{
			return Environment.Context.Tenant.GetService<IAuthorizationService>().GetPermissionValue(d.Id, SelectedSchema, SelectedClaim, Owner.PermissionDescriptor.Id);
		}

		private List<IPermission> Permissions
		{
			get
			{
				if (_permissions == null)
				{
					if (string.IsNullOrWhiteSpace(SelectedSchema))
						return null;

					var u = Environment.Context.Tenant.CreateUrl("Security", "SelectPermissions")
						.AddParameter("primaryKey", Owner.PrimaryKey);

					_permissions = Environment.Context.Tenant.Get<List<Permission>>(u).ToList<IPermission>();
				}

				return _permissions;
			}
		}

		public JArray PermissionSet
		{
			get
			{
				if (_permissionSet == null)
				{
					_permissionSet = new JArray();

					if (string.IsNullOrWhiteSpace(SelectedSchema) || string.IsNullOrWhiteSpace(SelectedClaim))
						return _permissionSet;

					foreach (var i in Descriptors)
					{
						var d = new JObject
						{
							{"id" ,i.Id},
							{"title" ,i.Title},
							{"description" ,i.Description},
							{"avatar" ,i.Avatar}
						};

						var value = Permissions.FirstOrDefault(f => string.Compare(f.Evidence, i.Id, true) == 0);

						d.Add("value", value == null ? PermissionValue.NotSet.ToString() : value.Value.ToString());

						_permissionSet.Add(d);
					}

				}

				return _permissionSet;
			}
		}

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "setPermission", true) == 0)
				return SetPermission(data);
			else if (string.Compare(action, "loadPermissions", true) == 0)
				return LoadPermissions(data);
			else if (string.Compare(action, "reset", true) == 0)
				return Reset(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult Reset(JObject data)
		{
			SelectedClaim = data.Required<string>("claim");
			SelectedSchema = data.Required<string>("schema");

			var u = Environment.Context.Tenant.CreateUrl("SecurityManagement", "Reset");
			var args = new JObject
			{
				{ "primaryKey", Owner.PrimaryKey }
			};

			Environment.Context.Tenant.Post(u, args);
			_permissions = null;
			return Result.ViewResult(this, "~/Views/Ide/Designers/PermissionDescriptors.cshtml");
		}

		private IDesignerActionResult LoadPermissions(JObject data)
		{
			SelectedClaim = data.Required<string>("claim");
			SelectedSchema = data.Required<string>("schema");

			return Result.ViewResult(this, "~/Views/Ide/Designers/PermissionDescriptors.cshtml");
		}

		private IDesignerActionResult SetPermission(JObject data)
		{
			var evidence = data.Required<Guid>("evidence");

			SelectedSchema = data.Required<string>("schema");
			SelectedClaim = data.Required<string>("claim");

			var u = Environment.Context.Tenant.CreateUrl("SecurityManagement", "SetPermission");
			var args = new JObject
			{
				{ "claim", SelectedClaim },
				{ "schema", SelectedSchema },
				{ "descriptor", Owner.PermissionDescriptor.Id },
				{ "primaryKey", Owner.PrimaryKey },
				{ "evidence", evidence },
				{ "resourceGroup", Owner.ResourceGroup },
				{ "component", Owner.PermissionComponent }
			};

			var value = Environment.Context.Tenant.Post<PermissionValue>(u, args).ToString();
			_permissions = null;
			var claimCount = Permissions.Count(f => string.Compare(f.Claim, SelectedClaim, true) == 0 && f.Value != PermissionValue.NotSet);
			var schemaCount = Permissions.Count(f => string.Compare(f.Schema, SelectedSchema, true) == 0 && f.Value != PermissionValue.NotSet);

			return Result.JsonResult(this, new JObject{
				{ "value", value },
				{ "claimCount", claimCount.ToString() },
				{ "schemaCount", schemaCount.ToString() }
			});
		}

		public bool SupportsInherit { get { return Owner.SupportsInherit; } }
		public override bool SupportsChaining => false;
	}
}
