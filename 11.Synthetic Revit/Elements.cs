using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using RevitServices.Transactions;

using Revit.Elements;
using DynElem = Revit.Elements.Element;
using DynParam = Revit.Elements.Parameter;

using RevitDB = Autodesk.Revit.DB;
using RevitElem = Autodesk.Revit.DB.Element;
using RevitElemId = Autodesk.Revit.DB.ElementId;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitFaceArray = Autodesk.Revit.DB.FaceArray;
using RevitFace = Autodesk.Revit.DB.Face;
using RevitGeo = Autodesk.Revit.DB.GeometryObject;

using Synthetic.Core;
using synthDict = Synthetic.Core.Dictionary;

namespace Synthetic.Revit
{
    /// <summary>
    /// Manipulation and modification of Dynamo wrapped Revit elements.
    /// </summary>
    [IsDesignScriptCompatible]
    public class Elements
    {
        /// <summary>
        /// Dummy constructor for the class.  Not used.
        /// </summary>
        internal Elements () { }

        /// <summary>
        /// Gets an elements document
        /// </summary>
        /// <param name="Element">A dynamo wrapped element</param>
        /// <returns name="Document">A Autodesk.Revit.DB.Document</returns>
        public static RevitDoc Document (DynElem Element)
        {
            return Element.InternalElement.Document;
        }

