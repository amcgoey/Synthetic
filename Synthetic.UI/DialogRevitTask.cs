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
        /// <summary>
        /// Internal property that stores the Revit TaskDialog object.
        /// </summary>
        internal RevitUi.TaskDialog dialog { get; private set; }

        /// <summary>
        /// Intneral constructor that takes a Revit TaskDialog object
        /// </summary>
        /// <param name="TaskDialog">A Revit TaskDialog object.</param>
        internal DialogRevitTask(RevitUi.TaskDialog TaskDialog)
        {
            dialog = TaskDialog;
        }

        /// <summary>
        /// Internal constructor that takes a title.
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        internal DialogRevitTask(string title)
        {
            dialog = new RevitUi.TaskDialog(title);
        }

        /// <summary>
        /// Creates a Revit Task Dialog and sets it Title.  Otherwise, the dialog is empty.
        /// </summary>
        /// <param name="Title">Title of the dialog.</param>
        /// <returns name="TaskDialog">A new task dialog.</returns>
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
        /// <returns name="Result">Returns the corresponding result based on the command link or button selected.  Will return null if the chosen button or command link has no result.  If now Results are input, the TaskDialogResult from the choice will be returned.</returns>
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
        /// Resets all the Task Dialogs properties and settings.
        /// </summary>
        /// <param name="TaskDialog">A task dialog.</param>
        /// <returns name="TaskDialog">The task dialog with a new Revit dialog.</returns>
        public static DialogRevitTask ResetDialog(DialogRevitTask TaskDialog, bool toggle)
        {
            string title = TaskDialog.dialog.Title;
            TaskDialog.dialog.Dispose();
            TaskDialog = new DialogRevitTask(new RevitUi.TaskDialog(title));
            return TaskDialog;
        }

        /// <summary>
        /// Creates a dictionary for setting the common buttons in a task dialog.
        /// </summary>
        /// <param name="None">Set an empty button.  If you include any other buttons, this setting will be overriden.</param>
        /// <param name="Ok">Include an Ok button.</param>
        /// <param name="Yes">Include a Yes button.</param>
        /// <param name="No">Include a No button.</param>
        /// <param name="Cancel">Include a Cancel button.</param>
        /// <param name="Retry">Include a Retry button.</param>
        /// <param name="Close">Include a Close button.</param>
        /// <returns name="Buttons">A dictionary for setting the common buttons.</returns>
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
        /// Creates a dictionary for setting the Command Links in a Task Dialog.  Up to four command links can be specified.
        /// </summary>
        /// <param name="CommandLink1">The text to be included in the command link.</param>
        /// <param name="CommandLink2">The text to be included in the command link.</param>
        /// <param name="CommandLink3">The text to be included in the command link.</param>
        /// <param name="CommandLink4">The text to be included in the command link.</param>
        /// <returns name="CommandLinks">A dectionary for setting the Command Links.</returns>
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
        /// A dictionary of possible results that correspond to the possible buttons and command links used by the Task Dialog.  Be careful to proivde consistent results as any object can be passed as a result.  In addition, one can pass a function as evaluate the results after the choice is made.  This could save computation time in more complex definitions.
        /// </summary>
        /// <param name="None">Result to be returned if this choice is chosen.</param>
        /// <param name="Ok">Result to be returned if this choice is chosen.</param>
        /// <param name="Cancel">Result to be returned if this choice is chosen.</param>
        /// <param name="Retry">Result to be returned if this choice is chosen.</param>
        /// <param name="Yes">Result to be returned if this choice is chosen.</param>
        /// <param name="No">Result to be returned if this choice is chosen.</param>
        /// <param name="Close">Result to be returned if this choice is chosen.</param>
        /// <param name="CommandLink1">Result to be returned if this choice is chosen.</param>
        /// <param name="CommandLink2">Result to be returned if this choice is chosen.</param>
        /// <param name="CommandLink3">Result to be returned if this choice is chosen.</param>
        /// <param name="CommandLink4">Result to be returned if this choice is chosen.</param>
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
        /// Adds commmon buttons to a Task Dialog given a dictionary specification.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="Buttons">A dictionary of specificed buttons.</param>
        /// <returns name="TaskDialog">The modified Task dialog.</returns>
        public static DialogRevitTask AddCommonButtons(DialogRevitTask TaskDialog,
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

            TaskDialog.dialog.CommonButtons = commonButtons;

            return TaskDialog;
        }

        /// <summary>
        /// Adds Command Links to a Task Dialog given a dictionary specification.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="CommandLinks">A dictionary of specificed command links.</param>
        /// <returns name="TaskDialog">The modified Task dialog.</returns>
        public static DialogRevitTask AddCommandLinks(DialogRevitTask TaskDialog, sDict CommandLinks)
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
                                TaskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink1, (string)cmdLink.Value);
                                break;
                            case "CommandLink2":
                                TaskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink2, (string)cmdLink.Value);
                                break;
                            case "CommandLink3":
                                TaskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink3, (string)cmdLink.Value);
                                break;
                            case "CommandLink4":
                                TaskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink4, (string)cmdLink.Value);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return TaskDialog;
        }

        /// <summary>
        /// Displays the task dialog box.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="Toggle">A toggle used to re-display the Task Dialog.</param>
        /// <returns name="DialogResult">A TaskDialogResult based on the button or command link that was chosen.</returns>
        public static RevitUi.TaskDialogResult Show(DialogRevitTask TaskDialog,
            [DefaultArgument("true")] bool Toggle)
        {
            RevitUi.TaskDialogResult tResult = TaskDialog.dialog.Show();

            TaskDialog.dialog.Dispose();
            return tResult;
        }

        /// <summary>
        /// Retrieve the a result from the possible results.  Will return null if the result does not have a corresponding choice.
        /// </summary>
        /// <param name="DialogResult">The value from the Task dialog.</param>
        /// <param name="Results">A dictionary of possible results.</param>
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
        /// Sets the content section of the task dialog.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="content">The content to be included</param>
        /// <returns name="TaskDialog">The modified Task dialog.</returns>
        public static DialogRevitTask SetContent (DialogRevitTask TaskDialog, string content)
        {
            TaskDialog.dialog.MainContent = content;
            return TaskDialog;
        }

        /// <summary>
        /// Sets the instructions portion of the task dialog.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="instructions">The instructions to be included</param>
        /// <returns name="TaskDialog">The modified Task dialog.</returns>
        public static DialogRevitTask SetInstructions(DialogRevitTask TaskDialog, string instructions)
        {
            TaskDialog.dialog.MainInstruction = instructions;
            return TaskDialog;
        }

        /// <summary>
        /// Sets the Title of the Task dialog.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="title">The titl to be included.</param>
        /// <returns name="TaskDialog">The modified Task dialog.</returns>
        public static DialogRevitTask SetTitle(DialogRevitTask TaskDialog, string title)
        {
            TaskDialog.dialog.Title = title;
            return TaskDialog;
        }

        /// <summary>
        /// Sets whether to include the name of the add-in as a prefix to the title.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="prefix">If true, show the prefix, else do not.</param>
        /// <returns name="TaskDialog">The modified Task dialog.</returns>
        public static DialogRevitTask SetTitleAutoPrefix(DialogRevitTask TaskDialog, bool prefix)
        {
            TaskDialog.dialog.TitleAutoPrefix = prefix;
            return TaskDialog;
        }

        /// <summary>
        /// Sets the Footer text of the Task dialog.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="FooterText">Footer text to be included.</param>
        /// <returns name="TaskDialog">The modified Task dialog.</returns>
        public static DialogRevitTask SetFooterText(DialogRevitTask TaskDialog, string FooterText)
        {
            TaskDialog.dialog.FooterText = FooterText;
            return TaskDialog;
        }

        /// <summary>
        /// Sets the Expanded Content of the Task dialog.  Expanded content is the portion at the bottom with an arrow to display more information.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="ExpandedContent">Expanded content to be included.</param>
        /// <returns name="TaskDialog">The modified Task dialog.</returns>
        public static DialogRevitTask SetExpandedContent(DialogRevitTask TaskDialog, string ExpandedContent)
        {
            TaskDialog.dialog.ExpandedContent = ExpandedContent;
            return TaskDialog;
        }

        /// <summary>
        /// Set the Verification text of the Task dialog.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="VerificationText">The verification text to be included.</param>
        /// <returns name="TaskDialog">The modified Task dialog.</returns>
        public static DialogRevitTask SetVerificationText(DialogRevitTask TaskDialog, string VerificationText)
        {
            TaskDialog.dialog.VerificationText = VerificationText;
            return TaskDialog;
        }

        /// <summary>
        /// Sets whether the dialog box can be canceled or not.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="AllowCancellation">If true, the dialog can be canceled, else it can not.</param>
        /// <returns name="TaskDialog">The modified Task dialog.</returns>
        public static DialogRevitTask AllowCancellation(DialogRevitTask TaskDialog, bool AllowCancellation)
        {
            TaskDialog.dialog.AllowCancellation = AllowCancellation;
            return TaskDialog;
        }

        /// <summary>
        /// Sets the ExtraCheckBox text of the Task dialog.
        /// </summary>
        /// <param name="TaskDialog">A Task dialog.</param>
        /// <param name="ExtraCheckBoxText">ExtraCheckBox text to be included.</param>
        /// <returns name="TaskDialog">The modified Task dialog.</returns>
        public static DialogRevitTask SetExtraCheckBoxText(DialogRevitTask TaskDialog, string ExtraCheckBoxText)
        {
            TaskDialog.dialog.ExtraCheckBoxText = ExtraCheckBoxText;
            return TaskDialog;
        }

        #endregion

        /// <summary>
        /// Represents the Dialog box in a string format.
        /// </summary>
        /// <returns name="string">A string representation</returns>
        public override string ToString()
        {
            return string.Format("Task Dialog {0}", this.dialog.Title);
        }

        /// <summary>
        /// Retrieves all the TaskDialogResults enumerations.
        /// </summary>
        /// <returns name="TaskDialogResults">TaskDialogResults enumerations</returns>
        public static List<object> TaskDialogResults ()
        {
            return sEnum.GetEnums("Autodesk.Revit.UI.TaskDialogResult");
        }

        /// <summary>
        /// Retrieves all the TaskDialogCommonButtons enumerations.
        /// </summary>
        /// <returns name="TaskDialogCommonButtons">TaskDialogCommonButtons enumerations</returns>
        public static List<object> TaskDialogCommonButtons()
        {
            return sEnum.GetEnums("Autodesk.Revit.UI.TaskDialogCommonButtons");
        }

        /// <summary>
        /// Retrieves all the TaskDialogCommandLinkId enumerations.
        /// </summary>
        /// <returns name="TaskDialogCommandLinkId">TaskDialogCommandLinkId enumerations</returns>
        public static List<object> TaskDialogCommandLinkId()
        {
            return sEnum.GetEnums("Autodesk.Revit.UI.TaskDialogCommandLinkId");
        }
    }
}
