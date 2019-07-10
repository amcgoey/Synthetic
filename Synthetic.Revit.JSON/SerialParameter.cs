﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Autodesk.DesignScript.Runtime;

using Autodesk.Revit.DB;
using revitParam = Autodesk.Revit.DB.Parameter;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitDoc = Autodesk.Revit.DB.Document;

using Select = Synthetic.Revit.Select;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public SerialElementId ValueElemId { get; set; }
        public string StorageType { get; set; }
        public int Id { get; set; }
        public string GUID { get; set; }
        public bool IsShared { get; set; }
        public bool IsReadOnly { get; set; }


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
        }

        public SerialParameter(revitParam param,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc Document)
        {
            this.Name = param.Definition.Name;
            this.StorageType = param.StorageType.ToString();
            switch (this.StorageType)
            {
                case "Double":
                    this.Value = param.AsDouble().ToString();
                    break;
                case "ElementId":
                    this.ValueElemId = new SerialElementId(param.AsElementId(), Document);
                    //this.Value = param.AsElementId().ToString();
                    break;
                case "Integer":
                    this.Value = param.AsInteger().ToString();
                    break;
                case "String":
                default:
                    this.Value = param.AsString();
                    break;
            }

            this.Id = param.Id.IntegerValue;

            if (IsShared)
            {
                this.GUID = param.GUID.ToString();
            }

            this.IsShared = param.IsShared;
            this.IsReadOnly = param.IsReadOnly;

        }

        public static string ToJSON(SerialParameter parameter)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(parameter, Formatting.Indented);
        }

        public static revitElem ModifyParameter(SerialParameter paramJSON, revitElem Elem)
        {
            revitParam param = null;
            revitDoc doc = Elem.Document;

            if (paramJSON.IsShared)
            {
                param = Elem.get_Parameter(new Guid(paramJSON.GUID));
            }
            else if (paramJSON.Id < 0)
            {
                param = Elem.get_Parameter((BuiltInParameter)paramJSON.Id);
            }
            else if (paramJSON.Id > 0)
            {
                ParameterElement paramElem = (ParameterElement)doc.GetElement(new ElementId(paramJSON.Id));
                if (paramElem != null)
                {
                    Definition def = paramElem.GetDefinition();
                    param = Elem.get_Parameter(def);
                }
            }

            if (param != null && !param.IsReadOnly)
            {
                switch (paramJSON.StorageType)
                {
                    case "Double":
                        param.Set(Convert.ToDouble(paramJSON.Value));
                        break;
                    case "ElementId":
                        //param.Set(new ElementId(Convert.ToInt32(paramJSON.Value)));                        
                        //param.Set(paramJSON.ValueElemId.ToElementId());
                        SerialParameter._ModifyElementIdParameter(param, paramJSON.ValueElemId, doc);
                        break;
                    case "Integer":
                        param.Set(Convert.ToInt32(paramJSON.Value));
                        break;
                    case "String":
                    default:
                        param.Set(paramJSON.Value);
                        break;
                }
            }

            return Elem;
        }

        private static bool _ModifyElementIdParameter (revitParam param, SerialElementId serialElementId, revitDoc document)
        {
            bool status = false;
            revitElem elem = null;

            if(serialElementId.UniqueId != null)
            {
                elem = document.GetElement(serialElementId.UniqueId);
            }
            else if (serialElementId.Id != 0)
            {
                elem = document.GetElement(serialElementId.ToElementId());
            }
            else if (serialElementId.Name != null)
            {
                Assembly assembly = typeof(revitElem).Assembly;
                Type elemClass = assembly.GetType(serialElementId.Class);

                elem = Select.ByNameClass(elemClass, serialElementId.Name, document);
            }

            if (elem == null)
            {
                status = param.Set(elem.Id);
            }

            return status;
        }
    }
}
