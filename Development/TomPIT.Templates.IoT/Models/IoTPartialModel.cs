﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.ComponentModel.UI;
using TomPIT.IoT.UI;
using TomPIT.IoT.UI.Stencils;
using TomPIT.Models;
using TomPIT.Services;

namespace TomPIT.IoT.Models
{
	public class IoTPartialModel : ExecutionContext, IRuntimeModel, IForwardDataProvider
	{
		private List<string> _stencils = null;
		private List<IIoTElement> _targetStencils = null;
		private List<IIoTFieldState> _state = null;

		public IComponent Component { get; set; }
		public IView ViewConfiguration => View as IView;
		protected Controller Controller { get; private set; }
		public ActionContext ActionContext { get; }
		public IIoTView View { get; private set; }

		public List<string> Stencils
		{
			get
			{
				if (_stencils == null)
					_stencils = new List<string>();

				return _stencils;
			}
		}

		public List<IIoTElement> TargetStencils
		{
			get
			{
				if (_targetStencils == null)
				{
					_targetStencils = new List<IIoTElement>();
					var v = View as IoTView;

					foreach (var i in Stencils)
					{
						var stencil = v.Elements.FirstOrDefault(f => string.Compare(f.Name, i, true) == 0);

						if (stencil == null)
							throw new RuntimeException(string.Format("{0} ({1})", SR.ErrIoTStencilNotFound, i));

						TargetStencils.Add(stencil);
					}
				}

				return _targetStencils;
			}
		}

		public void Initialize(Controller controller, IMicroService microService)
		{
			Controller = controller;
			MicroService = microService;
		}

		public void Initialize(Controller controller, string microService, string view)
		{
			Controller = controller;

			var ms = GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, microService));

			View = GetService<IComponentService>().SelectConfiguration(ms.Token, "View", view) as IIoTView;

			if (View == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrViewNotFound, view));

			Component = GetService<IComponentService>().SelectComponent(ViewConfiguration.Component);

			Initialize(null, ms);
		}

		public IEnumerable<ValidationResult> Validate()
		{
			return null;
		}

		public string Title { get; protected set; }
		public IModelNavigation Navigation => null;

		public void VerifyData(JObject data)
		{
			var cs = data.Required<string>("$checkSum");

			data.Remove("$checkSum");

			var checkSum = Encoding.UTF8.GetString(LZ4.LZ4Codec.Unwrap(Convert.FromBase64String(cs)));
			var content = JsonConvert.SerializeObject(data);

			using (var md = MD5.Create())
			{
				var hash = GetHash(md, Encoding.UTF8.GetBytes(content));
				var comparer = StringComparer.OrdinalIgnoreCase;

				if (comparer.Compare(hash, checkSum) != 0)
					throw new RuntimeException(SR.ErrDataCorrupted);
			}
		}

		private string GetHash(MD5 hash, byte[] value)
		{
			var data = hash.ComputeHash(value);
			var sb = new StringBuilder();

			for (var i = 0; i < data.Length; i++)
				sb.Append(data[i].ToString("x2"));

			return sb.ToString();
		}

		public List<IIoTFieldState> ForwardState
		{
			get
			{
				if (_state == null)
					_state = new List<IIoTFieldState>();

				return _state;
			}
		}
	}
}