﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitWallType = Autodesk.Revit.DB.WallType;

using Revit.Elements;
using DynElem = Revit.Elements.Element;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialWallType : SerialElementType
    {
        #region Public Properties

        public string Function { get; set; }
        public SerialCompoundStructure Structure { get; set; }

        [JsonIgnoreAttribute]
        public RevitWallType WallType { get; set; }

        [JsonIgnoreAttribute]
        public override RevitDB.Element Element
        {
            get => this.WallType;
            set => this.WallType = (RevitWallType)value;
        }

        #endregion
        #region Public Constructors

        public SerialWallType () : base () { }

        public SerialWallType (RevitWallType wallType) : base (wallType)
        {
            RevitDoc document = wallType.Document;
            this._ApplyProperties(wallType, document);
        }

        public SerialWallType(DynElem dynamoWallType) : base (dynamoWallType)
        {
            RevitWallType wallType = (RevitWallType) dynamoWallType.InternalElement;
            RevitDoc document = wallType.Document;
            this._ApplyProperties(wallType, document);
        }

        public SerialWallType(SerialElementType serialElementType) : base (serialElementType.ElementType)
        {
            if (serialElementType.Element.GetType() == typeof(RevitWallType))
            {
                RevitDoc document = serialElementType.Document;
                if (serialElementType.Element == null && document != null)
                {
                    this.WallType = (RevitWallType) serialElementType.GetElem(document);
                }

                if(this.WallType != null)
                {
                    this._ApplyProperties(this.WallType, document);
                }
            }
        }

        #endregion
        #region Helper Constructor Functions

        private void _ApplyProperties (RevitWallType wallType, RevitDoc document)
        {
            this.Function = wallType.Function.ToString();

            RevitDB.CompoundStructure cs = wallType.GetCompoundStructure();
            if (cs != null)
            {
                this.Structure = new SerialCompoundStructure(cs, document);
            }
        }

        #endregion
        #region Public Methods

        public static DynElem CreateWallType(SerialWallType serialWallType, RevitWallType templateWallType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            DynElem dElem = null;

            // Intialize an empty newType
            RevitWallType newType = (RevitWallType)serialWallType.GetElem(document);

            SerialWallType newSerial = (SerialWallType)serialWallType.MemberwiseClone();

            // If the ElementType doesn't exist, create a new type based on the template
            if (newType == null)
            {
                newType = (RevitWallType)templateWallType.Duplicate(serialWallType.Name);
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

        public static DynElem ModifyWallType(SerialWallType serialWallType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            DynElem dElem = null;

            if (serialWallType.Element == null)
            {
                serialWallType.Element = (RevitWallType)serialWallType.GetElem(document);
            }

            if (serialWallType.Element != null)
            {
                serialWallType._ModifyProperties((RevitWallType)serialWallType.Element, document);
                dElem = serialWallType.Element.ToDSType(true);
            }

            return dElem;
        }

        #endregion
        #region Helper Functions
        private void _ModifyProperties(RevitWallType wallType, RevitDoc docuement)
        {
            wallType.Name = this.Name;

            wallType.Function =
                (RevitDB.WallFunction)
                Enum.Parse(
                    typeof(RevitDB.WallFunction),
                    this.Function);

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
