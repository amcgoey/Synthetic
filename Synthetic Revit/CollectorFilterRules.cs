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
        [SupressImportIntoVM]
        public revitDB.FilterStringRuleEvaluator FilterStringRules (string ruleName)
        {
            switch (ruleName)
            {
                case "FilterStringBeginsWith":
                    return new revitDB.FilterStringBeginsWith();
                case "FilterStringContains":
                    return new revitDB.FilterStringContains();
                case "FilterStringEndsWith":
                    return new revitDB.FilterStringEndsWith();
                case "FilterStringEquals":
                    return new revitDB.FilterStringEquals();
                case "FilterStringGreater":
                    return new revitDB.FilterStringGreater();
                case "FilterStringGreaterOrEqual":
                    return new revitDB.FilterStringGreaterOrEqual();
                case "FilterStringLess":
                    return new revitDB.FilterStringLess();
                case "FilterStringLessOrEqual":
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
        [SupressImportIntoVM]
        public revitDB.FilterNumericRuleEvaluator FilterNumericRules(string ruleName)
        {
            switch (ruleName)
            {
                case "FilterNumericEquals":
                    return new revitDB.FilterNumericEquals();
                case "FilterNumericGreater":
                    return new revitDB.FilterNumericGreater();
                case "FilterNumericGreaterOrEqual":
                    return new revitDB.FilterNumericGreaterOrEqual();
                case "FilterNumericLess":
                    return new revitDB.FilterNumericLess();
                case "FilterNumericLessOrEqual":
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
