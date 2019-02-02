using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Ide
{
	public abstract class DomBase : EnvironmentPanel, IDom
	{
		public DomBase(IEnvironment environment, string path) : base(environment)
		{
			SelectedPath = path;
		}

		protected void Initialize()
		{
			if (!string.IsNullOrWhiteSpace(SelectedPath))
				Selected = Find(SelectedPath, null);
		}

		public List<IDomElement> Query(string path, int depth)
		{
			if (string.IsNullOrWhiteSpace(path))
				return QueryRoot(depth);
			else
				return QueryChildren(path, depth);
		}

		public IDomElement Select(string path, int depth)
		{
			var parentNode = Find(path, null);

			if (parentNode == null)
				return null;

			if (parentNode.HasChildren)
			{
				parentNode.LoadChildren();

				depth--;

				if (depth > 0)
				{
					foreach (var i in parentNode.Items)
						QueryChildren(i, depth);
				}
			}

			return parentNode;
		}

		protected List<IDomElement> QueryChildren(string path, int depth)
		{
			var parentNode = Find(path, null);

			if (parentNode == null)
				return null;

			if (parentNode.HasChildren)
			{
				parentNode.LoadChildren();

				depth--;

				if (depth > 0)
				{
					foreach (var i in parentNode.Items)
						QueryChildren(i, depth);
				}
			}

			return parentNode.Items;
		}

		protected void QueryChildren(IDomElement parent, int depth)
		{
			if (!parent.HasChildren)
				return;

			depth--;

			parent.LoadChildren();

			if (depth < 1)
				return;

			foreach (var i in parent.Items)
				QueryChildren(i, depth);
		}

		protected List<IDomElement> QueryRoot(int depth)
		{
			var root = Root();

			if (depth > 0)
			{
				depth--;

				foreach (var i in root)
					QueryChildren(i, depth);
			}

			return root;
		}

		protected abstract List<IDomElement> Root();

		internal IDomElement Find(string path, IDomElement parent)
		{
			var tokens = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var id = tokens[0];

			if (parent != null)
				parent.LoadChildren(id);

			var root = parent == null ? QueryRoot(0) : parent.Items;
			var target = root.FirstOrDefault(f => Types.Compare(f.Id, id));

			if (target == null)
				return null;

			if (tokens.Length == 1)
				return target;

			return Find(path.Substring(path.IndexOf('/') + 1), target);
		}

		public List<IDomElement> CreateDomTree(string path)
		{
			return null;
		}

		public IDomElement Selected { get; private set; } = null;
		public string SelectedPath { get; protected set; } = null;
		public virtual List<IItemDescriptor> ProvideAddItems(IDomElement selection) { return null; }
	}
}
