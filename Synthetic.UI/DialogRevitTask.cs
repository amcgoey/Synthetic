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

namespace Synthetic.UI
{
    /// <summary>
    /// Manages Revit standard Task Dialogs
    /// </summary>
    public class DialogRevitTask
    {
        internal RevitUi.TaskDialog dialog { get; private set; }

        internal DialogRevitTask(RevitUi.TaskDialog td)
        {
            dialog = td;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Title"></param>
        /// <returns></returns>
        public static DialogRevitTask ByTitle (string Title)
        {
            //Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            //RevitDoc activeDoc = DocumentManager.Instance.CurrentDBDocument;

            // Creates a Revit task dialog to communicate information to the user.
            return new DialogRevitTask(new RevitUi.TaskDialog(Title));
        }

        /// <summary>
        /// Resets all the Task Dialogs properties and setings.
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <returns></returns>
        public static DialogRevitTask ResetDialog(DialogRevitTask taskDialog)
        {
            string title = taskDialog.dialog.Title;
            taskDialog.dialog.Dispose();
            taskDialog = new DialogRevitTask(new RevitUi.TaskDialog(title));
            return taskDialog;
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="Buttons"></param>
        /// <returns></returns>
        public static DialogRevitTask AddCommonButtons (DialogRevitTask taskDialog,
            [DefaultArgument("{\"Close\"}")] List<string> Buttons)
        {

            RevitUi.TaskDialogCommonButtons commonButtons = RevitUi.TaskDialogCommonButtons.None;

            // Set common buttons and default button. If no CommonButton or CommandLink is added,
            // task dialog will show a Close button by default
            foreach (string button in Buttons)
            {
                switch (button)
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

            taskDialog.dialog.CommonButtons = commonButtons;

            return taskDialog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <param name="CommandLinks"></param>
        /// <returns></returns>
        public static DialogRevitTask CommandLinks(DialogRevitTask taskDialog, List<string> CommandLinks)
        {
            if (CommandLinks != null)
            {
                int i = 0;
                foreach (string _Text in CommandLinks)
                {
                    if (_Text != "" || _Text != null)
                    {
                        switch (i)
                        {
                            case 0:
                                taskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink1, _Text);
                                break;
                            case 1:
                                taskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink2, _Text);
                                break;
                            case 2:
                                taskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink3, _Text);
                                break;
                            case 3:
                                taskDialog.dialog.AddCommandLink(RevitUi.TaskDialogCommandLinkId.CommandLink4, _Text);
                                break;
                            default:
                                break;
                        }
                    }
                    i++;
                }
            }
            return taskDialog;
        }

        /// <summary>
        /// Shows the task dialog box.  All properties of the dialog will be reset to allow for changes in future runs.
        /// </summary>
        /// <param name="taskDialog"></param>
        /// <returns></returns>
        public static RevitUi.TaskDialogResult Show (DialogRevitTask taskDialog,
            [DefaultArgument("true")] bool reset)
        {
            RevitUi.TaskDialogResult tResult = taskDialog.dialog.Show();

            DialogRevitTask.ResetDialog(taskDialog);
            return tResult;
        }

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
        public static List<object> ListDialogResults ()
        {
            return sEnum.GetEnums("Autodesk.Revit.UI.TaskDialogResult");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ResultName"></param>
        /// <param name="Execute"></param>
        /// <returns></returns>
        public static Dictionary<RevitUi.TaskDialogResult, object> ParseResults (List<string> ResultName, List<object> Execute)
        {
            Dictionary<RevitUi.TaskDialogResult, object> dict = new Dictionary<Autodesk.Revit.UI.TaskDialogResult, object>();
            int i = 0;

            foreach (string name in ResultName)
            {
                if (i < Execute.Count)
                {
                    RevitUi.TaskDialogResult e = (RevitUi.TaskDialogResult)sEnum.Parse("Autodesk.Revit.UI.TaskDialogResult", name);
                    dict.Add(e, Execute[i]);
                }
                i++;
            }
            return dict;
        }

        /// <summary>
        /// Represents the Dialog box in a string format.
        /// </summary>
        /// <returns name="string">A string representation</returns>
        public override string ToString()
        {
            return string.Format("Task Dialog {0}", this.dialog.Title);
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
        public static object ByProperties(
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
    }
}
