using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitHostObjType = Autodesk.Revit.DB.HostObjAttributes;

using Revit.Elements;
using DynElem = Revit.Elements.Element;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialHostObjType : SerialElementType
    {
        #region Public Properties

        public SerialEnum Function { get; set; }
        public SerialCompoundStructure Structure { get; set; }

        [JsonIgnoreAttribute]
        public RevitHostObjType WallType { get; set; }

        [JsonIgnoreAttribute]
        public override RevitDB.Element Element
        {
            get => this.WallType;
            set => this.WallType = (RevitHostObjType)value;
        }

        #endregion
        #region Public Constructors

        public SerialHostObjType () : base () { }

        public SerialHostObjType (RevitHostObjType wallType) : base (wallType)
        {
            RevitDoc document = wallType.Document;
            this._ApplyProperties(wallType, document);
        }

        public SerialHostObjType(DynElem dynamoWallType) : base (dynamoWallType)
        {
            RevitHostObjType wallType = (RevitHostObjType) dynamoWallType.InternalElement;
            RevitDoc document = wallType.Document;
            this._ApplyProperties(wallType, document);
        }

        public SerialHostObjType(SerialElementType serialElementType) : base (serialElementType.ElementType)
        {
            if (serialElementType.Element.GetType() == typeof(RevitHostObjType))
            {
                RevitDoc document = serialElementType.Document;
                if (serialElementType.Element == null && document != null)
                {
                    this.WallType = (RevitHostObjType) serialElementType.GetRevitElem(document);
                }

                if(this.WallType != null)
                {
                    this._ApplyProperties(this.WallType, document);
                }
            }
        }

        #endregion
        #region Helper Constructor Functions

        private void _ApplyProperties (RevitHostObjType wallType, RevitDoc document)
        {
            //this.Function = wallType.Function.ToString();

            RevitDB.CompoundStructure cs = wallType.GetCompoundStructure();
            if (cs != null)
            {
                this.Structure = new SerialCompoundStructure(cs, document);
            }
        }

        #endregion
        #region Public Methods

        public static DynElem CreateWallType(SerialHostObjType serialWallType, RevitHostObjType templateWallType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            DynElem dElem = null;

            // Intialize an empty newType
            RevitHostObjType newType = (RevitHostObjType)serialWallType.GetRevitElem(document);

            SerialHostObjType newSerial = (SerialHostObjType)serialWallType.MemberwiseClone();

            // If the ElementType doesn't exist, create a new type based on the template
            if (newType == null)
            {
                newType = (RevitHostObjType)templateWallType.Duplicate(serialWallType.Name);
            }

            if (newType == null)
            {
                newSerial.Element = newType;
                newSerial.UniqueId = newType.UniqueId;
                newSerial.Id = newType.Id.IntegerValue;

                newSerial._ModifyProperties(newType, document);
                dElem = newType.ToDSType(true);
            }

            return dElem; 
        }

        public static DynElem ModifyWallType(SerialHostObjType serialWallType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            DynElem dElem = null;

            if (serialWallType.Element == null)
            {
                serialWallType.Element = (RevitHostObjType)serialWallType.GetRevitElem(document);
            }

            if (serialWallType.Element != null)
            {
                serialWallType._ModifyProperties((RevitHostObjType)serialWallType.Element, document);
                dElem = serialWallType.Element.ToDSType(true);
            }

            return dElem;
        }

        #endregion
        #region Helper Functions
        private void _ModifyProperties(RevitHostObjType wallType, RevitDoc docuement)
        {
            wallType.Name = this.Name;

            //wallType.Function =
            //    (RevitDB.WallFunction)
            //    Enum.Parse(
            //        typeof(RevitDB.WallFunction),
            //        this.Function);

            if (this.Structure != null)
            {
                wallType.SetCompoundStructure(this.Structure.CreateCompoundStructure(docuement));
            }

            foreach (SerialParameter paramJson in this.Parameters)
            {
                SerialParameter.ModifyParameter(paramJson, wallType);
            }
        }
        #endregion
    }
}
