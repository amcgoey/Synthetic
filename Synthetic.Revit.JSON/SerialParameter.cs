using System;
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

        /// <summary>
        /// Depending on SerialElementId properties, method attempts to find the Element referenced by the ElementId by first trying UniqueId, then Id, then Name, then Aliases of the name.  If an element is found, then set the parameter to the Element's Id.
        /// </summary>
        /// <param name="param">Parameter to modify</param>
        /// <param name="serialElementId">A SerialElementId that references an element in the Document.</param>
        /// <param name="document">A revit document</param>
        /// <returns name="status">Returns true if parameter was successfully set and false if not.  Parameter retains it previous value if Set was unsucessful.</returns>
        private static bool _ModifyElementIdParameter (revitParam param, SerialElementId serialElementId, revitDoc document)
        {
            //  Initialize variables.
            // Status of parameter change.
            bool status = false;

            //  Element to set the ElementId parameter to
            revitElem elem = null;

            // Assembly and Class that the Element should be
            Assembly assembly = typeof(revitElem).Assembly;
            Type elemClass = assembly.GetType(serialElementId.Class);

            // Try to collect the element by UniqueId
            if (serialElementId.UniqueId != null && elem == null)
            {
                elem = document.GetElement(serialElementId.UniqueId);
            }
            // Otherwise try to collect the element by Id
            if (serialElementId.Id != 0 && elem == null)
            {
                elem = document.GetElement(serialElementId.ToElementId());
            }
            // Otherwise try to collect the element by name
            if (serialElementId.Name != null && elem == null)
            {
                elem = Select.ByNameClass(elemClass, serialElementId.Name, document);
            }
            // Otherwise try to collect the element by aliases of it's name.
            if (serialElementId.Aliases != null && elem == null)
            {
                // Intialize list for alias ElementTypes
                List<revitElem> aliasElem = new List<revitElem>();

                //  Try to collect elements for each alias
                foreach (string alias in serialElementId.Aliases)
                {
                    aliasElem.Add((revitElem)Select.ByNameClass(elemClass, alias, document));
                }

                //  If an alias was found, then select that alias to use.
                if (aliasElem.FirstOrDefault() != null)
                {
                    elem = aliasElem.FirstOrDefault();
                }
            }

            //  If elem was selected, then set the parameter to that element.
            if (elem != null)
            {
                status = param.Set(elem.Id);
            }

            // Return if parameter change was successful.
            return status;
        }
    }
}
