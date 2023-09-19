using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using TomPIT.Data.Sql;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
   internal class ApiTest : PrimaryKeyRecord, IApiTest
   {
      [Required]
      public string Title { get; set; }
      public string Description { get; set; }
      [Browsable(false)]
      public Guid Identifier { get; set; }
      public string Api { get; set; }
      public string Tags { get; set; }

      protected override void OnCreate()
      {
         base.OnCreate();

         Title = GetString("title");
         Description = GetString("description");
         Identifier = GetGuid("identifier");
         Api = GetString("api");
         Tags = GetString("tags");
      }

      public override string ToString()
      {
         return string.IsNullOrWhiteSpace(Title) ? base.ToString() : Title;
      }
   }
}
