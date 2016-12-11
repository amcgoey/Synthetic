//References to the System
using System;
using System.Collections;
using System.Collections.Generic;

//References to Dynamo
using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;
using Revit.Elements;
using dynamoElem = Revit.Elements.Element;
using DynColor = DSCore.Color;

//References to Revit
using Autodesk.Revit.DB;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;
using RevitDoc = Autodesk.Revit.DB.Document;
using revitColor = Autodesk.Revit.DB.Color;

using RevitUi = Autodesk.Revit.UI;
using revitSelect = Autodesk.Revit.UI.Selection;

//References to Synthetic
using SynthEnum = Synthetic.Core.Enumeration;
using SynthColor = Synthetic.Revit.ColorWrapper;

namespace Synthetic.UI
{
    /// <summary>
    /// Access additional input methods.
    /// </summary>
    public class Pick
    {
        internal Pick() { }

        /// <summary>
        /// Pick Elementsin the current Revit Document.  Don't forget to hit the Finished button in the options bar.
        /// </summary>
        /// <param name="message">A message to be displayed in the status bar.</param>
        /// <param name="reset">Resets the node so one can pick new objects.</param>
        /// <returns name="Elements">List of the selected elements.</returns>
        public static List<dynamoElem> PickElements(
            [DefaultArgument("Select elements")] string message,
            [DefaultArgument("true")] bool reset)
        {

            Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            RevitDoc doc = DocumentManager.Instance.CurrentDBDocument;

            List<dynamoElem> elems = new List<dynamoElem>();

            revitSelect.Selection selection = uiapp.ActiveUIDocument.Selection;

            try
            {
                IList<Reference> references = selection.PickObjects(revitSelect.ObjectType.Element, message);
                foreach (Reference r in references)
                {
                    dynamoElem elem = doc.GetElement(r.ElementId).ToDSType(true);
                    elems.Add(elem);
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException e)
            {
                return null;
            }

            return elems;
        }

        /// <summary>
        /// Opens a pick color dialog box.
        /// </summary>
        /// <returns name="Color">A Dynamo Core Color</returns>
        public static DynColor PickColor()
        {
            DynColor _dColor = null;

            RevitUi.ColorSelectionDialog cSelect = new RevitUi.ColorSelectionDialog();

            if (cSelect.Show() == RevitUi.ItemSelectionDialogResult.Confirmed)
            {
                SynthColor _color = SynthColor.Wrap(cSelect.SelectedColor);
                _dColor = SynthColor.ToDynamoColor(_color);
            }

            cSelect.Dispose();
            return _dColor;
        }
    }
}
