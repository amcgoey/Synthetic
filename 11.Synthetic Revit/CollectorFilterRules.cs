using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;

namespace Synthetic.Revit
{
    /// <summary>
    /// Class for constructing Filter Rule Evaluators
    /// </summary>
    public class CollectorFilterRules
    {
        internal CollectorFilterRules () { }

        /// <summary>
        /// Given a rule name, returns that type of FilterStringRuleEvaluator.
        /// Possible values include "BeginsWith", "Contains", "EndsWith", "Equals", "Greater", "GreaterOrEqual", "Less", "LessOrEqual"
        /// </summary>
        /// <param name="ruleName">Name of the FilterStringRuleEvaluator.  Possible values include "BeginsWith", "Contains", "EndsWith", "Equals", "Greater", "GreaterOrEqual", "Less", "LessOrEqual"</param>
        /// <returns name="StringRule">A Revit FilterStringRuleEvaluator</returns>
        public static revitDB.FilterStringRuleEvaluator FilterStringRules (string ruleName)
        {
            switch (ruleName)
            {
                case "Autodesk.Revit.DB.FilterStringBeginsWith":
                case "FilterStringBeginsWith":
                case "BeginsWith":
                    return new revitDB.FilterStringBeginsWith();
                case "Autodesk.Revit.DB.FilterStringContains":
                case "FilterStringContains":
                case "Contains":
                    return new revitDB.FilterStringContains();
                case "Autodesk.Revit.DB.FilterStringEndsWith":
                case "FilterStringEndsWith":
                case "EndsWith":
                    return new revitDB.FilterStringEndsWith();
                case "Autodesk.Revit.DB.FilterStringEquals":
                case "FilterStringEquals":
                case "Equals":
                    return new revitDB.FilterStringEquals();
                case "Autodesk.Revit.DB.FilterStringGreater":
                case "FilterStringGreater":
                case "Greater":
                    return new revitDB.FilterStringGreater();
                case "Autodesk.Revit.DB.FilterStringGreaterOrEqual":
                case "FilterStringGreaterOrEqual":
                case "GreaterOrEqual":
                    return new revitDB.FilterStringGreaterOrEqual();
                case "Autodesk.Revit.DB.FilterStringLess":
                case "FilterStringLess":
                case "Less":
                    return new revitDB.FilterStringLess();
                case "Autodesk.Revit.DB.FilterStringLessOrEqual":
                case "FilterStringLessOrEqual":
                case "LessOrEqual":
                    return new revitDB.FilterStringLessOrEqual();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Given a rule name, returns that type of FilterNumericRuleEvaluator.
        /// Possible values include "Equals", "Greater", "GreaterOrEqual", "Less", "LessOrEqual"
        /// </summary>
        /// <param name="ruleName">Name of the FilterNumericRuleEvaluator.  Possible values include "Equals", "Greater", "GreaterOrEqual", "Less", "LessOrEqual"</param>
        /// <returns name="NumericRule">A Revit FilterNumbericRuleEvaluator</returns>
        public static revitDB.FilterNumericRuleEvaluator FilterNumericRules(string ruleName)
        {
            switch (ruleName)
            {
                case "Autodesk.Revit.DB.FilterNumericEquals":
                case "FilterNumericEquals":
                case "Equals":
                    return new revitDB.FilterNumericEquals();
                case "Autodesk.Revit.DB.FilterNumericGreater":
                case "FilterNumericGreater":
                case "Greater":
                    return new revitDB.FilterNumericGreater();
                case "Autodesk.Revit.DB.FilterNumericGreaterOrEqual":
                case "FilterNumericGreaterOrEqual":
                case "GreaterOrEqual":
                    return new revitDB.FilterNumericGreaterOrEqual();
                case "Autodesk.Revit.DB.FilterNumericLess":
                case "FilterNumericLess":
                case "Less":
                    return new revitDB.FilterNumericLess();
                case "Autodesk.Revit.DB.FilterNumericLessOrEqual":
                case "FilterNumericLessOrEqual":
                case "LessOrEqual":
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
        /// Enumertion of different String Rules
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
        /// Enumertion of different Numeric Rules
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
