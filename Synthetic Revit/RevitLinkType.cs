using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using RevitServices.Transactions;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RLinkType = Autodesk.Revit.DB.RevitLinkType;

using Revit.Elements;

using DynElem = Revit.Elements.Element;


namespace Synthetic.Revit
{
    public class RevitLinkTypes
    {
        internal RevitLinkTypes() { }

        public static IList<DynElem> GetRevitLinkTypes([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            IList<RevitDB.Element> elements = new RevitDB.FilteredElementCollector(document).OfClass(typeof(RLinkType)).ToElements();

            IList<DynElem> dynamoElements = new List<DynElem>();

            foreach (RevitDB.Element elem in elements)
            {
                try
                {
                    dynamoElements.Add(elem.ToDSType(true));
                }
                catch { }
            }

            return dynamoElements;
        }

        public static string LoadFrom(DynElem LinkType, string FilePath)
        {
            RLinkType rLinkType = (RLinkType)LinkType.InternalElement;

            RevitDB.ModelPath linkpath = RevitDB.ModelPathUtils
              .ConvertUserVisiblePathToModelPath(FilePath);

            RevitDB.LinkLoadResult linkLoadResult;

            linkLoadResult = rLinkType.LoadFrom(linkpath, new RevitDB.WorksetConfiguration());
            
            return linkLoadResult.ToString();
        }
    }
}
