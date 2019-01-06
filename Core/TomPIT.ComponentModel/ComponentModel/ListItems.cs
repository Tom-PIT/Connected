using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TomPIT.Annotations;

namespace TomPIT.ComponentModel
{
	public delegate void ListItemsChangedHandler<T>(T item, int index);

	[SuppressProperties("Capacity")]
	[JsonObject]
	public class ListItems<T> : IList<T>, ITypedList, IElement, ISupportInitialize where T : class
	{
		[JsonProperty]
		private IList<T> _items = null;

		private PropertyDescriptorCollection _col = null;
		public event ListItemsChangedHandler<T> Added;
		public event ListItemsChangedHandler<T> Removed;

		public ListItems()
		{
			_items = new List<T>();
		}

		public void Clear()
		{
			_items.Clear();
		}

		[JsonIgnore]
		public int Count { get { return _items.Count; } }

		public int IndexOf(T item)
		{
			return _items.IndexOf(item);
		}

		public void Insert(int index, T value)
		{
			_items.Insert(index, value);

			TrySetParent(value);

			OnInsertComplete(index, value);
		}

		public void Add(T item)
		{
			_items.Add(item);

			TrySetParent(item);

			OnAddComplete(item);
		}

		public void AddRange(IEnumerable<T> items)
		{
			if (items == null)
				return;

			foreach (var i in items)
				Add(i);
		}

		public bool Remove(T item)
		{
			int idx = _items.IndexOf(item);

			if (idx < 0)
				return false;

			_items.Remove(item);

			OnRemoveComplete(idx, item);

			return true;
		}

		public T this[int index]
		{
			get { return _items[index]; }
			set { _items[index] = value; }
		}

		protected virtual void OnInsertComplete(int index, T value)
		{
			Added?.Invoke(value, index);
		}

		protected virtual void OnAddComplete(T value)
		{
			Added?.Invoke(value, _items.Count - 1);
		}

		protected virtual void OnRemoveComplete(int index, T value)
		{
			Removed?.Invoke(value, index);
		}

		public virtual PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			if (_col == null)
				_col = TypeDescriptor.GetProperties(typeof(T));

			return _col;
		}

		public string GetListName(PropertyDescriptor[] listAccessors)
		{
			return GetType().Name;
		}

		public void Reset()
		{
			Id = Guid.NewGuid();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[JsonIgnore]
		public IElement Parent { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[KeyProperty]
		[JsonProperty]
		public Guid Id { get; set; } = Guid.NewGuid();

		[JsonIgnore]
		public bool IsReadOnly => false;

		public static implicit operator List<T>(ListItems<T> items)
		{
			if (items == null)
				return null;

			var r = new List<T>();

			if (items.Count > 0)
				r.AddRange(items);

			return r;
		}

		public static implicit operator ListItems<T>(List<T> items)
		{
			if (items == null)
				return null;

			var r = new ListItems<T>();

			if (items.Count > 0)
				r.AddRange(items);

			return r;
		}

		private void TrySetParent(object instance)
		{
			if (!(instance is IElement))
				return;

			var parent = instance.GetType().GetProperty("Parent");

			if (parent != null && parent.CanWrite)
				parent.SetValue(instance, this);
		}

		public void RemoveAt(int index)
		{
			_items.RemoveAt(index);
		}

		public bool Contains(T item)
		{
			return _items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_items.CopyTo(array, arrayIndex);
		}

		public void BeginInit()
		{

		}

		public void EndInit()
		{
			if (_items != null)
			{
				foreach (var i in _items)
				{
					if (!(i is IElement e))
						continue;

					TrySetParent(e);
					OnAddComplete(i);
				}
			}
		}
	}
}
