using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using RevitServices.Transactions;

using Revit.Elements;
using DynaElem = Revit.Elements.Element;
using DynaCurve = Revit.Elements.CurveElement;
using DynaCategory = Revit.Elements.Category;

using RevitDB = Autodesk.Revit.DB;
using RevitElem = Autodesk.Revit.DB.Element;
using RevitElemId = Autodesk.Revit.DB.ElementId;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitCategory = Autodesk.Revit.DB.Category;
using RevitGraphicStyle = Autodesk.Revit.DB.GraphicsStyle;
using RevitLinePattern = Autodesk.Revit.DB.LinePattern;
using RevitLinePatternElem = Autodesk.Revit.DB.LinePatternElement;
using RevitLinePatternSegment = Autodesk.Revit.DB.LinePatternSegment;
using RevitLinePatternSegementType = Autodesk.Revit.DB.LinePatternSegmentType;
using RevitCollector = Autodesk.Revit.DB.FilteredElementCollector;

namespace Synthetic.Revit
{
    /// <summary>
    /// Syntehtic Wrapper for Revit LinePatterns
    /// </summary>
    public class LinePatternElement
    {
        /// <summary>
        /// Internal constructor
        /// </summary>
        internal LinePatternElement() { }

        /// <summary>
        /// Create a LinePatternElement.  If the element already exists, then get the element.
        /// </summary>
        /// <param name="Name">Name of the LinePatternElement.</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="LinePatternElement">A newly created LinePatternElement</returns>
        public static RevitLinePatternElem ByName(string Name,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            if (Name == "Solid")
            {
                string transactionName = "Create LinePatternElement " + Name;

                RevitLinePatternElem linePatternElem;

                RevitCollector collector = new RevitCollector(document);
                RevitDB.ElementClassFilter elementClassFilter =
                    new RevitDB.ElementClassFilter(typeof(RevitDB.LinePatternElement));
                linePatternElem = (RevitDB.LinePatternElement)collector.WhereElementIsNotElementType()
                    .WherePasses(elementClassFilter)
                    .FirstOrDefault(elem => elem.Name.Equals(Name));

                // If the LinePatternElement doesn't exist, make it.
                if (linePatternElem == null)
                {
                    RevitLinePattern linePattern = new RevitLinePattern(Name);
                    List<RevitLinePatternSegment> segements = new List<RevitLinePatternSegment>();
                    segements.Add(_SegmentByTypeByLength("Dash", 0.0208333333333333));
                    segements.Add(_SegmentByTypeByLength("Space", 0.0208333333333333));
                    linePattern.SetSegments(segements);

                    if (document.IsModifiable)
                    {
                        TransactionManager.Instance.EnsureInTransaction(document);
                        linePatternElem = RevitLinePatternElem.Create(document, linePattern);
                        document.Regenerate();
                        TransactionManager.Instance.TransactionTaskDone();
                    }
                    else
                    {
                        using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                        {
                            trans.Start(transactionName);
                            linePatternElem = RevitLinePatternElem.Create(document, linePattern);
                            document.Regenerate();
                            trans.Commit();
                        }
                    }
                }
                return linePatternElem;
            }
            else { return null; }
        }

        /// <summary>
        /// Creates a LinePatternElement given a name and a list of segement types.  If the LinePatternElement already exists, it is returned.
        /// </summary>
        /// <param name="Name">Name of the LinePatternElement</param>
        /// <param name="segementTypes">Dash, Space, Dot</param>
        /// <param name="segmentLengths">Lengths of the segement</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="LinePatternElement">A newly created LinePatternElement</returns>
        public static RevitLinePatternElem ByNameBySegements (string Name,
            List<string> segementTypes,
            List<double> segmentLengths,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            string transactionName = "Create LinePatternElement " + Name;

            RevitLinePatternElem linePatternElem;

            RevitCollector collector = new RevitCollector(document);
            RevitDB.ElementClassFilter elementClassFilter =
                new RevitDB.ElementClassFilter(typeof(RevitDB.LinePatternElement));
            linePatternElem = (RevitDB.LinePatternElement)collector.WhereElementIsNotElementType()
                .WherePasses(elementClassFilter)
                .FirstOrDefault(elem => elem.Name.Equals(Name));

            
                List<RevitLinePatternSegment> segements = new List<RevitLinePatternSegment>();
                var segementTypesAndLengths = segementTypes.Zip(segmentLengths, (t, l) => new { segType = t, Length = l });
                foreach (var seg in segementTypesAndLengths)
                {
                    segements.Add(_SegmentByTypeByLength(seg.segType, seg.Length));
                }

                RevitLinePattern linePattern = new RevitLinePattern(Name);
                linePattern.SetSegments(segements);

            // If there was an existing LinePatternElem, modify it.
            if (linePatternElem != null)
            {
                transactionName = "Modify LinePattern " + Name;
                if (document.IsModifiable)
                {
                    TransactionManager.Instance.EnsureInTransaction(document);
                    linePatternElem.SetLinePattern(linePattern);
                    document.Regenerate();
                    TransactionManager.Instance.TransactionTaskDone();
                }
                else
                {
                    using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                    {
                        trans.Start(transactionName);
                        linePatternElem.SetLinePattern(linePattern);
                        document.Regenerate();
                        trans.Commit();
                    }
                }
            }
            // Otherwise create a new LinePatternElem
            else 
            {
                if (document.IsModifiable)
                {
                    TransactionManager.Instance.EnsureInTransaction(document);
                    linePatternElem = RevitLinePatternElem.Create(document, linePattern);
                    document.Regenerate();
                    TransactionManager.Instance.TransactionTaskDone();
                }
                else
                {
                    using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                    {
                        trans.Start(transactionName);
                        linePatternElem = RevitLinePatternElem.Create(document, linePattern);
                        document.Regenerate();
                        trans.Commit();
                    }
                }
            }

            return linePatternElem;            
        }

        private static RevitLinePatternSegment _SegmentByTypeByLength (string segementType, double length)
        {
            RevitLinePatternSegementType sType = (RevitLinePatternSegementType) Enum.Parse(typeof(RevitLinePatternSegementType), segementType);
            return new RevitLinePatternSegment(sType, length);
        }
    }
}
