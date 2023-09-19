using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Exceptions;
using TomPIT.Ide;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;

namespace TomPIT.Ide
{
    public static class IdeExtensions
    {
        public static void RemoveWhere<T>(this ListItems<T> source, Func<T, bool> predicate) where T : class
        {
            var where = source.Where(predicate);

            if (where.Count() == 0)
                return;

            var arr = where.ToArray();

            for (int i = 0; i < arr.Length; i++)
                source.Remove(arr[i]);
        }

        public static bool ChildrenBrowsable(this PropertyInfo pi)
        {
            if (pi == null)
                return true;

            var browsable = pi.FindAttribute<ChildrenBrowsableAttribute>();

            return browsable == null || browsable.Browsable;
        }

        public static bool IsSuppressed(this Type type, PropertyInfo pi)
        {
            var suppresses = type.FindAttribute<SuppressPropertiesAttribute>();
            string[] tokens = null;

            if (suppresses != null && !string.IsNullOrWhiteSpace(suppresses.Properties))
            {
                tokens = suppresses.Properties.Split(',');

                if (tokens.Contains(pi.Name))
                    return true;
            }

            return false;
        }

        public static bool Commit(this IEnvironment environment, object component, string property, string attribute)
        {
            if (environment.Selection.Transaction == null)
                return false;

            var current = environment.Selection.Transaction;
            var r = false;

            while (!r)
            {
                if (current == null)
                    break;

                r = current.Commit(component, property, attribute);

                if (r)
                    break;

                current = ParentHandler(current);
            }

            return r;
        }

        private static ITransactionHandler ParentHandler(this ITransactionHandler handler)
        {
            var current = handler.Element.Parent;

            while (current != null)
            {
                if (current.Transaction != null)
                    return current.Transaction;

                current = current.Parent;
            }

            return null;
        }

        public static string SelectedPath(this IEnvironment environment)
        {
            if (environment.Dom is DomRoot)
                return ((DomRoot)environment.Dom).SelectedPath;

            return null;
        }

        public static IDomElement Selected(this IEnvironment environment)
        {
            if (environment.Dom is DomRoot)
                return ((DomRoot)environment.Dom).Selected;

            return null;
        }

        public static IDomElement GetDomElement(this IComponent component, IDomElement parent)
        {
            var type = Reflection.TypeExtensions.GetType(component.Type);

            if (type == null)
                return new TypeExceptionElement(parent, component);

            var att = type.FindAttribute<DomElementAttribute>();

            if (att == null)
                return new ComponentElement(parent, component);
            else
                return new DomElementActivator(parent, component, att).CreateInstance();
        }

        public static IDomDesigner SystemDesigner(this IDomElement sender, object instance)
        {
            if (instance == null)
                return null;

            return SystemDesigner(sender, instance.GetType());
        }

        public static IDomDesigner SystemDesigner(this IDomElement sender, PropertyInfo property)
        {
            if (property == null)
                return null;

            return SystemDesigner(sender, property.PropertyType);
        }

        private static IDomDesigner SystemDesigner(this IDomElement sender, Type type)
        {
            if (type == null)
                return null;

            if (type.IsCollection())
                return new CollectionDesigner<IDomElement>(sender);
            else if (type.IsText())
                return new TextDesigner(sender);
            else
            {
                /*
				 * TODO: try to create instance provider
				 */
                return null;
            }
        }

        public static string ResolvePath(this IEnvironment environment, Guid component, Guid element, out string eventName)
        {
            eventName = string.Empty;

            var c = environment.Context.Tenant.GetService<IComponentService>().SelectComponent(component);

            if (c == null)
                throw new TomPITException(SR.ErrComponentNotFound);

            var config = environment.Context.Tenant.GetService<IComponentService>().SelectConfiguration(c.Token);

            if (config == null)
                throw new TomPITException(string.Format("{0} ({1})", SR.ErrCannotFindConfiguration, c.Name));

            var root = environment.Dom.Root();

            if (root == null)
                return null;

            var target = Traverse(element == Guid.Empty ? component : element, root);

            if (target == null)
                return null;

            if (target.IsEvent(element))
                eventName = target.EventName(element);

            return DomQuery.Path(target);
        }

