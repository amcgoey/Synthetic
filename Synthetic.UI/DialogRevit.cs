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
    public class DialogRevit
    {
        internal DialogRevit() { }

        /// <summary>
        /// Creates a Revit Open File dialog with inputs for filtering the file type.  Please note that this does not actually open a file, but returns a file path.
        /// </summary>
        /// <param name="title">Title of the dialog box.</param>
        /// <param name="fileTypeFilter">A string that filters the file types to be selected.  Format should be as follows: display value|file type.  For example "Text (*.txt)|*.txt"</param>
        /// <param name="reset">Resets the node so the dialog will reopen.</param>
        /// <returns name="File Path">A string of the file path.</returns>
        public static string DialogFileOpen(
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
        public static string DialogFileSave(
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
    }
}
