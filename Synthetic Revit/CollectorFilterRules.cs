using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;

namespace Synthetic.Revit
{
    /// <summary>
    /// 
    /// </summary>
    public class CollectorFilterRules
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruleName"></param>
        /// <returns name="StringRule"></returns>
        //[SupressImportIntoVM]
        public static revitDB.FilterStringRuleEvaluator FilterStringRules (string ruleName)
        {
            switch (ruleName)
            {
                case "Autodesk.Revit.DB.FilterStringBeginsWith":
                    return new revitDB.FilterStringBeginsWith();
                case "Autodesk.Revit.DB.FilterStringContains":
                    return new revitDB.FilterStringContains();
                case "Autodesk.Revit.DB.FilterStringEndsWith":
                    return new revitDB.FilterStringEndsWith();
                case "Autodesk.Revit.DB.FilterStringEquals":
                    return new revitDB.FilterStringEquals();
                case "Autodesk.Revit.DB.FilterStringGreater":
                    return new revitDB.FilterStringGreater();
                case "Autodesk.Revit.DB.FilterStringGreaterOrEqual":
                    return new revitDB.FilterStringGreaterOrEqual();
                case "Autodesk.Revit.DB.FilterStringLess":
                    return new revitDB.FilterStringLess();
                case "Autodesk.Revit.DB.FilterStringLessOrEqual":
                    return new revitDB.FilterStringLessOrEqual();
                default:
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruleName"></param>
        /// <returns name="NumericRule"></returns>
        //[SupressImportIntoVM]
        public static revitDB.FilterNumericRuleEvaluator FilterNumericRules(string ruleName)
        {
            switch (ruleName)
            {
                case "Autodesk.Revit.DB.FilterNumericEquals":
                    return new revitDB.FilterNumericEquals();
                case "Autodesk.Revit.DB.FilterNumericGreater":
                    return new revitDB.FilterNumericGreater();
                case "Autodesk.Revit.DB.FilterNumericGreaterOrEqual":
                    return new revitDB.FilterNumericGreaterOrEqual();
                case "Autodesk.Revit.DB.FilterNumericLess":
                    return new revitDB.FilterNumericLess();
                case "Autodesk.Revit.DB.FilterNumericLessOrEqual":
                    return new revitDB.FilterNumericLessOrEqual();
                default:
                    return null;
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterStringBeginsWith FilterStringBeginsWith ()
        //{
        //    return new revitDB.FilterStringBeginsWith();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterStringContains FilterStringContains()
        //{
        //    return new revitDB.FilterStringContains();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterStringEndsWith FilterStringEndsWith()
        //{
        //    return new revitDB.FilterStringEndsWith();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterStringEquals FilterStringEquals()
        //{
        //    return new revitDB.FilterStringEquals();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterStringGreater FilterStringGreater()
        //{
        //    return new revitDB.FilterStringGreater();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterStringGreaterOrEqual FilterStringGreaterOrEqual()
        //{
        //    return new revitDB.FilterStringGreaterOrEqual();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterStringLess FilterStringLess()
        //{
        //    return new revitDB.FilterStringLess();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterStringLessOrEqual FilterStringLessOrEqual()
        //{
        //    return new revitDB.FilterStringLessOrEqual();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterNumericEquals FilterNumericEquals()
        //{
        //    return new revitDB.FilterNumericEquals();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterNumericGreater FilterNumericGreater()
        //{
        //    return new revitDB.FilterNumericGreater();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterNumericGreaterOrEqual FilterNumericGreaterOrEqual()
        //{
        //    return new revitDB.FilterNumericGreaterOrEqual();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterNumericLess FilterNumericLess()
        //{
        //    return new revitDB.FilterNumericLess();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public revitDB.FilterNumericLessOrEqual FilterNumericLessOrEqual()
        //{
        //    return new revitDB.FilterNumericLessOrEqual();
        //}

        /// <summary>
        /// 
        /// </summary>
        [SupressImportIntoVM]
        public enum StringRules
        {
            /// <summary>
            /// 
            /// </summary>
            FilterStringBeginsWith,
            /// <summary>
            /// 
            /// </summary>
            FilterStringContains,
            /// <summary>
            /// 
            /// </summary>
            FilterStringEndsWith,
            /// <summary>
            /// 
            /// </summary>
            FilterStringEquals,
            /// <summary>
            /// 
            /// </summary>
            FilterStringGreater,
            /// <summary>
            /// 
            /// </summary>
            FilterStringGreaterOrEqual,
            /// <summary>
            /// 
            /// </summary>
            FilterStringLess,
            /// <summary>
            /// 
            /// </summary>
            FilterStringLessOrEqual
        };

        /// <summary>
        /// 
        /// </summary>
        [SupressImportIntoVM]
        public enum NumericRules
        {
            /// <summary>
            /// 
            /// </summary>
            FilterNumericEquals,
            /// <summary>
            /// 
            /// </summary>
            FilterNumericGreater,
            /// <summary>
            /// 
            /// </summary>
            FilterNumericGreaterOrEqual,
            /// <summary>
            /// 
            /// </summary>
            FilterNumericLess,
            /// <summary>
            /// 
            /// </summary>
            FilterNumericLessOrEqual
        };
    }
}
