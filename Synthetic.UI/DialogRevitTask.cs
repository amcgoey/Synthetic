using System;
using System.Collections.Generic;

//Dynamo References
using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;

//Revit References
using RevitUi = Autodesk.Revit.UI;
using RevitDoc = Autodesk.Revit.DB.Document;

//Synthetic References
using sEnum = Synthetic.Core.Enumeration;
using sDict = Synthetic.Core.Dictionary;

namespace Synthetic.UI
{
    /// <summary>
    /// Manages Revit standard Task Dialogs
    /// </summary>
    public class DialogRevitTask
    {
        internal RevitUi.TaskDialog dialog { get; private set; }

        internal DialogRevitTask(RevitUi.TaskDialog taskDialog)
        {
            dialog = taskDialog;
        }

        internal DialogRevitTask(string title)
        {
            dialog = new RevitUi.TaskDialog(title);
        }

        /// <summary>
        /// Creates a Revit Task Dialog and sets it Title.  Otherwise, the dialog is empty.
        /// </summary>
        /// <param name="Title"></param>
        /// <returns name="TaskDialog"></returns>
        public static DialogRevitTask ByTitle (string Title)
        {
            // Creates a Revit task dialog to communicate information to the user.
            return new DialogRevitTask(Title);
        }

        /// <summary>
        /// Creates a Revit Task Dialog.  The dialog can be configured in a variety of ways.  Additional options are available if you use the ByTitle method.
        /// </summary>
        /// <param name="Title">The Title of the dialog box.</param>
        /// <param name="Instructions">Instructions are written in a large font at the top of the dialog.</param>
        /// <param name="Content">Detailed content to be included.</param>
        /// <param name="FooterText">Footer text.  Useful for providing links to help content.</param>
        /// <param name="Buttons">A dictionary of the desired buttons.  Use the SpecifyButtons method to create the dictionary.</param>
        /// <param name="CommandLinks">A dictionary of the desired CommandLinks and associated text.  Use the SpecifyCommandLinks to create the dictionary.</param>
        /// <param name="Results">A dictionary of the possible results of the dialog.  Use the SpecifyResults method to create the dictionary.</param>
        /// <param name="Toggle">Resets the node so the dialog will reopen.</param>
        /// <returns name="Result">Returns the corresponding result based on the command link or button selected.  Will return null if no results are available.</returns>
        public static object ByAllProperties(
            [DefaultArgument("\"Title\"")] string Title,
            [DefaultArgument("\"\"")] string Instructions,
            [DefaultArgument("\"\"")] string Content,
            [DefaultArgument("\"\"")] string FooterText,
            [DefaultArgument("null")] sDict Buttons,
            [DefaultArgument("null")] sDict CommandLinks,
            [DefaultArgument("null")] sDict Results,
            [DefaultArgument("true")] bool Toggle
            )
        {
            // Creates a Revit task dialog to communicate information to the user.
            DialogRevitTask dialog = new DialogRevitTask(Title);

            // Set the content areas of the dialog
            if (Instructions != "") { DialogRevitTask.SetInstructions(dialog, Instructions); }
            if (Content != "") { DialogRevitTask.SetContent(dialog, Content); }
            if (FooterText != "") { DialogRevitTask.SetFooterText(dialog, FooterText); }

            // Set the common buttons.  The default button will be the first button on the list.
            if (Buttons != null) { DialogRevitTask.AddCommonButtons(dialog, Buttons); }

            // If there are command links, add them to the dialog
            if (CommandLinks != null) { DialogRevitTask.AddCommandLinks(dialog, CommandLinks); }

            RevitUi.TaskDialogResult tResult = DialogRevitTask.Show(dialog, true);

            if (Results != null)
            { return FilterResults(tResult, Results); }
            else
            { return tResult;  }
        }