        /// <summary>
        /// Sets an element's parameters based on a Dictionary object with the Key being the parameter name and the Value being the parameter value.
        /// </summary>
        /// <param name="element">A Dynamo wrapped element.</param>
        /// <param name="dictionary">A Synthetic Dictionary</param>
        /// <returns></returns>
        public static DynElem SetParamterByDictionary(DynElem element, synthDict dictionary)
        {
            RevitDoc doc = element.InternalElement.Document;

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
        /// <param name="parameterNames">A list of parameter names.</param>
        /// <returns></returns>
        public static synthDict GetParamterToDictionary(DynElem element, List<string> parameterNames)
        {
            synthDict dict = synthDict.ByEmpty();
            RevitDoc doc = element.InternalElement.Document;

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
        public static DynElem TransferParameters(DynElem Element, DynElem SourceElement)
        {
            string transactionName = "Transfer Parameter Between Elements";

            Action<DynElem, DynElem> transfer = (sElem, dElem) =>
            {
                _transferParameters(sElem.InternalElement, dElem.InternalElement);
            };

            RevitDoc document = Element.InternalElement.Document;

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
        public static List<RevitElemId> CopyElements (RevitDoc sourceDoc, List<int> elementIds, RevitDoc destinationDoc)
        {
            string transactionName = "Copy Elements from document " + sourceDoc.Title;
            List<RevitElemId> copiedElemsIds;
            List<RevitElemId> revitElemIds = new List<RevitElemId>();
            
            Func<RevitDoc, List<RevitElemId>, RevitDoc, List<RevitElemId>> copy = (sDoc, elemIds, dDoc) =>
            {
                Autodesk.Revit.DB.CopyPasteOptions cpo = new Autodesk.Revit.DB.CopyPasteOptions();
                return (List<RevitElemId>)Autodesk.Revit.DB.ElementTransformUtils.CopyElements(sDoc, elemIds, dDoc, null, cpo);
            };

            foreach (int id in elementIds)
            {
                revitElemIds.Add(new RevitElemId(id));
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
        public static RevitElem TransferElements(
            RevitElem Element,
            RevitElem SourceElement)
        {
            RevitDoc destinationDoc = Element.Document;
            RevitDoc sourceDoc = SourceElement.Document;

            string transactionName = "Element overwritten from " + sourceDoc.Title;

            RevitElem returnElem;
            
            Func<RevitElem, RevitDoc, RevitElem, RevitDoc, RevitElem> transfer = (dElem, dDoc, sElem, sDoc) =>
            {
                List<RevitElemId> revitElemIds = new List<RevitElemId>();
                revitElemIds.Add(sElem.Id);

                Autodesk.Revit.DB.CopyPasteOptions cpo = new Autodesk.Revit.DB.CopyPasteOptions();
                List<RevitElemId> ids = (List<RevitElemId>)Autodesk.Revit.DB.ElementTransformUtils.CopyElements(sDoc, revitElemIds, dDoc, null, cpo);

                RevitElem tempElem = dDoc.GetElement(ids[0]);
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
        /// Changes an element's subcategory.  Only works inside of Family documents
        /// </summary>
        /// <param name="element">Element to change</param>
        /// <param name="category">Subcategory to set the element too.</param>
        /// <returns name="Suceeded">If the element's category was changed.</returns>
        /// <returns name="Failed">If the change in category failed.</returns>
        [MultiReturn(new[] { "Succeeded", "Failed" })]
        public static IDictionary SetCategory (DynElem element, Category category)
        {
            //  Name of Transaction
            string transactionName = "Set Element Category to" + category.Name;

            RevitDoc document = element.InternalElement.Document;

            // Intialize list for elements that are successfully merged and failed to merge.
            List<DynElem> elementsMerged = new List<DynElem>();
            List<DynElem> elementsFailed = new List<DynElem>();

            // Define Function to change element's category.
            Action<DynElem, Category> _SetCategory = (elem, cat) =>
            {
                // If Element is in a group, put the element in the failed list
                int groupId = elem.InternalElement.GroupId.IntegerValue;
                if (groupId == -1)
                {
                    //elem.TextNoteType = rToType;
                    RevitDB.Parameter parameter = elem.InternalElement.get_Parameter(RevitDB.BuiltInParameter.FAMILY_ELEM_SUBCATEGORY);
                    parameter.Set(cat.RevitCategory.Id);
                    elementsMerged.Add(elem);
                }
                else
                {
                    elementsFailed.Add(elem);
                }
            };

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                _SetCategory(element, category);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    _SetCategory(element, category);
                    trans.Commit();
                }
            }

            return new Dictionary<string, object>
            {
                {"Succeeded", elementsMerged},
                {"Failed", elementsFailed}
            };
        }

        /// <summary>
        /// Merges ElementType FromType into ToType.  FromType will be deleted if all instances of the Type are successfully changed.  Elements in groups will not be changed.
        /// </summary>
        /// <param name="FromType">All instances of this ElementType will be merged into the ToType and the Type will be deleted.</param>
        /// <param name="ToType">ElementType to merge into.</param>
        /// <returns name="Merged">A list of instances that were successfully changed to ToType</returns>
        /// <returns name="Failed">A list of instances that failed to changed to ToType</returns>
        [MultiReturn(new[] { "Merged", "Failed" })]
        public static IDictionary MergeElementTypes(DynElem FromType, DynElem ToType)
        {
            //  Name of Transaction
            string transactionName = "Merge Element Type";

            // Get the Revit elements from the Dynamo Elements
            RevitDB.ElementType rFromType = (RevitDB.ElementType)FromType.InternalElement;
            RevitDB.ElementType rToType = (RevitDB.ElementType)ToType.InternalElement;

            RevitDoc document = rToType.Document;

            // Collect all instances of FromType
            IEnumerable<RevitDB.Element> instances = Select.GetInstancesFromElemType_Revit(rFromType, document);

            // Intialize list for elements that are successfully merged and failed to merge.
            List<DynElem> elementsMerged = new List<DynElem>();
            List<DynElem> elementsFailed = new List<DynElem>();

            // Define Function to change instances types.
            Action<IEnumerable<RevitDB.Element>> _SetType = (elements) =>
            {
                foreach (RevitDB.Element elem in elements)
                {
                    // If Element is in a group, put the element in the failed list
                    int groupId = elem.GroupId.IntegerValue;
                    if (groupId == -1)
                    {
                        //elem.TextNoteType = rToType;
                        RevitDB.Parameter param = elem.get_Parameter(RevitDB.BuiltInParameter.ELEM_TYPE_PARAM);
                        param.Set(rToType.Id);
                        DynElem dElem = elem.ToDSType(true);
                        elementsMerged.Add(dElem);
                    }
                    else
                    {
                        DynElem dElem = elem.ToDSType(true);
                        elementsFailed.Add(dElem);
                    }
                }

                // Check if there are any instances of FromType left
                int count = elements.Count();
                if (count == 0)
                {
                    document.Delete(rFromType.Id);
                }
            };

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                _SetType(instances);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    _SetType(instances);
                    trans.Commit();
                }
            }

            return new Dictionary<string, object>
            {
                {"Merged", elementsMerged},
                {"Failed", elementsFailed}
            };
        }

        /// <summary>
        /// Merges ElementType FromType into ToType.  FromType will be deleted if all instances of the Type are successfully changed.  Elements in groups will not be changed.
        /// </summary>
        /// <param name="FromType">All instances of this ElementType will be merged into the ToType and the Type will be deleted.</param>
        /// <param name="ToType">ElementType to merge into.</param>
        /// <returns name="Merged">A list of instances that were successfully changed to ToType</returns>
        /// <returns name="Failed">A list of instances that failed to changed to ToType</returns>
        [MultiReturn(new[] { "Merged", "Failed" })]
        public static IDictionary MergeElementTypesRevit(RevitElem FromType, RevitElem ToType)
        {
            //  Name of Transaction
            string transactionName = "Merge Element Type";

            RevitDoc document = ToType.Document;

            // Collect all instances of FromType
            IEnumerable<RevitDB.Element> instances = Select.GetInstancesFromElemType_Revit(FromType, document);

            // Intialize list for elements that are successfully merged and failed to merge.
            List<RevitElem> elementsMerged = new List<RevitElem>();
            List<RevitElem> elementsFailed = new List<RevitElem>();

            // Define Function to change instances types.
            Action<IEnumerable<RevitDB.Element>> _SetType = (elements) =>
            {
                foreach (RevitDB.Element elem in elements)
                {
                    // If Element is in a group, put the element in the failed list
                    int groupId = elem.GroupId.IntegerValue;
                    if (groupId == -1)
                    {
                        //elem.TextNoteType = rToType;
                        RevitDB.Parameter param = elem.get_Parameter(RevitDB.BuiltInParameter.ELEM_TYPE_PARAM);
                        param.Set(ToType.Id);
                        elementsMerged.Add(elem);
                    }
                    else
                    {
                        elementsFailed.Add(elem);
                    }
                }

                // Check if there are any instances of FromType left
                int count = elements.Count();
                if (count == 0)
                {
                    document.Delete(FromType.Id);
                }
            };

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                _SetType(instances);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    _SetType(instances);
                    trans.Commit();
                }
            }

            return new Dictionary<string, object>
            {
                {"Merged", elementsMerged},
                {"Failed", elementsFailed}
            };
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
        public static bool FilterByParameterValue(DynElem element, List<string> parameterNames, string value)
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
                    DynElem e = (DynElem)valueParam;
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
        public static RevitElem GetByElementId (RevitElemId elementId, RevitDoc document)
        {
            return document.GetElement(elementId);
        }

        /// <summary>
        /// Gets an element given its UniqueId
        /// </summary>
        /// <param name="UniqueId">A UniqueId as a string</param>
        /// <param name="document">Document that the element is in.</param>
        /// <returns name="Element">Returns a unwrapped Autodesk.Revit.DB.Element</returns>
        public static RevitElem GetByUniqueId(string UniqueId, RevitDoc document)
        {
            return document.GetElement(UniqueId);
        }

        /// <summary>
        /// Gets an element given its UniqueId
        /// </summary>
        /// <param name="UniqueId">A UniqueId as a string</param>
        /// <param name="document">Document that the element is in.</param>
        /// <returns name="Element">Returns a wrapped Dynamo Revit Element</returns>
        public static DynElem GetDynamoByUniqueId(string UniqueId, RevitDoc document)
        {
            RevitElem elem = document.GetElement(UniqueId);
            return WrapRevitElement(elem);
        }

        /// <summary>
        /// Gets a Element's ElementId
        /// </summary>
        /// <param name="Element">A Autodesk.Revit.DB.Element, NOT a Dynamo wrapped element</param>
        /// <returns name="ElementId">The Autodesk.Revit.DB.ElementId</returns>
        public static RevitElemId Id(System.Object Element)
        {
            RevitElem elem = (RevitElem)Element;
            return elem.Id;
        }

        /// <summary>
        /// Gets a Element's name
        /// </summary>
        /// <param name="Element">A Autodesk.Revit.DB.Element, NOT a Dynamo wrapped element</param>
        /// <returns name="Name">The name of the element</returns>
        public static string Name(System.Object Element)
        {
            RevitElem elem = (RevitElem)Element;
            return elem.Name;
        }

        /// <summary>
        /// Gets a Element's UniqueId
        /// </summary>
        /// <param name="Element">A Autodesk.Revit.DB.Element, NOT a Dynamo wrapped element</param>
        /// <returns name="UniqueId">The UniqueId of the element</returns>
        public static string UniqueId(System.Object Element)
        {
            RevitElem elem = (RevitElem)Element;
            return elem.UniqueId;
        }

        /// <summary>
        /// If the object 
        /// </summary>
        /// <param name="Object"></param>
        /// <returns></returns>
        public static RevitElem CastRevitElement(System.Object Object)
        {
            return (RevitElem)Object;
        }

        /// <summary>
        /// Paints every face in an element with a material
        /// </summary>
        /// <param name="Element">The element to paint</param>
        /// <param name="MaterialId">The material to paint</param>
        /// <returns name="Element">The modified element</returns>
        public static DynElem PaintElement (DynElem Element, RevitElemId MaterialId)
        {
            string transactionName = "Paint Faces of Element";

            RevitDoc document = Element.InternalElement.Document;
            RevitElemId elementId = Element.InternalElement.Id;

            RevitDB.Options op = new RevitDB.Options();
            RevitDB.GeometryElement geoElem = Element.InternalElement.get_Geometry(op);

            IList<RevitDB.Face> faces = new List<RevitDB.Face>();

            foreach (RevitDB.GeometryObject geo in  geoElem)
            {
                if (geo is RevitDB.Solid)
                {
                    RevitDB.Solid solid = geo as RevitDB.Solid;
                    foreach (RevitDB.Face face in solid.Faces)
                    {
                        faces.Add(face);
                    }

                    }
                else if (geo is RevitDB.Face)
                {
                    faces.Add(geo as RevitDB.Face);
                }

                Action<RevitElemId, IList<RevitDB.Face>, RevitElemId> paintFaces = (RevitElemId elemId, IList<RevitDB.Face> faceList, RevitElemId matId) =>
                {
                    foreach (RevitDB.Face face in faceList)
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
        public static DynElem RemovePaintElement(DynElem Element)
        {
            string transactionName = "Remove Painted Faces of Element";

            RevitDoc document = Element.InternalElement.Document;
            RevitElemId elementId = Element.InternalElement.Id;

            RevitDB.Options op = new RevitDB.Options();
            RevitDB.GeometryElement geoElem = Element.InternalElement.get_Geometry(op);

            IList<RevitDB.Face> faces = new List<RevitDB.Face>();

            foreach (RevitDB.GeometryObject geo in geoElem)
            {
                if (geo is RevitDB.Solid)
                {
                    RevitDB.Solid solid = geo as RevitDB.Solid;
                    foreach (RevitDB.Face face in solid.Faces)
                    {
                        faces.Add(face);
                    }

                }
                else if (geo is RevitDB.Face)
                {
                    faces.Add(geo as RevitDB.Face);
                }

                Action<RevitElemId, IList<RevitDB.Face>> RemovePaintedFaces = (RevitElemId elemId, IList<RevitDB.Face> faceList) =>
                {
                    foreach (RevitDB.Face face in faceList)
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
        public static DynElem WrapRevitElement ( RevitElem element)
        {
            if (element != null)
            {
                DynElem dElem = null;

                try
                {
                    dElem = element.ToDSType(true);
                }
                catch { }

                return dElem;
            }
            else return null;
        }

        /// <summary>
        /// Converts a Dynamo element into its equivalent Autodesk.Revit.DB.Element if possible.
        /// </summary>
        /// <param name="element">A Dynamo representation of a revit element</param>
        /// <returns name="Revit Element">If successful, returns a Autodesk.Revit.DB.Element, otherwise returns null.</returns>
        public static RevitElem UnwrapDynamoElement (DynElem element)
        {
            RevitElem rElem = null;

            try
            {
                rElem = element.InternalElement;
            }

            catch { }

            return rElem;
        }

#region Helper Functions

        private static void _transferParameters(RevitElem SourceElement, RevitElem DestinationElement)
        {
            RevitDB.ParameterSet sourceParameters = SourceElement.Parameters;

            foreach (RevitDB.Parameter sourceParam in sourceParameters)
            {
                if (sourceParam.IsReadOnly == false)
                {
                    RevitDB.Definition def = sourceParam.Definition;
                    RevitDB.Parameter destinationParam = DestinationElement.get_Parameter(def);

                    RevitDB.StorageType st = sourceParam.StorageType;
                    switch (st)
                    {
                        case RevitDB.StorageType.Double:
                            destinationParam.Set(sourceParam.AsDouble());
                            break;
                        case RevitDB.StorageType.ElementId:
                            destinationParam.Set(sourceParam.AsElementId());
                            break;
                        case RevitDB.StorageType.Integer:
                            destinationParam.Set(sourceParam.AsInteger());
                            break;
                        case RevitDB.StorageType.String:
                            destinationParam.Set(sourceParam.AsString());
                            break;
                    }
                }
            }
        }
#endregion

    }
}
