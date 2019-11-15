using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitSheet = Autodesk.Revit.DB.ViewSheet;
using RevitRevision = Autodesk.Revit.DB.Revision;

using RevitServices.Transactions;
using Revit.Elements;
using DynaElem = Revit.Elements.Element;
using DynaRevision = Revit.Elements.Revision;
using DynaSheet = Revit.Elements.Views.Sheet;

namespace Synthetic.Revit
{
    /// <summary>
    /// Sheet wrapper class
    /// </summary>
    public class Sheet
    {
        internal Sheet() { }

        /// <summary>
        /// Adds a Revision to a Sheet
        /// </summary>
        /// <param name="Sheet">Dynamo wrapped Sheet element</param>
        /// <param name="Revision">Dynamo wrapped Revision element</param>
        /// <returns name="Sheet">Returns the modified sheet.</returns>
        public static DynaSheet AddRevision(DynaSheet Sheet, DynaRevision Revision)
        {
            RevitSheet revitSheet = (RevitSheet)Sheet.InternalElement;
            RevitRevision revitRevision = (RevitRevision)Revision.InternalElement;

            RevitDoc document = revitSheet.Document;

            List<RevitDB.ElementId> revisions = (List<RevitDB.ElementId>)revitSheet.GetAdditionalRevisionIds();

            if (!revisions.Contains(revitRevision.Id))
            {
                revisions.Add(revitRevision.Id);
            }

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                revitSheet.SetAdditionalRevisionIds(revisions);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start("Set Revision on Sheet");
                    revitSheet.SetAdditionalRevisionIds(revisions);
                    trans.Commit();
                }
            }

            return Sheet;
        }

        /// <summary>
        /// Creates a Placeholder Sheet
        /// </summary>
        /// <param name="SheetNumber">The sheet number</param>
        /// <param name="SheetTitle">The sheet title</param>
        /// <param name="document">Document to create the sheet in.</param>
        /// <returns name="Sheet">Returns the created placeholder sheet as a dynamo wrapped sheet.</returns>
        public static DynaSheet CreatePlaceHolderSheet (string SheetNumber, string SheetTitle,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            RevitSheet revitSheet;

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                revitSheet = RevitSheet.CreatePlaceholder(document);
                revitSheet.SheetNumber = SheetNumber;
                //revitSheet.ViewName = SheetTitle;
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start("Create Placeholder Sheet");
                    revitSheet = RevitSheet.CreatePlaceholder(document);
                    revitSheet.SheetNumber = SheetNumber;
                    //revitSheet.ViewName = SheetTitle;
                    trans.Commit();
                }
            }
            

            return (DynaSheet)revitSheet.ToDSType(false);
        }
    }
}
