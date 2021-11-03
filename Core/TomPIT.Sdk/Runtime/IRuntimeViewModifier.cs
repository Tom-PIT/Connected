using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Runtime
{
    public interface IRuntimeViewModifier
    {
        int Priority { get; }
       
        public ViewPreRenderModificationArguments PreRenderView(ViewPreRenderModificationArguments state);
        public string PostRenderView(ViewPostRenderModificationArguments args);
      
        public PartialViewPreRenderModificationArguments PreRenderPartialView(PartialViewPreRenderModificationArguments state);
        public string PostRenderPartialView(PartialViewPostRenderModificationArguments args);
    }
}
