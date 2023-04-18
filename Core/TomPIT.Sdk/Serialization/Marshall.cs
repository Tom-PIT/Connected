using Newtonsoft.Json.Linq;

using System;
using System.Collections;
using System.Linq;
using System.Reflection;

using TomPIT.Compilation;
using TomPIT.Data;
using TomPIT.Reflection;

namespace TomPIT.Serialization
{
   public static class Marshall
   {
      public static R Convert<R>(object value)
      {
         if (value == null)
            return Types.Convert<R>(value);

         if (NeedsMarshalling(value, typeof(R)))
         {
            if (value is DataEntity de && typeof(R).ImplementsInterface<DataEntity>())
            {
               var rtype = typeof(R);
               var instance = rtype.IsValueType ? rtype.DefaultValue() as IDataEntity : rtype.CreateInstance() as IDataEntity;

               instance.Deserialize(de);

               return (R)instance;
            }
            else
               return Serializer.Deserialize<R>(Serializer.Serialize(value));
         }
         else if (typeof(R) == typeof(JArray))
         {
            if (value is JArray)
               return (R)value;

            return Types.Convert<R>(Serializer.Deserialize<JArray>(Serializer.Serialize(value)));
         }
         else if (typeof(R) == typeof(JObject))
         {
            if (value is JObject)
               return (R)value;

            return Types.Convert<R>(Serializer.Deserialize<JObject>(Serializer.Serialize(value)));
         }

         return Types.Convert<R>(value);
      }

      public static object Convert(object value, Type destinationType)
      {
         if (value == null)
            return Types.Convert(value, destinationType);

         if (NeedsMarshalling(value, destinationType))
         {
            if (value is DataEntity de && destinationType.ImplementsInterface<DataEntity>())
            {
               var rtype = destinationType;
               var instance = rtype.IsValueType ? rtype.DefaultValue() as IDataEntity : rtype.CreateInstance() as IDataEntity;

               instance.Deserialize(de);

               return instance;
            }
            else
               return Serializer.Deserialize(Serializer.Serialize(value), destinationType);
         }
         else if (destinationType == typeof(JArray))
         {
            if (value is JArray)
               return value;

            return Types.Convert(Serializer.Deserialize<JArray>(Serializer.Serialize(value)), destinationType);
         }
         else if (destinationType == typeof(JObject))
         {
            if (value is JObject)
               return value;

            return Types.Convert(Serializer.Deserialize<JObject>(Serializer.Serialize(value)), destinationType);
         }

         return Types.Convert(value, destinationType);
      }

      public static bool NeedsMarshalling(object instance, Type type, ResolverStrategy strategy = ResolverStrategy.Complete)
      {
         if (type is null)
            return false;

         var resolver = new SubmissionTypeResolver();

         resolver.Resolve(type, strategy);

         return !resolver.IsCompatible(instance);
      }

      public static void Merge(object source, object destination)
      {
         if (source is null || destination is null)
            return;

         var destinationProperties = destination.GetType().GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);
         var sourceProperties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);

         foreach (var property in destinationProperties)
         {
            var sourceProperty = sourceProperties.FirstOrDefault(f => string.Equals(f.Name, property.Name, StringComparison.Ordinal));

            sourceProperty ??= sourceProperties.FirstOrDefault(f => string.Equals(f.Name, property.Name, StringComparison.OrdinalIgnoreCase));
             
            if (sourceProperty is null)
               continue;

            if (property.CanWrite)
               property.SetValue(destination, Types.Convert(sourceProperty.GetValue(source), property.PropertyType));
            else
            {
               if (property.IsPrimitive())
                  continue;
               else if (property.PropertyType.IsCollection())
               {
                  throw new NotImplementedException();

                  var en = sourceProperty.GetValue(source) as IEnumerable;
                  var destinationValue = property.GetValue(destination);
                  var elementType = property.PropertyType.GetElementType();

                  if (en is null || destinationValue is null)
                     continue;

                  var enumerator = en.GetEnumerator();

                  while (enumerator.MoveNext())
                  {
                     var entity = enumerator.Current;

                     //if (NeedsMarshalling(entity, property.PropertyType))
                     //{ 
                     //   entity = Convert(entity);
                     //}
                  }
               }

               /*
					 * TODO: implement nested complex objects
					 */
               var value = property.GetValue(source);
               //Tenant.GetService<ICompilerService>().ResolveType()
               //if ()
            }
         }
      }
   }
}
