﻿using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Dom
{
    public class IntegrationElement : TomPIT.Dom.Element
    {
        public const string FolderId = "Integration";
        private IntegrationDesigner _designer = null;

        public IntegrationElement(IEnvironment environment) : base(environment, null)
        {
            Id = FolderId;
            Glyph = "fal fa-upload";
            Title = "Continuous integration";
        }

        public override IDomDesigner Designer
        {
            get
            {
                if (_designer == null)
                    _designer = new IntegrationDesigner(this);

                return _designer;
            }
        }
    }
}