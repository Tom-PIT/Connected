using Newtonsoft.Json;

namespace TomPIT.Sys.SqlDeployment.DTOs;

internal class ChangeScript
{
   [JsonProperty(PropertyName = "version")]
   public string? Version { get; set; }
   [JsonProperty(PropertyName = "content")]
   public string? Content { get; set; }

   public SchemaVersion SchemaVersion => new(Version);
}
