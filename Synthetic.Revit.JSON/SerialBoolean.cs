using System;
using System.Collections.Generic;
using System.Linq;


using Autodesk.DesignScript.Runtime;

namespace Synthetic.Serialize.Revit
{
    [SupressImportIntoVM]
    public class SerialBoolean : SerialObject
    {
        public bool boolean { get; set; }

        public SerialBoolean () { }

        public SerialBoolean (bool boolean)
        {
            this.boolean = boolean;
        }
    }
}
