using System;
using System.Collections;
using System.Collections.Generic;
using winForms = System.Windows.Forms;

using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;
using Revit.Elements;
using dynamoElem = Revit.Elements.Element;
using DynColor = DSCore.Color;

using Autodesk.Revit.DB;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;
using RevitDoc = Autodesk.Revit.DB.Document;
using revitColor = Autodesk.Revit.DB.Color;

using RevitUi = Autodesk.Revit.UI;
using revitSelect = Autodesk.Revit.UI.Selection;

using SynthEnum = Synthetic.Core.Enumeration;

namespace Synthetic.Revit
{
    /// <summary>
    /// Access additional input methods.
    /// </summary>
    public class Input
    {
        internal Input() { }

        /// <summary>
        /// Creates a Open File Dialog box with inputs for filtering the file type and whether the multiple files can be selected.
        /// </summary>
        /// <param name="fileTypeFilter">A string that filters the file types to be selected.  Format should be as follows: display value|file type.  For example "Text (*.txt)|*.txt"</param>
        /// <param name="multiSelect">True allows multiple files to be selected, false allows only a single file.</param>
        /// <param name="reset">Resets the node so the dialog will reopen.</param>
        /// <returns name="File Path">A string of the file path.</returns>
        public static string[] DialogFileOpenByOS(
            [DefaultArgument("\"All Files (*.*)|*.*\"")] string fileTypeFilter,
            [DefaultArgument("true")] bool multiSelect,
            [DefaultArgument("true")] bool reset
            )
        {
            string[] fileNames = null;

            // Create an instance of the open file dialog box.
            winForms.OpenFileDialog openFileDialog1 = new winForms.OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = fileTypeFilter;
            openFileDialog1.FilterIndex = 1;

            openFileDialog1.Multiselect = multiSelect;

            // Call the ShowDialog method to show the dialog box.
            winForms.DialogResult userClickedOK = openFileDialog1.ShowDialog();
            // Process input if the user clicked OK.
            if (userClickedOK == winForms.DialogResult.OK)
            {
                fileNames = openFileDialog1.FileNames;
            }

            openFileDialog1.Dispose();

            return fileNames;
        }

        /// <summary>
        /// Creates a Revit Open File dialog with inputs for filtering the file type.  Please note that this does not actually open a file, but returns a file path.
        /// </summary>
        /// <param name="title">Title of the dialog box.</param>
        /// <param name="fileTypeFilter">A string that filters the file types to be selected.  Format should be as follows: display value|file type.  For example "Text (*.txt)|*.txt"</param>
        /// <param name="reset">Resets the node so the dialog will reopen.</param>
        /// <returns name="File Path">A string of the file path.</returns>
        public static string DialogFileOpenByRevit(
            [DefaultArgument("\"Select a file\"")] string title,
            [DefaultArgument("\"All Files (*.*)|*.*\"")] string fileTypeFilter,
            [DefaultArgument("true")] bool reset
            )
        {
            string FilePath = null;

            RevitUi.FileOpenDialog dialog = new RevitUi.FileOpenDialog(fileTypeFilter);
            dialog.Title = title;

            if (dialog.Show() == RevitUi.ItemSelectionDialogResult.Confirmed)
            {
                FilePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(dialog.GetSelectedModelPath());
            }

            dialog.Dispose();

            return FilePath;
        }

        /// <summary>
        /// Creates a Revit Save File dialog with inputs for filtering the file type.  If an existing file is selected, the dialog will ask the user if they want to overwrite the file.  Please note that this does not actually save a file, but returns a file path.
        /// </summary>
        /// <param name="title">Title of the dialog box.</param>
        /// <param name="fileTypeFilter">A string that filters the file types to be selected.  Format should be as follows: display value|file type.  For example "Text (*.txt)|*.txt"</param>
        /// <param name="reset">Resets the node so the dialog will reopen.</param>
        /// <returns name="File Path">A string of the file path.</returns>
        public static string DialogFileSaveByRevit(
            [DefaultArgument("\"Select a file\"")] string title,
            [DefaultArgument("\"All Files (*.*)|*.*\"")] string fileTypeFilter,
            [DefaultArgument("true")] bool reset
            )
        {
            string FilePath = null;

            RevitUi.FileSaveDialog dialog = new RevitUi.FileSaveDialog(fileTypeFilter);
            dialog.Title = title;

            if (dialog.Show() == RevitUi.ItemSelectionDialogResult.Confirmed)
            {
                FilePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(dialog.GetSelectedModelPath());
            }

            dialog.Dispose();

            return FilePath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reset"></param>
        /// <returns></returns>
        [MultiReturn(new[] { "Filter Name", "Filter Id" })]
        public static IDictionary DialogFilter(
            [DefaultArgument("true")] bool reset
            )
        {
            ParameterFilterElement filter = null;
            string filterName = null;
            string filterId = null;


            RevitDoc doc = DocumentManager.Instance.CurrentDBDocument;

            RevitUi.FilterDialog dialog = new RevitUi.FilterDialog(doc, ElementId.InvalidElementId);

            dialog.Show();

            filterName = dialog.NewFilterName;
            filterId = dialog.NewFilterName;

            dialog.Dispose();

            return new Dictionary<string, object>
            {
                {"Filter Name", filterName},
                {"Filter Id", filterId}
            };
        }

        /// <summary>
        /// Pick Elementsin the current Revit Document.  Don't forget to hit the Finished button in the options bar.
        /// </summary>
        /// <param name="message">A message to be displayed in the status bar.</param>
        /// <param name="reset">Resets the node so one can pick new objects.</param>
        /// <returns name="Elements">List of the selected elements.</returns>
        public static List<dynamoElem> PickElements(
            [DefaultArgument("Select elements")] string message,
            [DefaultArgument("true")] bool reset )
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
        public static DynColor PickColor ()
        {
            DynColor _dColor = null;

            RevitUi.ColorSelectionDialog cSelect = new RevitUi.ColorSelectionDialog();

            if (cSelect.Show() == RevitUi.ItemSelectionDialogResult.Confirmed)
            {
                ColorWrapper _color = ColorWrapper.Wrap(cSelect.SelectedColor);
                _dColor = ColorWrapper.ToDynamoColor(_color);
            }

            cSelect.Dispose();
            return _dColor;
        }
    }
}
