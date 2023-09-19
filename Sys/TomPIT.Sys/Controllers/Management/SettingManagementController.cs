using Microsoft.AspNetCore.Mvc;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Management
{
    public class SettingManagementController : SysController
    {
        [HttpPost]
        public void Update()
        {
            var body = FromBody();

            var name = body.Required<string>("name");
            var value = body.Optional("value", string.Empty);
            var type = body.Optional("type", string.Empty);
            var primaryKey = body.Optional("primaryKey", string.Empty);
            var nameSpace = body.Optional("nameSpace", string.Empty);

            DataModel.Settings.Update(name, nameSpace, type, primaryKey, value);
        }

        [HttpPost]
        public void Delete()
        {
            var body = FromBody();

            var name = body.Required<string>("name");
            var type = body.Optional("type", string.Empty);
            var primaryKey = body.Optional("primaryKey", string.Empty);
            var nameSpace = body.Optional("nameSpace", string.Empty);

            DataModel.Settings.Delete(name, nameSpace, type, primaryKey);
        }
    }
}
