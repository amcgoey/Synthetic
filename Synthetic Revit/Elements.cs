using System;
using System.Collections.Generic;

using Synthetic.Core;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

using Revit.Elements;
using RevitServices.Transactions;
using dynamoElem = Revit.Elements.Element;
using dynamoParam = Revit.Elements.Parameter;
using dynRevit = Revit.Elements;

using revitDB = Autodesk.Revit.DB;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitDoc = Autodesk.Revit.DB.Document;
using revitFaceArray = Autodesk.Revit.DB.FaceArray;
using revitFace = Autodesk.Revit.DB.Face;
using revitGeo = Autodesk.Revit.DB.GeometryObject;

using synthDict = Synthetic.Core.Dictionary;

namespace Synthetic.Revit
{
    /// <summary>
    /// Manipulation and modification of Dynamo wrapped Revit elements.
    /// </summary>
    [IsDesignScriptCompatible]
    public class Elements
    {
        internal Elements () { }

        /// <summary>
        /// Gets an elements document
        /// </summary>
        /// <param name="Element">A dynamo wrapped element</param>
        /// <returns name="Document">A Autodesk.Revit.DB.Document</returns>
        public static revitDoc Document (dynamoElem Element)
        {
            return Element.InternalElement.Document;
        }

        /// <summary>
        /// Sets an element's parameters based on a Dictionary object with the Key being the parameter name and the Value being the parameter value.
        /// </summary>
        /// <param name="element">A Dynamo wrapped element.</param>
        /// <param name="dictionary">A Synthetic Dictionary</param>
        /// <returns></returns>
        public static dynamoElem SetParamterByDictionary(dynamoElem element, synthDict dictionary)
        {
            revitDoc doc = element.InternalElement.Document;

            TransactionManager.Instance.EnsureInTransaction(doc);

            foreach (KeyValuePair<string, object> keyValue in synthDict.UnwrapDictionary(dictionary))
            {
                if (element.InternalElement.GetParameters(keyValue.Key)[0].IsReadOnly == false)
                {
                    element.SetParameterByName(keyValue.Key, keyValue.Value);
                }
            }

            TransactionManager.Instance.TransactionTaskDone();

            return element;
        }

