using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    [SupressImportIntoVM]
    public class SerialOverrideGraphicSettings : SerialObject
    {
        #region Public Properties

        [JsonIgnoreAttribute]
        public bool IsModified { get; set; }

        public bool IsProjectionFillPaternVisible { get; set; }
        public SerialColor ProjectionFillColor { get; set; }
        public SerialElementId ProjectionFillPatternId { get; set; }
        public SerialColor ProjectionLineColor { get; set; }
        public SerialElementId ProjectionLinePatternId { get; set; }
        public int ProjectionLineWeight { get; set; }
        

        public bool IsCutFillPaternVisible { get; set; }
        public SerialColor CutFillColor { get; set; }
        public SerialElementId CutFillPatternId { get; set; }
        public SerialColor CutLineColor { get; set; }
        public SerialElementId CutLinePatternId { get; set; }
        public int CutLineWeight { get; set; }
        

        public int Transparency { get; set; }

        public bool Halftone { get; set; }

        public SerialEnum DetailLevel { get; set; }

        #endregion
        #region Public Constructors

        public SerialOverrideGraphicSettings () { }

        public SerialOverrideGraphicSettings (RevitDB.OverrideGraphicSettings overrideGraphicSettings,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            this._setProperties(overrideGraphicSettings, Document);
        }

        public SerialOverrideGraphicSettings(RevitDB.Category category, RevitDB.OverrideGraphicSettings overrideGraphicSettings,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            this.IsModified = _IsModified(overrideGraphicSettings);

            if (this.IsModified)
            {
                this._setProperties(overrideGraphicSettings, Document);
            }
        }

        private void _setProperties(RevitDB.OverrideGraphicSettings ogs,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            if (this._IsModified(ogs))
            {
                this.IsProjectionFillPaternVisible = ogs.IsProjectionFillPatternVisible;
                this.ProjectionFillColor = new SerialColor(ogs.ProjectionFillColor);
                this.ProjectionFillPatternId = new SerialElementId(ogs.ProjectionFillPatternId, Document);
                this.ProjectionLineColor = new SerialColor(ogs.ProjectionLineColor);
                this.ProjectionLinePatternId = new SerialElementId(ogs.ProjectionLinePatternId, Document);
                this.ProjectionLineWeight = ogs.ProjectionLineWeight;

                this.IsCutFillPaternVisible = ogs.IsCutFillPatternVisible;
                this.CutFillColor = new SerialColor(ogs.CutFillColor);
                this.CutFillPatternId = new SerialElementId(ogs.CutFillPatternId, Document);
                this.CutLineColor = new SerialColor(ogs.CutLineColor);
                this.CutLinePatternId = new SerialElementId(ogs.CutLinePatternId, Document);
                this.CutLineWeight = ogs.CutLineWeight;

                this.Transparency = ogs.Transparency;
                this.Halftone = ogs.Halftone;
                this.DetailLevel = new SerialEnum(typeof(RevitDB.ViewDetailLevel), ogs.DetailLevel);
            }
        }

        private bool _IsModified(RevitDB.OverrideGraphicSettings ogs)
        {
            bool isDefault = false;

            if (!ogs.IsProjectionFillPatternVisible)
            {
                IsModified = true;
            }
            if (ogs.ProjectionFillColor.IsValid)
            {
                IsModified = true;
            }
            if (ogs.ProjectionFillPatternId.IntegerValue != -1)
            {
                IsModified = true;
            }
            if (ogs.ProjectionFillColor.IsValid)
            {
                IsModified = true;
            }
            if (ogs.ProjectionLinePatternId.IntegerValue != -1)
            {
                IsModified = true;
            }
            if (ogs.ProjectionLineWeight != -1)
            {
                IsModified = true;
            }

            if (!ogs.IsCutFillPatternVisible)
            {
                IsModified = true;
            }
            if (ogs.CutFillColor.IsValid)
            {
                IsModified = true;
            }
            if (ogs.CutFillPatternId.IntegerValue != -1)
            {
                IsModified = true;
            }
            if (ogs.CutLineColor.IsValid)
            {
                IsModified = true;
            }
            if (ogs.CutLinePatternId.IntegerValue != -1)
            {
                IsModified = true;
            }
            if (ogs.CutLineWeight != -1)
            {
                IsModified = true;
            }

            if (ogs.Transparency != 0)
            {
                IsModified = true;
            }
            if (ogs.Halftone == true)
            {
                IsModified = true;
            }
            if (ogs.DetailLevel != RevitDB.ViewDetailLevel.Undefined)
            {
                IsModified = true;
            }

            return IsModified;
        }

        #endregion
        #region Public Methods

        public RevitDB.OverrideGraphicSettings ToOverrideGraphicSettings ()
        {
            RevitDB.OverrideGraphicSettings ogs = new RevitDB.OverrideGraphicSettings();

            ogs.SetProjectionFillPatternVisible(this.IsProjectionFillPaternVisible);
            ogs.SetProjectionFillColor(this.ProjectionFillColor.ToColor());
            ogs.SetProjectionFillPatternId(this.ProjectionFillPatternId.ToElementId());
            ogs.SetProjectionLineColor(this.ProjectionLineColor.ToColor());
            ogs.SetProjectionLinePatternId(this.ProjectionFillPatternId.ToElementId());
            ogs.SetProjectionLineWeight(this.ProjectionLineWeight);

            ogs.SetCutFillPatternVisible(this.IsCutFillPaternVisible);
            ogs.SetCutFillColor(this.CutFillColor.ToColor());
            ogs.SetCutFillPatternId(this.CutFillPatternId.ToElementId());
            ogs.SetCutLineColor(this.CutLineColor.ToColor());
            ogs.SetCutLinePatternId(this.CutLinePatternId.ToElementId());
            ogs.SetCutLineWeight(this.CutLineWeight);

            ogs.SetSurfaceTransparency(this.Transparency);
            ogs.SetHalftone(this.Halftone);
            ogs.SetDetailLevel((RevitDB.ViewDetailLevel)this.DetailLevel.ToEnum());

            return ogs;
        }

        #endregion
    }
}
