using System;
using winForms = System.Windows.Forms;

using Autodesk.DesignScript.Runtime;

namespace Synthetic.Core.Input
{
    class DialogWindows
    {
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
    }
}