        /// <summary>
        /// Gets the listed parameters of an element and returns a Dictionary with the Key being the parameter name and the Value being the parameter value.
        /// </summary>
        /// <param name="element">A Dynamo wrapped element.</param>
        /// <param name="dictionary">A Synthetic Dictionary</param>
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
        /// Overwrite an elements parameters with the parameter values from a source element.
        /// </summary>
        /// <param name="Element">Destination element</param>
        /// <param name="SourceElement">Source element for the parameter values</param>
        /// <returns name="Element">The destination element</returns>
        public static dynamoElem TransferParameters(dynamoElem Element, dynamoElem SourceElement)
        {
            string transactionName = "Transfer Parameter Between Elements";

            Action<dynamoElem, dynamoElem> transfer = (sElem, dElem) =>
            {
                _transferParameters(sElem.InternalElement, dElem.InternalElement);
            };

            revitDoc document = Element.InternalElement.Document;

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                transfer(SourceElement, Element);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    transfer(SourceElement, Element);
                    trans.Commit();
                }
            }
            return Element;
        }

        /// <summary>
        /// Copy elements to the same location between documents.  Can be used to copy system types or view templates between documents.  Model elements are copied in the same location.  If the elements already exist, Revit will give you an option to either duplicate the types or cancel the operation.  Please note that documents are to be a Autodesk.Revit.DB.Document objects, not a Dynamo wrapped Revit Document.
        /// </summary>
        /// <param name="sourceDoc">The source document to copy items from.</param>
        /// <param name="elementIds">List of Element Ids of elements to be copied.</param>
        /// <param name="destinationDoc">The destination document.</param>
        /// <returns></returns>
        public static List<revitElemId> CopyElements (revitDoc sourceDoc, List<int> elementIds, revitDoc destinationDoc)
        {
            string transactionName = "Copy Elements from document " + sourceDoc.Title;
            List<revitElemId> copiedElemsIds;
            List<revitElemId> revitElemIds = new List<revitElemId>();
            
            Func<revitDoc, List<revitElemId>, revitDoc, List<revitElemId>> copy = (sDoc, elemIds, dDoc) =>
            {
                Autodesk.Revit.DB.CopyPasteOptions cpo = new Autodesk.Revit.DB.CopyPasteOptions();
                return (List<revitElemId>)Autodesk.Revit.DB.ElementTransformUtils.CopyElements(sDoc, elemIds, dDoc, null, cpo);
            };

            foreach (int id in elementIds)
            {
                revitElemIds.Add(new revitElemId(id));
            }

            if (destinationDoc.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(destinationDoc);
                copiedElemsIds = copy(sourceDoc, revitElemIds, destinationDoc);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(destinationDoc))
                {
                    trans.Start(transactionName);
                    copiedElemsIds = copy(sourceDoc, revitElemIds, destinationDoc);
                    trans.Commit();
                }
            }

            return copiedElemsIds;
        }

        /// <summary>
        /// Overwrites the parameters of an element with the parameters of an element from a different document.  Associated elements such as materials may be duplicated in the document.
        /// </summary>
        /// <param name="Element"></param>
        /// <param name="SourceElement"></param>
        /// <returns></returns>
        public static revitElem TransferElements(
            revitElem Element,
            revitElem SourceElement)
        {
            revitDoc destinationDoc = Element.Document;
            revitDoc sourceDoc = SourceElement.Document;

            string transactionName = "Element overwritten from " + sourceDoc.Title;

            revitElem returnElem;
            
            Func<revitElem, revitDoc, revitElem, revitDoc, revitElem> transfer = (dElem, dDoc, sElem, sDoc) =>
            {
                List<revitElemId> revitElemIds = new List<revitElemId>();
                revitElemIds.Add(sElem.Id);

                Autodesk.Revit.DB.CopyPasteOptions cpo = new Autodesk.Revit.DB.CopyPasteOptions();
                List<revitElemId> ids = (List<revitElemId>)Autodesk.Revit.DB.ElementTransformUtils.CopyElements(sDoc, revitElemIds, dDoc, null, cpo);

                revitElem tempElem = dDoc.GetElement(ids[0]);
                _transferParameters(tempElem, dElem);
                destinationDoc.Delete(tempElem.Id);

                return dElem;
            };

            if (destinationDoc.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(destinationDoc);
                returnElem = transfer(Element, destinationDoc, SourceElement, sourceDoc);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(destinationDoc))
                {
                    trans.Start(transactionName);
                    returnElem = transfer(Element, destinationDoc, SourceElement, sourceDoc);
                    trans.Commit();
                }
            }

            return returnElem;
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
        /// Gets an element given the ElementId
        /// </summary>
        /// <param name="elementId">A Autodesk.Revit.DB.ElementId</param>
        /// <param name="document">Document that the element is in.</param>
        /// <returns name="Element">Returns a unwrapped Autodesk.Revit.DB.Element</returns>
        public static revitElem GetByElementId (revitElemId elementId, revitDoc document)
        {
            return document.GetElement(elementId);
        }

        /// <summary>
        /// Gets an element given its UniqueId
        /// </summary>
        /// <param name="UniqueId">A UniqueId as a string</param>
        /// <param name="document">Document that the element is in.</param>
        /// <returns name="Element">Returns a unwrapped Autodesk.Revit.DB.Element</returns>
        public static revitElem GetByUniqueId(string UniqueId, revitDoc document)
        {
            return document.GetElement(UniqueId);
        }

        /// <summary>
        /// Gets a Element's ElementId
        /// </summary>
        /// <param name="Element">A Autodesk.Revit.DB.Element, NOT a Dynamo wrapped element</param>
        /// <returns name="ElementId">The Autodesk.Revit.DB.ElementId</returns>
        public static revitElemId Id(System.Object Element)
        {
            revitElem elem = (revitElem)Element;
            return elem.Id;
        }

        /// <summary>
        /// Gets a Element's name
        /// </summary>
        /// <param name="Element">A Autodesk.Revit.DB.Element, NOT a Dynamo wrapped element</param>
        /// <returns name="Name">The name of the element</returns>
        public static string Name(System.Object Element)
        {
            revitElem elem = (revitElem)Element;
            return elem.Name;
        }

        /// <summary>
        /// Gets a Element's UniqueId
        /// </summary>
        /// <param name="Element">A Autodesk.Revit.DB.Element, NOT a Dynamo wrapped element</param>
        /// <returns name="UniqueId">The UniqueId of the element</returns>
        public static string UniqueId(System.Object Element)
        {
            revitElem elem = (revitElem)Element;
            return elem.UniqueId;
        }

        /// <summary>
        /// If the object 
        /// </summary>
        /// <param name="Object"></param>
        /// <returns></returns>
        public static revitElem CastRevitElement(System.Object Object)
        {
            return (revitElem)Object;
        }

        /// <summary>
        /// Paints every face in an element with a material
        /// </summary>
        /// <param name="Element">The element to paint</param>
        /// <param name="MaterialId">The material to paint</param>
        /// <returns name="Element">The modified element</returns>
        public static dynamoElem PaintElement (dynamoElem Element, revitElemId MaterialId)
        {
            string transactionName = "Paint Faces of Element";

            revitDoc document = Element.InternalElement.Document;
            revitElemId elementId = Element.InternalElement.Id;

            revitDB.Options op = new revitDB.Options();
            revitDB.GeometryElement geoElem = Element.InternalElement.get_Geometry(op);

            IList<revitDB.Face> faces = new List<revitDB.Face>();

            foreach (revitDB.GeometryObject geo in  geoElem)
            {
                if (geo is revitDB.Solid)
                {
                    revitDB.Solid solid = geo as revitDB.Solid;
                    foreach (revitDB.Face face in solid.Faces)
                    {
                        faces.Add(face);
                    }

                    }
                else if (geo is revitDB.Face)
                {
                    faces.Add(geo as revitDB.Face);
                }

                Action<revitElemId, IList<revitDB.Face>, revitElemId> paintFaces = (revitElemId elemId, IList<revitDB.Face> faceList, revitElemId matId) =>
                {
                    foreach (revitDB.Face face in faceList)
                    {
                        document.Paint(elemId, face, matId);
                    }
                };

                if (document.IsModifiable)
                {
                    TransactionManager.Instance.EnsureInTransaction(document);
                    paintFaces(elementId, faces, MaterialId);
                    TransactionManager.Instance.TransactionTaskDone();
                }
                else
                {
                    using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                    {
                        trans.Start(transactionName);
                        paintFaces(elementId, faces, MaterialId);
                        trans.Commit();
                    }
                }
            }

            return Element;
        }

        /// <summary>
        /// Removes all painted faces on an elementl
        /// </summary>
        /// <param name="Element">The element to removve painted faces</param>
        /// <returns name="Element">The modified element</returns>
        public static dynamoElem RemovePaintElement(dynamoElem Element)
        {
            string transactionName = "Remove Painted Faces of Element";

            revitDoc document = Element.InternalElement.Document;
            revitElemId elementId = Element.InternalElement.Id;

            revitDB.Options op = new revitDB.Options();
            revitDB.GeometryElement geoElem = Element.InternalElement.get_Geometry(op);

            IList<revitDB.Face> faces = new List<revitDB.Face>();

            foreach (revitDB.GeometryObject geo in geoElem)
            {
                if (geo is revitDB.Solid)
                {
                    revitDB.Solid solid = geo as revitDB.Solid;
                    foreach (revitDB.Face face in solid.Faces)
                    {
                        faces.Add(face);
                    }

                }
                else if (geo is revitDB.Face)
                {
                    faces.Add(geo as revitDB.Face);
                }

                Action<revitElemId, IList<revitDB.Face>> RemovePaintedFaces = (revitElemId elemId, IList<revitDB.Face> faceList) =>
                {
                    foreach (revitDB.Face face in faceList)
                    {
                        document.RemovePaint(elemId, face);
                    }
                };

                if (document.IsModifiable)
                {
                    TransactionManager.Instance.EnsureInTransaction(document);
                    RemovePaintedFaces(elementId, faces);
                    TransactionManager.Instance.TransactionTaskDone();
                }
                else
                {
                    using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                    {
                        trans.Start(transactionName);
                        RemovePaintedFaces(elementId, faces);
                        trans.Commit();
                    }
                }
            }

            return Element;
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

#region Helper Functions

        private static void _transferParameters(revitElem SourceElement, revitElem DestinationElement)
        {
            revitDB.ParameterSet sourceParameters = SourceElement.Parameters;

            foreach (revitDB.Parameter sourceParam in sourceParameters)
            {
                if (sourceParam.IsReadOnly == false)
                {
                    revitDB.Definition def = sourceParam.Definition;
                    revitDB.Parameter destinationParam = DestinationElement.get_Parameter(def);

                    revitDB.StorageType st = sourceParam.StorageType;
                    switch (st)
                    {
                        case revitDB.StorageType.Double:
                            destinationParam.Set(sourceParam.AsDouble());
                            break;
                        case revitDB.StorageType.ElementId:
                            destinationParam.Set(sourceParam.AsElementId());
                            break;
                        case revitDB.StorageType.Integer:
                            destinationParam.Set(sourceParam.AsInteger());
                            break;
                        case revitDB.StorageType.String:
                            destinationParam.Set(sourceParam.AsString());
                            break;
                    }
                }
            }
        }
#endregion

    }
}
