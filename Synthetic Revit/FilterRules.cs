using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using revitDB = Autodesk.Revit.DB;

using synthEnum = Synthetic.Core.Enumeration;

namespace Synthetic.Revit
{
    /// <summary>
    /// 
    /// </summary>
    public class FilterRule
    {
        internal revitDB.ElementId _parameterId { get; private set; }
        internal revitDB.ParameterValueProvider _valueProvider { get; private set; }
        internal Type _parameterStorageType { get; private set; }
        internal EvaluatorType _evaluator { get; private set; }
        internal object _value { get; private set; }

        internal FilterRule (revitDB.ElementId parameterId, EvaluatorType evaluator, object value)
        {
            _parameterId = parameterId;

            _evaluator = evaluator;
            _value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="evaluator"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FilterRule ByParameterName(string parameterName, EvaluatorType evaluator, object value)
        {

            revitDB.ElementId parameterId = new revitDB.ElementId((revitDB.BuiltInParameter)Enum.Parse(typeof(revitDB.BuiltInParameter), parameterName));
            return new FilterRule(parameterId, evaluator, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterId"></param>
        /// <param name="evaluator"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FilterRule ByParameterId (int parameterId, EvaluatorType evaluator, object value)
        {
            return new FilterRule(new revitDB.ElementId(parameterId), evaluator, value);
        }

        public static revitDB.FilterRule GetFilterRule(FilterRule)
        {
            
        }

        #region Enumeration EvaluatorType
        /// <summary>
        /// 
        /// </summary>
        public enum EvaluatorType { Equals, Greater, GreaterOrEqual, Less, LessOrEqual, Contains, BeginsWith, EndsWith }

        public static EvaluatorType GetEvaluatorType (string name)
        {
            return (EvaluatorType)Enum.Parse(typeof(EvaluatorType), name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string[] GetEvaluatorTypes()
        {
            return Enum.GetNames(typeof(EvaluatorType));
        }
        #endregion
    }
}
