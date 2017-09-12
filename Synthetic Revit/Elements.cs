using System;
using System.Collections.Generic;

using Synthetic.Core;

using Autodesk.DesignScript.Runtime;

using Revit.Elements;
using RevitServices.Transactions;
using dynamoElem = Revit.Elements.Element;
using dynamoParam = Revit.Elements.Parameter;

using revitDB = Autodesk.Revit.DB;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitDoc = Autodesk.Revit.DB.Document;

using synthDict = Synthetic.Core.Dictionary;

namespace Synthetic.Revit
{
    /// <summary>
    /// Manipulation and modification of Dynamo wrapped Revit elements.
    /// </summary>
    public class Elements
    {
        internal Elements () { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static dynamoElem SetParamterByDictionary(dynamoElem element, synthDict dictionary)
        {
            revitDoc doc = element.InternalElement.Document;

            TransactionManager.Instance.EnsureInTransaction(doc);
            //using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(doc))
            //{
            //    trans.Start("Set Parameters");

                foreach (KeyValuePair<string, object> keyValue in synthDict.UnwrapDictionary(dictionary))
                {
                    element.SetParameterByName(keyValue.Key, keyValue.Value);
                }
            //    trans.Commit();
            //}

            //End Transaction
            TransactionManager.Instance.TransactionTaskDone();

            return element;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="parameterNames"></param>
        /// <returns></returns>
        public static synthDict GetParamterToDictionary(dynamoElem element, List<string> parameterNames)
        {
            synthDict dict = synthDict.ByEmpty();
            revitDoc doc = element.InternalElement.Document;

            foreach (string name in parameterNames)
            {
                object value = element.GetParameterValueByName(name);
                if (value != null)
                {
                    synthDict.Add(dict, name, value);
                }
            }

            return dict;
        }

        /// <summary>
        /// Copy elements to the same location between documents.  Can be used to copy system types or view templates between documents.  Model elements are copied in the same location.  If the elements already exist, Revit will give you an option to either duplicate the types or cancel the operation.  Please note that documents are to be a Autodesk.Revit.DB.Document objects, not a Dynamo wrapped Revit Document.
        /// </summary>
        /// <param name="sourceDoc">The source document to copy items from.</param>
        /// <param name="elementIds">List of Element Ids of elements to be copied.</param>
        /// <param name="destinationDoc">The destination document.</param>
        /// <returns></returns>
        public static List<revitElemId> CopyElementsBetweenDocs (revitDoc sourceDoc, List<int> elementIds, revitDoc destinationDoc)
        {
            List<revitElemId> copiedElemsIds;
            List<revitElemId> revitElemIds = new List<revitElemId>();
            Autodesk.Revit.DB.CopyPasteOptions cpo = new Autodesk.Revit.DB.CopyPasteOptions();

            foreach (int id in elementIds)
            {
                revitElemIds.Add(new revitElemId(id));
            }

            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(destinationDoc))
            {
                try
                {
                    trans.Start("Copy Elements from document " + sourceDoc.Title);
                    copiedElemsIds = (List<revitElemId>)Autodesk.Revit.DB.ElementTransformUtils.CopyElements(sourceDoc, revitElemIds, destinationDoc, null, cpo);
                    trans.Commit();
                }
                catch
                {
                    copiedElemsIds = (List<revitElemId>)Autodesk.Revit.DB.ElementTransformUtils.CopyElements(sourceDoc, revitElemIds, destinationDoc, null, cpo);
                }
            }

            return copiedElemsIds;
        }

        /// <summary>
        /// Tests whether the element has a parameter of a given value.  Returns true if the parameter has an equal value and false otherwise.  A list of parameters names can be given to test against values in elements within parameters.  For example, one can test for a type description from a instance by creating a list of each parameter.  Please note that the comparision is done using the string representation of each parameter.
        /// </summary>
        /// <param name="element">A dynamo wrapped Revit element.</param>
        /// <param name="parameterNames">A list of parameter names.  The parameters in the list will each be retrieved iteratively.  So the first name is on the input element, the next name on the element returned from the first parameter and so on.</param>
        /// <param name="value">A parameter value as a string.</param>
        /// <returns name="Filter">True if the element's parameter equals the input value, otherwise false.</returns>
        //[MultiReturn(new[] { "Filter", "Debug" })]
        //public static Dictionary<string,object> FilterByParameterValue (dynamoElem element, List<string> parameterNames, string value )
        public static bool FilterByParameterValue(dynamoElem element, List<string> parameterNames, string value)
        {
            bool filter;
            object valueParam = element;

            //List<string> debug = new List<string>();

            foreach (string param in parameterNames)
            {
                Type vType = valueParam.GetType();
                //debug.Add("Param Type = " + vType.ToString());

                //if (vType == typeof(dynamoElem))
                //{
                //    dynamoElem e = (dynamoElem)valueParam;
                //    valueParam = e.GetParameterValueByName(param);
                //    debug.Add("Parameter = " + valueParam.ToString());
                
                //}

                if (vType != typeof(string) && vType != typeof(int) && vType != typeof(double))
                {
                    dynamoElem e = (dynamoElem)valueParam;
                    valueParam = e.GetParameterValueByName(param);
                    //debug.Add("Parameter = " + valueParam.ToString());
                }
            }

            //debug.Add("Parameter Value = " + valueParam.ToString());
            //debug.Add("Parameter Type = " + valueParam.GetType().ToString());
            //debug.Add("Filter Value = " + value.ToString());
            //debug.Add("Filter Value Type = " + value.GetType().ToString());

            if ( value.ToString() == valueParam.ToString() )
            {
                filter = true;
            }
            else
            {
                filter = false;
            }

            //debug.Add("Filter Bool = " + filter.ToString());
            //return new Dictionary<string, object>
            //{
            //    { "Filter", filter },
            //    { "Debug",  debug}
            //};
            return filter;
        }

        /// <summary>
        /// Converts a Autodesk.Revit.DB.Element into its equivalent Dynamo element if possible.
        /// </summary>
        /// <param name="element">A Autodesk.Revit.DB.Element</param>
        /// <returns name="Dynamo Element">If successful, returns a dynamo element, otherwise returns null.</returns>
        public static dynamoElem WrapRevitElement ( revitElem element)
        {
            dynamoElem dElem = null;

            try
            {
                dElem = element.ToDSType(true);
            }
            catch { }

            return dElem;
        }

        /// <summary>
        /// Converts a Dynamo element into its equivalent Autodesk.Revit.DB.Element if possible.
        /// </summary>
        /// <param name="element">A Dynamo representation of a revit element</param>
        /// <returns name="Revit Element">If successful, returns a Autodesk.Revit.DB.Element, otherwise returns null.</returns>
        public static revitElem UnwrapDynamoElement (dynamoElem element)
        {
            revitElem rElem = null;

            try
            {
                rElem = element.InternalElement;
            }

            catch { }

            return rElem;
        }
    }
}
