using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using RevitServices.Transactions;

using Revit.Elements;

using RevitDB = Autodesk.Revit.DB;
using RevitElem = Autodesk.Revit.DB.Element;
using RevitElemId = Autodesk.Revit.DB.ElementId;
using RevitDoc = Autodesk.Revit.DB.Document;

namespace Synthetic.Revit
{
    public class Lines
    {
        internal Lines () {}

        public static IDictionary MergeLineStyles (RevitDB.Category FromLineStyle, RevitDB.Category ToLineStyle)
        {

        }
    }
}
