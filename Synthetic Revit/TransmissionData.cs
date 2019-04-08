using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;

using Autodesk.Revit.DB;

using tData = Autodesk.Revit.DB.TransmissionData;
using modelPath = Autodesk.Revit.DB.ModelPath;

namespace Synthetic.Revit
{
    public class TransmissionData
    {
        internal TransmissionData () { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns name="TransmissionData"></returns>
        public static tData Read (string FilePath)
        {
            ModelPath mPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(FilePath);

            return tData.ReadTransmissionData(mPath);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="transmissionData"></param>
        /// <returns name="FilePath">Returns the FilePath.</returns>
        public static string Write (string FilePath, tData transmissionData)
        {
            ModelPath mPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(FilePath);

            tData.WriteTransmissionData(mPath, transmissionData);

            return FilePath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns name="bool">Returns true if file is marked as Transmitted.  False if is not transmitted.</returns>
        public static bool IsTransmitted (string FilePath)
        {
            ModelPath mPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(FilePath);
            tData td = tData.ReadTransmissionData(mPath);

            return td.IsTransmitted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="IsTransmitted"></param>
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
