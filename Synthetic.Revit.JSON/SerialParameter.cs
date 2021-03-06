﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;

using Autodesk.Revit.DB;
using RevitParam = Autodesk.Revit.DB.Parameter;
using RevitElem = Autodesk.Revit.DB.Element;
using RevitElemId = Autodesk.Revit.DB.ElementId;
using RevitDoc = Autodesk.Revit.DB.Document;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialParameter : SerialObject
    {
        #region Public Properties

        public string Name { get; set; }
        public string Value { get; set; }
        public SerialElementId ValueElemId { get; set; }
        public string StorageType { get; set; }
        public int Id { get; set; }
        public string GUID { get; set; }
        public bool IsShared { get; set; }
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// If true, SerialParameter is intended to be deserialized as a template for use as standards or transfer to another project.
        /// If false, SerialParameter is intended to modify an element inside the project and will include ElementIds and UniqueIds.
        /// </summary>
        [JsonIgnoreAttribute]
        public bool IsTemplate { get; set; }

        #endregion

        #region Public Constructors

        [JsonConstructor]
        public SerialParameter(string Name, string Value, SerialElementId ValueElemId, string StorageType, int Id, string GUID, bool IsShared, bool IsReadOnly)
        {
            this.Name = Name;
            this.Value = Value;
            this.ValueElemId = ValueElemId;
            this.StorageType = StorageType;
            this.Id = Id;
            this.GUID = GUID;
            this.IsShared = IsShared;
            this.IsReadOnly = IsReadOnly;

            this.IsTemplate = false;
        }

        public SerialParameter(RevitParam parameter,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document, [DefaultArgument("true")] bool IsTemplate)
        {
            this.IsTemplate = IsTemplate;

            this.Name = parameter.Definition.Name;
            this.StorageType = parameter.StorageType.ToString();
            switch (this.StorageType)
            {
                case "Double":
                    this.Value = parameter.AsDouble().ToString();
                    break;
                case "ElementId":
                    this.ValueElemId = new SerialElementId(parameter.AsElementId(), Document, IsTemplate);
                    //this.Value = param.AsElementId().ToString();
                    break;
                case "Integer":
                    this.Value = parameter.AsInteger().ToString();
                    break;
                case "String":
                default:
                    this.Value = parameter.AsString();
                    break;
            }

            this.Id = parameter.Id.IntegerValue;

            if (IsShared)
            {
                this.GUID = parameter.GUID.ToString();
            }

            this.IsShared = parameter.IsShared;
            this.IsReadOnly = parameter.IsReadOnly;

        }

        #endregion
        #region Public Methods

        public static string ToJSON(SerialParameter parameter)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(parameter, Formatting.Indented);
        }

        public static RevitElem ModifyParameter(SerialParameter serialParameter, RevitElem Elem)
        {
            RevitParam param = null;
            RevitDoc doc = Elem.Document;

            //Get the parameter object
            if (serialParameter.IsShared)
            {
                param = Elem.get_Parameter(new Guid(serialParameter.GUID));
            }
            else if (serialParameter.Id < 0)
            {
                param = Elem.get_Parameter((BuiltInParameter)serialParameter.Id);
            }
            else if (serialParameter.Id > 0)
            {
                ParameterElement paramElem = (ParameterElement)doc.GetElement(new ElementId(serialParameter.Id));
                if (paramElem != null)
                {
                    Definition def = paramElem.GetDefinition();
                    param = Elem.get_Parameter(def);
                }
            }

            //Depending on the parameter type, change the parameter
            if (param != null && !param.IsReadOnly)
            {
                switch (serialParameter.StorageType)
                {
                    case "Double":
                        param.Set(Convert.ToDouble(serialParameter.Value));
                        break;
                    case "ElementId":
                        SerialParameter._ModifyElementIdParameter(param, serialParameter.ValueElemId, doc);
                        break;
                    case "Integer":
                        param.Set(Convert.ToInt32(serialParameter.Value));
                        break;
                    case "String":
                    default:
                        param.Set(serialParameter.Value);
                        break;
                }
            }

            return Elem;
        }

        #endregion
        #region Helper Functions

        /// <summary>
        /// Depending on SerialElementId properties, method attempts to find the Element referenced by the ElementId by first trying UniqueId, then Id, then Name, then Aliases of the name.  If an element is found, then set the parameter to the Element's Id.
        /// </summary>
        /// <param name="param">Parameter to modify</param>
        /// <param name="serialElementId">A SerialElementId that references an element in the Document.</param>
        /// <param name="document">A revit document</param>
        /// <returns name="status">Returns true if parameter was successfully set and false if not.  Parameter retains it previous value if Set was unsucessful.</returns>
        private static bool _ModifyElementIdParameter (RevitParam param, SerialElementId serialElementId, RevitDoc document)
        {
            //  Initialize variables.
            // Status of parameter change.
            bool status = false;

            //  Element to set the ElementId parameter.
            //  Will return null if element is not found.
            RevitElem elem = serialElementId.GetElem(document);

            //  If elem was selected, then set the parameter to that element.
            if (elem != null)
            {
                status = param.Set(elem.Id);
            }

            // Return if parameter change was successful.
            return status;
        }

        #endregion
    }
}