        private static IDomElement Traverse(Guid element, ListItems<IDomElement> elements)
        {
            if (elements == null || elements.Count == 0)
                return null;

            var r = elements.FirstOrDefault(f => string.Compare(f.Id, element.ToString(), true) == 0);

            if (r != null)
                return r;

            foreach (var i in elements)
            {
                if (i.IsEvent(element))
                    return i;

                i.LoadChildren();

                r = Traverse(element, i.Items);

                if (r != null)
                    return r;
            }

            return null;
        }

        public static string EventName(this IDomElement dom, Guid element)
        {
            if (dom.Value == null)
                return null;

            var properties = dom.Value.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (property.IsText())
                {
                    var propertyValue = property.GetValue(dom.Value);

                    if (propertyValue is IElement el && el.Id == element)
                        return property.Name;
                }
            }

            return null;
        }


        public static bool IsEvent(this IDomElement dom, Guid element)
        {
            if (dom.Value == null)
                return false;

            var properties = dom.Value.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (!property.IsBrowsable() || property.IsIndexer())
                    continue;

                if (property.IsText())
                {
                    var propertyValue = property.GetValue(dom.Value);

                    if (propertyValue is IElement el && el.Id == element)
                        return true;
                }
            }

            return false;
        }

        public static ITransactionResult WithData(this ITransactionResult result, JObject data)
        {
            if (result is TransactionResult r)
                r.Data = data;

            return result;
        }

        public static Guid MicroService(this IDomElement element)
        {
            var scope = DomQuery.Closest<IMicroServiceScope>(element);

            if (scope != null)
                return scope.MicroService.Token;

            return element.Environment.Context.MicroService.Token;
        }

        public static DomDesignerAttribute ResolveDesigner(this Type type, IEnvironment environment)
        {
            var mode =  environment.Mode;
            var designers = type.FindAttributes<DomDesignerAttribute>();

            if (designers.Count == 0)
                return null;

            foreach (var i in designers)
            {
                if (i.Mode.HasFlag(mode))
                    return i;
            }

            return null;
        }

        public static DomDesignerAttribute ResolveDesigner(this PropertyInfo property)
        {
            var designers = property.FindAttributes<DomDesignerAttribute>();

            if (designers.Count == 0)
                return null;

            foreach (var i in designers)
            {
                if ((i.Mode & EnvironmentMode.Design) == EnvironmentMode.Design)
                    return i;
            }

            return null;
        }

        public static string ToolboxItemHelper(this IItemDescriptor descriptor)
        {
            if (descriptor.Type == null)
                return null;

            var att = descriptor.Type.FindAttribute<ToolboxItemGlyphAttribute>();

            if (att == null)
                return null;

            return att.View;
        }

        public static void ProcessComponentCreating(IMiddlewareContext context, object instance)
        {
            var att = instance.GetType().FindAttribute<ComponentCreatingHandlerAttribute>();

            if (att != null)
            {
                var handler = att.Type == null
                    ? Reflection.TypeExtensions.GetType(att.TypeName).CreateInstance<IComponentCreateHandler>()
                    : att.Type.CreateInstance<IComponentCreateHandler>();

                if (handler != null)
                    handler.InitializeNewComponent(context, instance);
            }
        }

        public static void ProcessComponentCreated(IMiddlewareContext context, object instance)
        {
            var att = instance.GetType().FindAttribute<ComponentCreatedHandlerAttribute>();

            if (att != null)
            {
                var handler = att.Type == null
                    ? Reflection.TypeExtensions.GetType(att.TypeName).CreateInstance<IComponentCreateHandler>()
                    : att.Type.CreateInstance<IComponentCreateHandler>();

                if (handler != null)
                    handler.InitializeNewComponent(context, instance);
            }
        }

        public static string Glyph(this IComponent component, ITenant tenant)
        {
            var r = "fal fa-file";
            var ms = tenant.GetService<IMicroServiceService>().Select(component.MicroService);
            var template = tenant.GetService<IMicroServiceTemplateService>().Select(ms.Template);

            var items = template.ProvideAddItems(null);
            var item = items.FirstOrDefault(f => string.Compare(component.Type, f.Type.TypeName(), true) == 0);

            if (item != null && !string.IsNullOrWhiteSpace(item.Glyph))
                r = item.Glyph;

            return r;
        }

        public static void SetContext(this IMiddlewareObject target, IMiddlewareContext context)
        {
            var property = target.GetType().GetProperty("Context");

            if (property.SetMethod == null)
                return;

            property.SetMethod.Invoke(target, new object[] { context });
        }
    }
}