        /// <summary>
        /// Resets all the Task Dialogs properties and setings.
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <returns></returns>
        public static DialogRevitTask ResetDialog(DialogRevitTask taskDialog, bool toggle)
        {
            string title = taskDialog.dialog.Title;
            taskDialog.dialog.Dispose();
            taskDialog = new DialogRevitTask(new RevitUi.TaskDialog(title));
            return taskDialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="None"></param>
        /// <param name="Ok"></param>
        /// <param name="Yes"></param>
        /// <param name="No"></param>
        /// <param name="Cancel"></param>
        /// <param name="Retry"></param>
        /// <param name="Close"></param>
        /// <returns name="Buttons"></returns>
        public static sDict SpecifyButtons (
            [DefaultArgument("true")] bool None,
            [DefaultArgument("false")] bool Ok,
            [DefaultArgument("false")] bool Yes,
            [DefaultArgument("false")] bool No,
            [DefaultArgument("false")] bool Cancel,
            [DefaultArgument("false")] bool Retry,
            [DefaultArgument("false")] bool Close
            )
        {
            List<string> keys = new List<string> { "None", "Ok", "Cancel", "Retry", "Yes", "No", "Close" };
            List<object> values = new List<object> { None, Ok, Cancel, Retry, Yes, No, Close };

            sDict dict = sDict.ByKeysValues(keys, values);

            return dict;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CommandLink1"></param>
        /// <param name="CommandLink2"></param>
        /// <param name="CommandLink3"></param>
        /// <param name="CommandLink4"></param>
        /// <returns name="CommandLinks"></returns>
        public static sDict SpecifyCommandLinks(
            [DefaultArgument("null")] string CommandLink1,
            [DefaultArgument("null")] string CommandLink2,
            [DefaultArgument("null")] string CommandLink3,
            [DefaultArgument("null")] string CommandLink4
            )
        {
            List<string> keys = new List<string> ();
            List<object> values = new List<object> ();

            if (CommandLink1 != null)
            {
                keys.Add("CommandLink1");
                values.Add(CommandLink1);
            }

            if (CommandLink2 != null)
            {
                keys.Add("CommandLink2");
                values.Add(CommandLink2);
            }

            if (CommandLink3 != null)
            {
                keys.Add("CommandLink3");
                values.Add(CommandLink3);
            }

            if (CommandLink4 != null)
            {
                keys.Add("CommandLink4");
                values.Add(CommandLink4);
            }

            sDict dict = sDict.ByKeysValues(keys, values);

            return dict;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="None"></param>
        /// <param name="Ok"></param>
        /// <param name="Cancel"></param>
        /// <param name="Retry"></param>
        /// <param name="Yes"></param>
        /// <param name="No"></param>
        /// <param name="Close"></param>
        /// <param name="CommandLink1"></param>
        /// <param name="CommandLink2"></param>
        /// <param name="CommandLink3"></param>
        /// <param name="CommandLink4"></param>
        /// <returns name="Results">A dictionary of possible results.</returns>
        public static sDict SpecifyResults(
            [DefaultArgument("null")] object None,
            [DefaultArgument("null")] object Ok,
            [DefaultArgument("null")] object Cancel,
            [DefaultArgument("null")] object Retry,
            [DefaultArgument("null")] object Yes,
            [DefaultArgument("null")] object No,
            [DefaultArgument("null")] object Close,
            [DefaultArgument("null")] object CommandLink1,
            [DefaultArgument("null")] object CommandLink2,
            [DefaultArgument("null")] object CommandLink3,
            [DefaultArgument("null")] object CommandLink4
            )
        {
            List<string> keys = new List<string> { "None", "Ok", "Cancel", "Retry", "Yes", "No", "Close", "CommandLink1", "CommandLink2", "CommandLink3", "CommandLink4" };
            List<object> values = new List<object> { None, Ok, Cancel, Retry, Yes, No, Close, CommandLink1, CommandLink2, CommandLink3, CommandLink4 };

            sDict dict = sDict.ByKeysValues(keys, values);

            return dict;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="Buttons"></param>
        /// <returns></returns>
        public static DialogRevitTask AddCommonButtons(DialogRevitTask taskDialog,
            [DefaultArgument("null")] sDict Buttons)
        {

            RevitUi.TaskDialogCommonButtons commonButtons = RevitUi.TaskDialogCommonButtons.None;

            if (Buttons != null)
            {
                // Set common buttons and default button. If no CommonButton or CommandLink is added,
                // task dialog will show a Close button by default
                foreach (KeyValuePair<string, object> button in sDict.Unwrap(Buttons))
                {
                    if ((bool)button.Value)
                    {
                        switch (button.Key)
                        {
                            case "Yes":
                                commonButtons = commonButtons | RevitUi.TaskDialogCommonButtons.Yes;
                                break;
                            case "No":
                                commonButtons = commonButtons | RevitUi.TaskDialogCommonButtons.No;
                                break;
                            case "Cancel":
                                commonButtons = commonButtons | RevitUi.TaskDialogCommonButtons.Cancel;
                                break;
                            case "Ok":
                                commonButtons = commonButtons | RevitUi.TaskDialogCommonButtons.Ok;
                                break;
                            case "Retry":
                                commonButtons = commonButtons | RevitUi.TaskDialogCommonButtons.Retry;
                                break;
                            case "None":
                                commonButtons = commonButtons | RevitUi.TaskDialogCommonButtons.None;
                                break;
                            case "Close":
                            default:
                                commonButtons = commonButtons | RevitUi.TaskDialogCommonButtons.Close;
                                break;
                        }
                    }
                }
            }
            else
            {
                commonButtons = RevitUi.TaskDialogCommonButtons.Close;
            }

            taskDialog.dialog.CommonButtons = commonButtons;

            return taskDialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="CommandLinks"></param>
        /// <returns></returns>
        public static DialogRevitTask AddCommandLinks(DialogRevitTask taskDialog, sDict CommandLinks)
        {
            if (CommandLinks != null)
            {
                foreach (KeyValuePair<string, object> cmdLink in sDict.Unwrap(CommandLinks))
                {
                    if ((string)cmdLink.Value != "" || cmdLink.Value != null)
                    {
                        switch (cmdLink.Key)
                        {
                            case "CommandLink1":
                                taskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink1, (string)cmdLink.Value);
                                break;
                            case "CommandLink2":
                                taskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink2, (string)cmdLink.Value);
                                break;
                            case "CommandLink3":
                                taskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink3, (string)cmdLink.Value);
                                break;
                            case "CommandLink4":
                                taskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink4, (string)cmdLink.Value);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return taskDialog;
        }

        /// <summary>
        /// Shows the task dialog box.  All properties of the dialog will be reset to allow for changes in future runs.
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="ToggleReset"></param>
        /// <returns></returns>
        public static RevitUi.TaskDialogResult Show(DialogRevitTask taskDialog,
            [DefaultArgument("true")] bool ToggleReset)
        {
            RevitUi.TaskDialogResult tResult = taskDialog.dialog.Show();

            taskDialog.dialog.Dispose();
            return tResult;
        }

        /// <summary>
        /// Retrieve the a result from the possible results.  Will return null if the result does not have a corresponding choice.
        /// </summary>
        /// <param name="DialogResult">The value from the Revit Task Dialog.</param>
        /// <param name="Results">A dictionary of possible results.  One can pass values or functions.</param>
        /// <returns name="Result">The chosen result.</returns>
        public static object FilterResults(
            RevitUi.TaskDialogResult DialogResult,
            sDict Results
            )
        {
            return sDict.Value(Results, DialogResult.ToString());
        }

        #region Set Dialog Properties

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static DialogRevitTask SetContent (DialogRevitTask taskDialog, string content)
        {
            taskDialog.dialog.MainContent = content;
            return taskDialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static DialogRevitTask SetInstructions(DialogRevitTask taskDialog, string instructions)
        {
            taskDialog.dialog.MainInstruction = instructions;
            return taskDialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static DialogRevitTask SetTitle(DialogRevitTask taskDialog, string title)
        {
            taskDialog.dialog.Title = title;
            return taskDialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static DialogRevitTask SetTitleAutoPrefix(DialogRevitTask taskDialog, bool prefix)
        {
            taskDialog.dialog.TitleAutoPrefix = prefix;
            return taskDialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="FooterText"></param>
        /// <returns></returns>
        public static DialogRevitTask SetFooterText(DialogRevitTask taskDialog, string FooterText)
        {
            taskDialog.dialog.FooterText = FooterText;
            return taskDialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="ExpandedContent"></param>
        /// <returns></returns>
        public static DialogRevitTask SetExpandedContent(DialogRevitTask taskDialog, string ExpandedContent)
        {
            taskDialog.dialog.ExpandedContent = ExpandedContent;
            return taskDialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="VerificationText"></param>
        /// <returns></returns>
        public static DialogRevitTask SetVerificationText(DialogRevitTask taskDialog, string VerificationText)
        {
            taskDialog.dialog.VerificationText = VerificationText;
            return taskDialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="AllowCancellation"></param>
        /// <returns></returns>
        public static DialogRevitTask AllowCancellation(DialogRevitTask taskDialog, bool AllowCancellation)
        {
            taskDialog.dialog.AllowCancellation = AllowCancellation;
            return taskDialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="ExtraCheckBoxText"></param>
        /// <returns></returns>
        public static DialogRevitTask SetExtraCheckBoxText(DialogRevitTask taskDialog, string ExtraCheckBoxText)
        {
            taskDialog.dialog.ExtraCheckBoxText = ExtraCheckBoxText;
            return taskDialog;
        }

        #endregion

        public static object ExecuteResults (RevitUi.TaskDialogResult DialogResult, List<string> ExecuteNames, List<object> Execute)
        {
            int i = 0;
            object e = null;

            foreach (string name in ExecuteNames)
            {
                RevitUi.TaskDialogResult executeType = (RevitUi.TaskDialogResult)sEnum.Parse("Autodesk.Revit.UI.TaskDialogResult", name);
                if (executeType == DialogResult)
                {
                    if (i < Execute.Count)
                    {
                        e = Execute[i];
                    }
                }
                i++;
            }
            return e;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ResultName"></param>
        /// <returns></returns>
        public static RevitUi.TaskDialogResult GetResultType (string ResultName)
        {
            return (RevitUi.TaskDialogResult)sEnum.Parse("Autodesk.Revit.UI.TaskDialogResult", ResultName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<object> TaskDialogResults ()
        {
            return sEnum.GetEnums("Autodesk.Revit.UI.TaskDialogResult");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<object> TaskDialogCommonButtons()
        {
            return sEnum.GetEnums("Autodesk.Revit.UI.TaskDialogCommonButtons");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<object> TaskDialogCommandLinkId()
        {
            return sEnum.GetEnums("Autodesk.Revit.UI.TaskDialogCommandLinkId");
        }

        /// <summary>
        /// Represents the Dialog box in a string format.
        /// </summary>
        /// <returns name="string">A string representation</returns>
        public override string ToString()
        {
            return string.Format("Task Dialog {0}", this.dialog.Title);
        }
    }
}
