using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthetic.Revit.CollectorFilterRules
{
    /// <summary>
    /// 
    /// </summary>
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
