using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitElem = Autodesk.Revit.DB.Element;
using RevitElemId = Autodesk.Revit.DB.ElementId;
using RevitDoc = Autodesk.Revit.DB.Document;

using Select = Synthetic.Revit.Select;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialElementId : SerialObject
    {
        #region Public Properties

        public string Class { get; set; }
        public string Category { get; set; }

        /// <summary>
        /// Name of the element the Id belongs too.
        /// </summary>
        public string Name { get; set; }

        public List<string> Aliases { get; set; }

        /// <summary>
        /// Value of the Element Id as an int
        /// </summary>
        public int Id { get; set; }
        public string UniqueId { get; set; }

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

        public SerialElementId ()
        {
            this.IsTemplate = true;
        }

        public SerialElementId([DefaultArgument("true")] bool IsTemplate)
        {
            this.IsTemplate = IsTemplate;
        }

        public SerialElementId (string Name, int ElementId, string Class, string Category)
        {
            this.Id = ElementId;
            this.Name = Name;
            this.Class = Class;
            this.Category = Category;
            this.IsTemplate = false;
        }

        public SerialElementId (RevitElemId Id,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document, [DefaultArgument("true")] bool IsTemplate)
        {
            this.Id = Id.IntegerValue;
            this.IsTemplate = IsTemplate;

            RevitElem elem = Document.GetElement(Id);

            if (elem != null)
            {
                this.Name = elem.Name;
                this.Class = elem.GetType().FullName;
                this.UniqueId = elem.UniqueId;

                RevitDB.Category cat = elem.Category;

                if(cat != null)
                {
                    this.Category = elem.Category.Name;
                }
                
            }
        }

        #endregion
        #region Public Methods

        public static SerialElementId ByJSON (string JSON)
        {
            return JsonConvert.DeserializeObject<SerialElementId>(JSON);
        }

        public static string ToJSON (SerialElementId IdJSON)
        {
            return JsonConvert.SerializeObject(IdJSON, Formatting.Indented);
        }

        /// <summary>
        /// Try to select the element referenced in the SerialElementId by first trying UniqueId, then Id, then Name, then Aliases of the name.
        /// </summary>
        /// <param name="document">A revit document</param>
        /// <returns name="Element">Returns a Revit Element</returns>
        public RevitElem GetElem (RevitDoc document)
        {
            //  Element to set the ElementId parameter to
            RevitElem elem = null;
            
            // Try to collect the element by UniqueId
            if (this.UniqueId != null && elem == null)
            {
                elem = document.GetElement(this.UniqueId);
            }
            // Otherwise try to collect the element by Id
            if (this.Id != 0 && elem == null)
            {
                elem = document.GetElement(this.ToElementId());
            }
            // Otherwise try to collect the element by name
            if (this.Name != null && elem == null && this.Class != null)
            {
                // Assembly and Class that the Element should be
                Assembly assembly = typeof(RevitElem).Assembly;
                Type elemClass = assembly.GetType(this.Class);
                elem = Select.RevitElementByNameClass(this.Name, elemClass, document);
            }
            // Otherwise try to collect the element by aliases of it's name.
            if (this.Aliases != null && elem == null && this.Class != null)
            {
                // Intialize list for alias ElementTypes
                List<RevitElem> aliasElem = new List<RevitElem>();

                //  Try to collect elements for each alias
                foreach (string alias in this.Aliases)
                {
                    // Assembly and Class that the Element should be
                    Assembly assembly = typeof(RevitElem).Assembly;
                    Type elemClass = assembly.GetType(this.Class);
                    aliasElem.Add((RevitElem)Select.RevitElementByNameClass(alias, elemClass, document));
                }

                //  If an alias was found, then select that alias to use.
                if (aliasElem.FirstOrDefault() != null)
                {
                    elem = aliasElem.FirstOrDefault();
                }
            }

            return elem;
        }

        public RevitElemId ToElementId ()
        {
            return new RevitElemId(this.Id);
        }

        #endregion
    }
}
