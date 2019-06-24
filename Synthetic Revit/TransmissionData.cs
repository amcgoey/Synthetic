using System;

using Autodesk.DesignScript.Runtime;

using Autodesk.Revit.DB;

using tData = Autodesk.Revit.DB.TransmissionData;
using modelPath = Autodesk.Revit.DB.ModelPath;

namespace Synthetic.Revit
{
    /// <summary>
    /// Wrapper for Revit API TransmissionData
    /// </summary>
    public class TransmissionData
    {
        internal TransmissionData () { }

        /// <summary>
        /// Reads the TransmissionData from a file.
        /// </summary>
        /// <param name="FilePath">Path to the file.</param>
        /// <returns name="TransmissionData">Revit TransmissionData</returns>
        public static tData Read (string FilePath)
        {
            ModelPath mPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(FilePath);

            return tData.ReadTransmissionData(mPath);
        }


        /// <summary>
        /// Writes modified TransmissionData from a file.  File must be closed to write TransmissionData.
        /// </summary>
        /// <param name="FilePath">Path to the file.</param>
        /// <param name="transmissionData">A Revit TransmissionData object</param>
        /// <returns name="FilePath">Returns the FilePath to the project.</returns>
        public static string Write (string FilePath, tData transmissionData)
        {
            ModelPath mPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(FilePath);

            tData.WriteTransmissionData(mPath, transmissionData);

            return FilePath;
        }

        /// <summary>
        /// Verifies whether the file is flagged as Transmitted or not.
        /// </summary>
        /// <param name="FilePath">Path to the file.</param>
        /// <returns name="bool">Returns true if file is marked as Transmitted.  False if is not transmitted.</returns>
        public static bool IsTransmitted (string FilePath)
        {
            ModelPath mPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(FilePath);
            tData td = tData.ReadTransmissionData(mPath);

            return td.IsTransmitted;
        }

        /// <summary>
        /// Set's the file to Transmitted or not.
        /// </summary>
        /// <param name="FilePath">Path to the file.</param>
        /// <param name="IsTransmitted">True to flag the file as Transmitted.</param>
        /// <returns name="FilePath">Returns the FilePath.</returns>
        public static string SetIsTransmitted (string FilePath, bool IsTransmitted)
        {
            ModelPath mPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(FilePath);
            tData td = tData.ReadTransmissionData(mPath);

            if(td.IsTransmitted != IsTransmitted)
            {
                td.IsTransmitted = IsTransmitted;
                tData.WriteTransmissionData(mPath, td);
            }

            return FilePath;
        }

    }
}
