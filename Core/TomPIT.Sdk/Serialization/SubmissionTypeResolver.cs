using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TomPIT.Reflection;

namespace TomPIT.Serialization
{
   public enum ResolverStrategy
   {
      Complete = 1,
      SkipRoot = 2
   }

   internal sealed class SubmissionTypeResolver
   {
      public List<Type> SubmissionTypes { get; private set; }
      private List<Type> References { get; set; }

      public SubmissionTypeResolver()
      {
         SubmissionTypes = new();
         References = new();
      }

      public bool IsCompatible(object instance)
      {
         if (instance is null)
            return true;

         var instanceType = instance.GetType();

         if (instanceType.IsCollection())
         {
            if (SubmissionTypes.Any())
               return false;
         }
         else
         {
            if (!IsSubmissionType(instance.GetType()))
               return true;

            foreach (var type in SubmissionTypes)
            {
               if (!IsSameSubmission(instance, type))
                  return false;
            }
         }

         return true;
      }

      public void Resolve(Type type, ResolverStrategy strategy)
      {
         SubmissionTypes = new();
         References = new();

         switch (strategy)
         {
            case ResolverStrategy.Complete:
               ResolveType(type);
               break;
            case ResolverStrategy.SkipRoot:
               var members = type.GetMembers();

               foreach (var member in members)
               {
                  if (member is EventInfo ei)
                     ResolveType(ei.EventHandlerType);
                  else if (member is FieldInfo fi)
                     ResolveType(fi.FieldType);
                  else if (member is PropertyInfo pi)
                     ResolveType(pi.PropertyType);
               }
               break;
            default:
               break;
         }
      }

      private void ResolveType(Type type)
      {
         if (References.Contains(type))
            return;

         References.Add(type);

         if (type.IsPrimitive || !string.IsNullOrWhiteSpace(type.Namespace) && !type.IsCollection())
            return;

         if (IsSubmissionType(type))
         {
            if (!SubmissionTypes.Contains(type))
               SubmissionTypes.Add(type);

            ResolveChildren(type);

            return;
         }
         else if (type.IsCollection())
         {
            if (type.IsGenericType)
            {
               foreach (var parameterType in type.GetGenericArguments())
                  ResolveType(parameterType);
            }
            else
            {
               var elementType = type.GetElementType();

               ResolveType(elementType);
            }
         }
         else
         {
            if (type.IsGenericType)
            {
               var types = type.GetGenericArguments();

               for (var i = 0; i < types.Length; i++)
                  ResolveType(types[i]);
            }
            else
               ResolveChildren(type);
         }
      }

      private void ResolveChildren(Type type)
      {
         var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

         foreach (var member in members)
         {
            if (member is FieldInfo field)
               ResolveType(field.FieldType);
            else if (member is PropertyInfo property)
               ResolveType(property.PropertyType);
         }
      }

      private static bool IsSubmissionType(Type type)
      {
         return string.IsNullOrEmpty(type.Namespace) && string.IsNullOrEmpty(type.Assembly.Location);
      }

      private static bool IsSameSubmission(object instance, Type type)
      {
         return IsSameSubmission(instance.GetType(), type);
      }

      private static bool IsSameSubmission(Type instanceType, Type type)
      {
         return string.IsNullOrWhiteSpace(type.Namespace) && string.IsNullOrWhiteSpace(instanceType.Namespace)
            && string.Equals(type.Assembly.FullName, instanceType.Assembly.FullName, StringComparison.Ordinal);
      }
   }
}
