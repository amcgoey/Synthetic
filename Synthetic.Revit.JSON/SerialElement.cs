﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;

using Revit.Elements;
using dynElem = Revit.Elements.Element;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialElement
    {
        public const string ClassName = "Element";

        #region Public Properties
        public string Class { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string Category { get; set; }
        public List<SerialParameter> Parameters { get; set; }

        public List<string> Aliases { get; set; }

        [JsonIgnoreAttribute]
        public revitElem Element { get; set; }

        [JsonIgnoreAttribute]
        public revitDoc Document { get; set; }
        #endregion

        #region Public Constructors

        public SerialElement() { }

        public SerialElement(dynElem dynamoElement)
        {
            revitElem elem = dynamoElement.InternalElement;
            _ByElement(elem);
        }

        public SerialElement(revitElem revitElement)
        {
            _ByElement(revitElement);
        }
        #endregion

        #region Constructor Helper Functions
        private void _ByElement (revitElem elem)
        {
            this.Element = elem;
            this.Document = elem.Document;

            this.Class = elem.GetType().FullName;
            this.Name = elem.Name;
            this.Id = elem.Id.IntegerValue;
            this.UniqueId = elem.UniqueId.ToString();

            revitDB.Category cat = elem.Category;

            if (cat != null)
            {
                this.Category = elem.Category.Name;
            }

            this.Parameters = new List<SerialParameter>();

            //Iterate through parameters
            foreach (revitDB.Parameter param in elem.Parameters)
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
        public static SerialElement ByJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<SerialElement>(JSON);
        }

        public static string ToJSON(SerialElement serialElement)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(serialElement, Formatting.Indented);
        }

        public static dynElem ModifyElement(SerialElement serialElement,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            revitElem elem = null;

            if (serialElement.UniqueId != null)
            {
                elem = (revitElem)document.GetElement(serialElement.UniqueId);
            }
            else if (serialElement.Id != 0)
            {
                elem = (revitElem)document.GetElement(new revitElemId(serialElement.Id));
            }
            else if (serialElement.Name != null) 
            {
                Assembly assembly = typeof(revitElem).Assembly;
                Type elemClass = assembly.GetType(serialElement.Class);

                revitDB.FilteredElementCollector collector = new revitDB.FilteredElementCollector(document);
                elem = collector.OfClass(elemClass)
                    .FirstOrDefault(e => e.Name.Equals(serialElement.Name));
            }

            if(elem != null)
            {
                serialElement._ModifyProperties(elem);
            }

            return elem.ToDSType(true);
        }

        public override string ToString()
        {
            return string.Format("{0}(Name=\"{1}\", ID={2})", this.GetType().Name, this.Name, this.Id);
        }
        #endregion

        #region Helper Functions
        private void _ModifyProperties (revitElem elem)
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