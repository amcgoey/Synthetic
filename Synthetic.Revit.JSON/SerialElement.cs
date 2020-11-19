using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;
using RevitServices.Transactions;

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
    public class SerialElement : SerialObject
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

        /// <summary>
        /// List of the Parameters that belong to the Element.
        /// </summary>
        public List<SerialParameter> Parameters { get; set; }

        /// <summary>
        /// The Revit ElementId of the element linked to the SerialElement.
        /// </summary>
        [JsonIgnoreAttribute]
        public SerialElementId ElementId { get; set; }

        /// <summary>
        /// The Revit Element that is linked to the SerialElement.
        /// </summary>
        [JsonIgnoreAttribute]
        virtual public RevitElem Element { get; set; }

        /// <summary>
        /// The Revit Document the element belongs too.
        /// </summary>
        [JsonIgnoreAttribute]
        public RevitDoc Document { get; set; }

        /// <summary>
        /// If true, SerialElement is intended to be deserialized as a template for use as standards or transfer to another project.
        /// If false, SerialElement is intended to modify an element inside the project and will include ElementIds and UniqueIds.
        /// </summary>
        [JsonIgnoreAttribute]
        public bool IsTemplate { get; set; }

        #endregion
        #region Conditional Serialization Methods for Properties

        /// <summary>
        /// If IsTemplate, don't serialize the Element Id
        /// </summary>
        /// <returns>True if not a template</returns>
        public bool ShouldSerializeId()
        {
            return !IsTemplate;
        }

        /// <summary>
        /// If IsTemplate, don't serialize the Unqiue Id
        /// </summary>
        /// <returns>True if not a template</returns>
        public bool ShouldSerializeUniqueId()
        {
            return !IsTemplate;
        }

        #endregion
        #region Public Constructors

        public SerialElement()
        {
            this.ElementId = new SerialElementId();
            this.IsTemplate = true;
        }

        public SerialElement(DynElem dynamoElement, [DefaultArgument("true")] bool IsTemplate)
        {
            RevitElem elem = dynamoElement.InternalElement;
            _ByElement(elem, IsTemplate);
        }

        public SerialElement(RevitElem revitElement, [DefaultArgument("true")] bool IsTemplate)
        {
            _ByElement(revitElement, IsTemplate);
        }

        #endregion
        #region Constructor Helper Functions

        private void _ByElement (RevitElem elem, bool IsTemplate)
        {
            this.Element = elem;
            this.Document = elem.Document;

            this.ElementId = new SerialElementId(IsTemplate);

            this.Class = elem.GetType().FullName;
            this.Name = elem.Name;
            this.Id = elem.Id.IntegerValue;
            this.UniqueId = elem.UniqueId.ToString();

            this.IsTemplate = IsTemplate;

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
                    this.Parameters.Add(new SerialParameter(param, this.Document, IsTemplate));
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

        public List<RevitElem> GetAliasElements_Revit ([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            List<RevitElem> elements = new List<RevitElem>();

            if (this.Aliases != null)
            {
                foreach (string aliasName in this.Aliases)
                {
                    // Assembly and Class that the Element should be
                    Assembly assembly = typeof(RevitElem).Assembly;
                    Type elemClass = assembly.GetType(this.Class);
                    RevitElem elem = Select.RevitElementByNameClass(aliasName, elemClass, document);
                    if (elem != null)
                    {
                        elements.Add(elem);
                    }
                }
            }
            return elements;
        }

        public List<DynElem> GetAliasElements_Dynamo([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            List<DynElem> elements = new List<DynElem>();

            foreach (string aliasName in this.Aliases)
            {
                // Assembly and Class that the Element should be
                Assembly assembly = typeof(RevitElem).Assembly;
                Type elemClass = assembly.GetType(this.Class);
                RevitElem elem = Select.RevitElementByNameClass(aliasName, elemClass, document);
                if (elem != null)
                {
                    elements.Add(elem.ToDSType(true));
                }
            }
            return elements;
        }

        /// <summary>
        /// Sets the SerialElement's aliases given a list of strings.
        /// </summary>
        /// <param name="serialElement">A SerialElement</param>
        /// <param name="aliases">A list of strings representing name aliases.</param>
        /// <returns name="serialElement">Returns the modified SerialElement</returns>
        public static SerialElement SetAliases (SerialElement serialElement, List<string> aliases)
        {
            if (aliases.Count()>0)
            {
                serialElement.Aliases = aliases;
            }
            return serialElement;
        }

        public static DynElem ModifyElement(SerialElement serialElement, [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            string transactionName = "Modify Element from Serialization";

            if (serialElement.Element == null)
            {
                serialElement.Element = serialElement.GetRevitElem(document);
            }

            if(serialElement.Element != null)
            {
                if (document.IsModifiable)
                {
                    TransactionManager.Instance.EnsureInTransaction(document);
                    serialElement._ModifyProperties(serialElement.Element);
                    TransactionManager.Instance.TransactionTaskDone();
                }
                else
                {
                    using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                    {
                        trans.Start(transactionName);
                        serialElement._ModifyProperties(serialElement.Element);
                        trans.Commit();
                    }
                }
                
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
