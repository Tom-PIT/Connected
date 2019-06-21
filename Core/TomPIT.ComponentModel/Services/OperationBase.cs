using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Apis;
using TomPIT.Data;
using TomPIT.Services.Context;

namespace TomPIT.Services
{
	public abstract class OperationBase: IOperationBase, IUniqueValueProvider
	{
		protected OperationBase(IDataModelContext context)
		{
			Context = context;
		}

		protected OperationBase(IDataModelContext context, IApiTransaction transaction)
		{
			Context = context;
			Transaction = transaction;
		}

		protected IDataModelContext Context { get; }
		protected IApiTransaction Transaction { get; }
		public void Validate(IDataModelContext context, List<ValidationResult> results)
		{
			OnValidating(results);
		}

		protected virtual void OnValidating(List<ValidationResult> results)
		{

		}

		protected virtual List<object> OnProvideUniqueValues(string propertyName)
		{
			return null;
		}

		bool IUniqueValueProvider.IsUnique(IDataModelContext context, string propertyName)
		{
			return IsValueUnique(propertyName);
		}

		protected virtual bool IsValueUnique(string propertyName)
		{
			return true;
		}

		public void Validate()
		{
			var results = new List<ValidationResult>();
			var refs = new List<object>();

			ValidateProperties(results, this, refs);

			var sb = new StringBuilder();

			foreach (var result in results)
			{
				if (result != null)
					sb.AppendLine(result.ErrorMessage);
			}

			if (sb.Length > 0)
				throw new RuntimeException(sb.ToString());
		}

		private void ValidateProperties(List<ValidationResult> results, object instance, List<object> references)
		{
			if (instance == null || references.Contains(instance))
				return;

			references.Add(instance);

			var properties = instance.GetType().GetProperties();

			foreach (var property in properties)
			{
				if (property.GetMethod == null || !property.GetMethod.IsPublic)
					continue;

				var skipAtt = property.FindAttribute<SkipValidationAttribute>();

				if (skipAtt != null)
					continue;

				if (property.PropertyType.IsTypePrimitive())
					ValidateProperty(results, instance, property);
				else if (property.PropertyType.IsCollection())
				{
					if (!(GetValue(instance, property) is IEnumerable ien))
						continue;

					var en = ien.GetEnumerator();

					while (en.MoveNext())
					{
						if (en.Current == null)
							continue;

						ValidateProperties(results, en.Current, references);
					}
				}
				else
				{
					var value = GetValue(instance, property);

					if (value == null)
						continue;

					ValidateProperties(results, value, references);
				}
			}

			if (results.Count == 0)
			{
				if (instance is RequestEntity re)
					re.Validate(Context, results);
			}
		}

		private void ValidateProperty(List<ValidationResult> results, object instance, PropertyInfo property)
		{
			var attributes = property.GetCustomAttributes(false);

			foreach (var attribute in attributes)
			{
				if (attribute is ValidationAttribute val)
				{
					try
					{
						var serviceProvider = new ValidationServiceProvider();

						serviceProvider.AddService(typeof(IDataModelContext), Context);
						serviceProvider.AddService(typeof(IUniqueValueProvider), this);

						var ctx = new ValidationContext(instance, serviceProvider, new Dictionary<object, object>
						{
							{ "entity", this }
						})
						{
							DisplayName = property.Name,
							MemberName = property.Name
						};

						val.Validate(GetValue(instance, property), ctx);
					}
					catch (ValidationException ex)
					{
						results.Add(new ValidationResult(ex.Message, new List<string> { property.Name }));
					}
				}
			}
		}

		private object GetValue(object component, PropertyInfo property)
		{
			try
			{
				return property.GetValue(component);
			}
			catch
			{
				return null;
			}
		}

		public IApiTransaction BeginTransaction()
		{
			return new ApiTransaction(Context)
			{
				Id = Guid.NewGuid()
			};
		}

		public IApiTransaction BeginTransaction(string name)
		{
			return new ApiTransaction(Context)
			{
				Id = Guid.NewGuid(),
				Name = name
			};
		}

		public void Commit()
		{
			OnCommit();
		}

		protected virtual void OnCommit()
		{

		}

		public void Rollback()
		{
			OnRollback();
		}

		protected virtual void OnRollback()
		{

		}
	}
}
