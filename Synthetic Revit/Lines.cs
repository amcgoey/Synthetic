using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
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

namespace Synthetic.Revit
{
    public class Lines
    {
        internal Lines() { }

        public static RevitCategory LineStyleByName(string Name, RevitDB.GraphicsStyle graphicsStyle, [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            //  Name of Transaction
            string transactionName = "Create Line Style";

            RevitDB.Categories categories = document.Settings.Categories;

            RevitCategory lineCat = categories.get_Item(RevitDB.BuiltInCategory.OST_Lines);

            RevitCategory newLineStyleCat;

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                newLineStyleCat = categories.NewSubcategory(lineCat, Name);
                document.Regenerate();
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    newLineStyleCat = categories.NewSubcategory(lineCat, Name);
                    document.Regenerate();
                    trans.Commit();
                }
            }
            return newLineStyleCat;
        }


        [MultiReturn(new[] { "Merged", "Failed" })]
        public static IDictionary MergeLineStyles(string FromLineStyle, string ToLineStyle, [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            //  Name of Transaction
            string transactionName = "Merge Line Style";
            List<DynaCurve> elements = new List<DynaCurve>();
            List<DynaCurve> elementsFailed = new List<DynaCurve>();

            RevitDB.CategoryNameMap lineCategories = RevitDB.Category.GetCategory(document, RevitDB.BuiltInCategory.OST_Lines).SubCategories;

            RevitDB.Category FromCategory;
            RevitDB.Category ToCategory;

            //  If both FromLineStyle and ToLineStyle categories exist, proceed with merge.
            if (lineCategories.Contains(FromLineStyle) && lineCategories.Contains(ToLineStyle))
            {
                FromCategory = RevitDB.Category.GetCategory(document, RevitDB.BuiltInCategory.OST_Lines).SubCategories.get_Item(FromLineStyle);
                ToCategory = RevitDB.Category.GetCategory(document, RevitDB.BuiltInCategory.OST_Lines).SubCategories.get_Item(ToLineStyle);

                RevitDB.GraphicsStyle ToGraphicStyle = ToCategory.GetGraphicsStyle(RevitDB.GraphicsStyleType.Projection);
                RevitDB.GraphicsStyle FromGraphicStyle = FromCategory.GetGraphicsStyle(RevitDB.GraphicsStyleType.Projection);

                RevitDB.ElementCategoryFilter categoryFilter = new RevitDB.ElementCategoryFilter(FromCategory.Id);
                RevitDB.FilteredElementCollector collector = new RevitDB.FilteredElementCollector(document)
                    .OfClass(typeof(RevitDB.CurveElement))
                    //.OfCategory(RevitDB.BuiltInCategory.OST_Lines)
                    ;

                IEnumerable<RevitDB.Element> lineElements = collector
                    .Cast<RevitDB.CurveElement>()
                    .Where(q => q.LineStyle.Id == FromGraphicStyle.Id)
                    .ToList();

                // Define Function to change CurveElement LineStyles.
                Action<IEnumerable<RevitDB.Element>> _SetLineStyle = (lines) =>
                {
                    foreach (RevitDB.CurveElement line in lines)
                    {
                        // If Element is in a group, put the element in the failed list
                        int groupId = line.GroupId.IntegerValue;
                        if (groupId == -1)
                        {
                            line.LineStyle = ToGraphicStyle;
                            DynaCurve dElem = (DynaCurve)line.ToDSType(true);

                            if (line.LineStyle.Id == ToGraphicStyle.Id)
                            {
                                elements.Add(dElem);
                            }
                            else
                            {
                                elementsFailed.Add(dElem);
                            }
                        }
                        else
                        {
                            DynaCurve dElem = (DynaCurve)line.ToDSType(true);
                            elementsFailed.Add(dElem);
                        }
                    }

                    // Check if there are any instances of FromType left
                    int count = collector
                        .Cast<RevitDB.CurveElement>()
                        .Where(q => q.LineStyle.Id == FromGraphicStyle.Id)
                        .ToList().Count();
                    if (count == 0)
                    {
                        if (!FromCategory.IsReadOnly &&
                        !FromCategory.Name.Contains("<") &&
                        FromCategory.Name != "Hidden Lines" &&
                        FromCategory.Name != "Axis of Rotation" &&
                        FromCategory.Name != "Boundary" &&
                        FromCategory.Name != "Insulation Batting Lines" &&
                        FromCategory.Name != "Lines" &&
                        FromCategory.Name != "Medium Lines" &&
                        FromCategory.Name != "Wide Lines" &&
                        FromCategory.Name != "Thin Lines"
                        )
                            //if (FromCategory.Name != "Thin Lines")
                        {
                            document.Delete(FromCategory.Id);
                        }
                    }

                    _MoveAllFilledRegions(document);
                };

                //if (document.IsModifiable)
                //{
                TransactionManager.Instance.EnsureInTransaction(document);
                _SetLineStyle(lineElements);
                document.Regenerate();
                TransactionManager.Instance.TransactionTaskDone();
                //}
                //else
                //{
                //    using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                //    {
                //        trans.Start(transactionName);
                //        _SetLineStyle(lineElements);
                //        trans.Commit();
                //    }
                //}
            }

            return new Dictionary<string, object>
            {
                {"Merged", elements},
                {"Failed", elementsFailed}
            };

        }

        private static void _MoveAllFilledRegions(RevitDoc document)
        {
            RevitDB.FilteredElementCollector fillRegions = new RevitDB.FilteredElementCollector(document).OfClass(typeof(RevitDB.FilledRegion));

            // Moves the filled region in the Z axis to refresh the lines
            List<RevitDB.FilledRegion> ListFilledRegion = new List<RevitDB.FilledRegion>();
            foreach (RevitDB.FilledRegion filledRegion in fillRegions)
            {
                try
                {
                    // If filled region is pinned will unpin and add to a list
                    if (filledRegion.Pinned == true)
                    {
                        filledRegion.Pinned = false;
                        ListFilledRegion.Add(filledRegion);
                    }

                    // Move in the z axis
                    if (filledRegion.Pinned == false)
                    {
                        RevitDB.XYZ z = RevitDB.XYZ.BasisZ;
                        RevitDB.ElementTransformUtils.MoveElement(document, filledRegion.Id, z);
                        RevitDB.ElementTransformUtils.MoveElement(document, filledRegion.Id, -z);
                    }
                }
                catch { }
            }

            // Pins all filled regions that had been previously pinned 
            foreach (RevitDB.FilledRegion filledPinned in ListFilledRegion)
            {
                filledPinned.Pinned = true;
            }
        }
    }
}
