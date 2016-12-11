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
        /// Creates a Revit Task Dialog.  This can be used to make a choice from up to four different options.
        /// </summary>
        /// <param name="Title">The Title of the dialog box.</param>
        /// <param name="Instructions">Instructions are written in a large font at the top of the dialog.</param>
        /// <param name="Content">Detailed content to be included.</param>
        /// <param name="FooterText">Footer text.  Useful for providing links to help content.</param>
        /// <param name="CommandLinkText">The names of each command link button.  Used to return different selections from the dialog.  A command link will be added for each item in the list with a maximum of (4) command links.</param>
        /// <param name="CommandLinkResults">The results to be returned based on the command link selection.  There should be an equal number of results as there are command links.</param>
        /// <param name="reset">Resets the node so the dialog will reopen.</param>
        /// <returns name="Results">Returns the corresponding result based on the command link selected.  Will return null if no command links are selected or available.</returns>
        public static object DialogTask(
            [DefaultArgument("\"Title\"")] string Title,
            [DefaultArgument("\"\"")] string Instructions,
            [DefaultArgument("\"\"")] string Content,
            [DefaultArgument("\"\"")] string FooterText,
            [DefaultArgument("{}")] List<string> CommandLinkText,
            [DefaultArgument("{}")]List<object> CommandLinkResults,
            [DefaultArgument("true")] bool reset
            )
        {
            object result = null;


            Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            RevitDoc activeDoc = DocumentManager.Instance.CurrentDBDocument;

            // Creates a Revit task dialog to communicate information to the user.
            RevitUi.TaskDialog mainDialog = new RevitUi.TaskDialog(Title);
            mainDialog.MainInstruction = Instructions;
            mainDialog.MainContent = Content;

            if (CommandLinkText != null)
            {
                int i = 0;
                foreach (string _Text in CommandLinkText)
                {
                    if (_Text != "" || _Text != null)
                    {
                        switch (i)
                        {
                            case 0:
                                mainDialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink1, _Text);
                                break;
                            case 1:
                                mainDialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink2, _Text);
                                break;
                            case 2:
                                mainDialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink3, _Text);
                                break;
                            case 3:
                                mainDialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink4, _Text);
                                break;
                            default:
                                break;
                        }
                    }
                    i++;
                }
            }

            // Set common buttons and default button. If no CommonButton or CommandLink is added,
            // task dialog will show a Close button by default
            mainDialog.CommonButtons = RevitUi.TaskDialogCommonButtons.Close;
            mainDialog.DefaultButton = RevitUi.TaskDialogResult.Close;

            // Set footer text. Footer text is usually used to link to the help document.
            mainDialog.FooterText = FooterText;

            RevitUi.TaskDialogResult tResult = mainDialog.Show();

            // If the user clicks the first command link, a simple Task Dialog 
            // with only a Close button shows information about the Revit installation. 
            if (RevitUi.TaskDialogResult.CommandLink1 == tResult)
            {
                if (0 < CommandLinkResults.Count)
                {
                    result = CommandLinkResults[0];
                }
            }

            // If the user clicks the second command link, a simple Task Dialog 
            // created by static method shows information about the active document
            else if (RevitUi.TaskDialogResult.CommandLink2 == tResult)
            {
                if (1 < CommandLinkResults.Count)
                {
                    result = CommandLinkResults[1];
                }
            }

            else if (RevitUi.TaskDialogResult.CommandLink3 == tResult)
            {
                if (2 < CommandLinkResults.Count)
                {
                    result = CommandLinkResults[2];
                }
            }

            else if (RevitUi.TaskDialogResult.CommandLink4 == tResult)
            {
                if (3 < CommandLinkResults.Count)
                {
                    result = CommandLinkResults[3];
                }
            }

            return result;
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
