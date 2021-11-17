using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Reports;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Runtime
{
    public abstract class RuntimeViewModifier: IRuntimeViewModifier
    {
        public virtual int Priority => 10000;

        public ViewPreRenderModificationArguments PreRenderView(ViewPreRenderModificationArguments state)
        {
            if (!CanPreRenderView(state))
                return state;

            return OnPreRenderView(state);
        }

        protected virtual ViewPreRenderModificationArguments OnPreRenderView(ViewPreRenderModificationArguments state) 
        {
            return state;
        }

        public string PostRenderView(ViewPostRenderModificationArguments args)
        {
            if (!CanPostRenderView(args))
                return args.Content;

            var parsedContent = new HtmlDocument();

            parsedContent.LoadHtml(args.Content);

            var result = OnPostRenderView(parsedContent, args);

            using var ms = new StringWriter();
            result.Save(ms);

            return ms.ToString();
        }

        protected virtual HtmlDocument OnPostRenderView(HtmlDocument content, ViewPreRenderModificationArguments args)
        {
            return content;
        }

        public PartialViewPreRenderModificationArguments PreRenderPartialView(PartialViewPreRenderModificationArguments state)
        {
            if (!CanPreRenderPartialView(state))
                return state;

            return OnPreRenderPartialView(state);
        }

        protected virtual PartialViewPreRenderModificationArguments OnPreRenderPartialView(PartialViewPreRenderModificationArguments state)
        {
            return state;
        }

        public string PostRenderPartialView(PartialViewPostRenderModificationArguments args)
        {
            if (!CanPostRenderPartialView(args))
                return args.Content;

            var parsedContent = new HtmlDocument();

            parsedContent.LoadHtml(args.Content);

            var result = OnPostRenderPartialView(parsedContent, args);

            using var ms = new StringWriter();
            result.Save(ms);

            return ms.ToString();
        }

        protected virtual HtmlDocument OnPostRenderPartialView(HtmlDocument content, PartialViewPreRenderModificationArguments args)
        {
            return content;
        }

        protected virtual bool CanPreRenderView(ViewPreRenderModificationArguments state)
        {
            return false;
        }

        protected virtual bool CanPostRenderView(ViewPostRenderModificationArguments state)
        {
            return false;
        }

        protected virtual bool CanPreRenderPartialView(PartialViewPreRenderModificationArguments state)
        {
            return false;
        }

        protected virtual bool CanPostRenderPartialView(PartialViewPostRenderModificationArguments state)
        {
            return false;
        }
    }
}
