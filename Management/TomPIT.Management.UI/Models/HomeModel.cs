using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Models;
using TomPIT.Runtime;

namespace TomPIT.Management.Models
{
   public class HomeModel : IdeModelBase
   {
      protected override IDom CreateDom()
      {
         var path = string.IsNullOrWhiteSpace(Path)
             ? RequestBody?.Optional("path", string.Empty)
             : Path;


         return new Ide.Dom(this, path);
      }

      public override string Id => string.Empty;
      public override string IdeUrl => $"{Services.Routing.RootUrl}/mng";

      public override EnvironmentMode Mode => EnvironmentMode.Runtime;
      protected override void OnDatabinding()
      {
         Title = "Management";
      }
   }
}