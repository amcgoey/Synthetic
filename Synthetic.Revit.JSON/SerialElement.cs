using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitElem = Autodesk.Revit.DB.Element;
using RevitElemId = Autodesk.Revit.DB.ElementId;

using Revit.Elements;
using DynElem = Revit.Elements.Element;

using Select = Synthetic.Revit.Select;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialElement
    {
        #region Public Properties
       
        public string Class
        {
            get => this.ElementId.Class;
            set => this.ElementId.Class = value;
        }

        public string Category
        {
            get => this.ElementId.Category;
            set => this.ElementId.Category = value;
        }

        public string Name
        {
            get => this.ElementId.Name;
            set => this.ElementId.Name = value;
        }

        public List<string> Aliases
        {
            get => this.ElementId.Aliases;
            set => this.ElementId.Aliases = value;
        }

        public int Id
        {
            get => this.ElementId.Id;
            set => this.ElementId.Id = value;
        }

        public string UniqueId
        {
            get => this.ElementId.UniqueId;
            set => this.ElementId.UniqueId = value;
        }

        public List<SerialParameter> Parameters { get; set; }

        [JsonIgnoreAttribute]
        public SerialElementId ElementId { get; set; }

        [JsonIgnoreAttribute]
        virtual public RevitElem Element { get; set; }

        [JsonIgnoreAttribute]
        public RevitDoc Document { get; set; }

        #endregion
        #region Public Constructors

        public SerialElement()
        {
            this.ElementId = new SerialElementId();
        }

        public SerialElement(DynElem dynamoElement)
        {
            RevitElem elem = dynamoElement.InternalElement;
            _ByElement(elem);
        }

        public SerialElement(RevitElem revitElement)
        {
            _ByElement(revitElement);
        }

        #endregion
        #region Constructor Helper Functions

        private void _ByElement (RevitElem elem)
        {
            this.Element = elem;
            this.Document = elem.Document;

            this.ElementId = new SerialElementId();

            this.Class = elem.GetType().FullName;
            this.Name = elem.Name;
            this.Id = elem.Id.IntegerValue;
            this.UniqueId = elem.UniqueId.ToString();

            RevitDB.Category cat = elem.Category;

            if (cat != null)
            {
                this.Category = elem.Category.Name;
            }

            this.Parameters = new List<SerialParameter>();

            //Iterate through parameters
            foreach (RevitDB.Parameter param in elem.Parameters)
            {
                //If the parameter has a value, the add it to the parameter list for export
                if (!param.IsReadOnly)
                {
                    this.Parameters.Add(new SerialParameter(param, this.Document));
                }
            }
        }

        #endregion
        #region Public Methods

        public RevitElem GetRevitElem ([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            return this.ElementId.GetElem(document);
        }

        public DynElem GetDynamoElem([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            return this.ElementId.GetElem(document).ToDSType(true);
        }

        public List<RevitElem> GetAliasRevitElements ([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            List<RevitElem> elements = new List<RevitElem>();

            foreach(string aliasName in this.Aliases)
            {
                // Assembly and Class that the Element should be
                Assembly assembly = typeof(RevitElem).Assembly;
                Type elemClass = assembly.GetType(this.Class);
                RevitElem elem = Select.RevitElementByNameClass(aliasName, elemClass, document);
                elements.Add(elem);
            }
            return elements;
        }

        public List<DynElem> GetAliasDynamoElements([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            List<DynElem> elements = new List<DynElem>();

            foreach (string aliasName in this.Aliases)
            {
                // Assembly and Class that the Element should be
                Assembly assembly = typeof(RevitElem).Assembly;
                Type elemClass = assembly.GetType(this.Class);
                RevitElem elem = Select.RevitElementByNameClass(aliasName, elemClass, document);
                elements.Add(elem.ToDSType(true));
            }
            return elements;
        }

        public static DynElem ModifyElement(SerialElement serialElement, [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            if (serialElement.Element == null)
            {
                serialElement.Element = serialElement.GetRevitElem(document);
            }

            if(serialElement.Element != null)
            {
                serialElement._ModifyProperties(serialElement.Element);
            }

            return serialElement.Element.ToDSType(true);
        }

        public override string ToString()
        {
            return string.Format("{0}(Name=\"{1}\", ID={2})", this.GetType().Name, this.Name, this.Id);
        }

        public static SerialElement ByJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<SerialElement>(JSON);
        }

        public static string ToJSON(SerialElement serialElement)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(serialElement, Formatting.Indented);
        }

        #endregion
        #region Helper Functions

        private void _ModifyProperties (RevitElem elem)
        {
            elem.Name = this.Name;

            foreach (SerialParameter paramJson in this.Parameters)
            {
                SerialParameter.ModifyParameter(paramJson, elem);
            }
        }

        #endregion
    }
}
