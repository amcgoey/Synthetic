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

        public bool IsSurfaceBackgroundPatternVisible { get; set; }
        public SerialColor SurfaceBackgroundPatternColor { get; set; }
        public SerialElementId SurfaceBackgroundPatternId { get; set; }
        
        public bool IsSurfaceForegroundPatternVisible { get; set; }
        public SerialColor SurfaceForegroundPatternColor { get; set; }
        public SerialElementId SurfaceForegroundPatternId { get; set; }

        public SerialColor ProjectionLineColor { get; set; }
        public SerialElementId ProjectionLinePatternId { get; set; }
        public int ProjectionLineWeight { get; set; }
        

        public bool IsCutBackgroundPatternVisible { get; set; }
        public SerialColor CutBackgroundPatternColor { get; set; }
        public SerialElementId CutBackgroundPatternId { get; set; }

        public bool IsCutForegroundPatternVisible { get; set; }
        public SerialColor CutForegroundPatternColor { get; set; }
        public SerialElementId CutForegroundPatternId { get; set; }

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
                this.IsSurfaceBackgroundPatternVisible = ogs.IsSurfaceBackgroundPatternVisible;
                this.SurfaceBackgroundPatternColor = new SerialColor(ogs.SurfaceBackgroundPatternColor);
                this.SurfaceBackgroundPatternId = new SerialElementId(ogs.SurfaceBackgroundPatternId, Document);

                this.IsSurfaceForegroundPatternVisible = ogs.IsSurfaceForegroundPatternVisible;
                this.SurfaceForegroundPatternColor = new SerialColor(ogs.SurfaceForegroundPatternColor);
                this.SurfaceForegroundPatternId = new SerialElementId(ogs.SurfaceForegroundPatternId, Document);
                
                this.ProjectionLineColor = new SerialColor(ogs.ProjectionLineColor);
                this.ProjectionLinePatternId = new SerialElementId(ogs.ProjectionLinePatternId, Document);
                this.ProjectionLineWeight = ogs.ProjectionLineWeight;

                this.IsCutBackgroundPatternVisible = ogs.IsCutBackgroundPatternVisible;
                this.CutBackgroundPatternColor = new SerialColor(ogs.CutBackgroundPatternColor);
                this.CutBackgroundPatternId = new SerialElementId(ogs.CutBackgroundPatternId, Document);

                this.IsCutForegroundPatternVisible = ogs.IsCutForegroundPatternVisible;
                this.CutForegroundPatternColor = new SerialColor(ogs.CutForegroundPatternColor);
                this.CutForegroundPatternId = new SerialElementId(ogs.CutForegroundPatternId, Document);

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


            if (!ogs.IsSurfaceBackgroundPatternVisible)
            {
                IsModified = true;
            }
            if (ogs.SurfaceBackgroundPatternColor.IsValid)
            {
                IsModified = true;
            }
            if (ogs.SurfaceBackgroundPatternId.IntegerValue != -1)
            {
                IsModified = true;
            }

            if (!ogs.IsSurfaceForegroundPatternVisible)
            {
                IsModified = true;
            }
            if (ogs.SurfaceForegroundPatternColor.IsValid)
            {
                IsModified = true;
            }
            if (ogs.SurfaceForegroundPatternId.IntegerValue != -1)
            {
                IsModified = true;
            }

            if (ogs.ProjectionLineColor.IsValid)
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

            if (!ogs.IsCutBackgroundPatternVisible)
            {
                IsModified = true;
            }
            if (ogs.CutBackgroundPatternColor.IsValid)
            {
                IsModified = true;
            }
            if (ogs.CutBackgroundPatternId.IntegerValue != -1)
            {
                IsModified = true;
            }

            if (!ogs.IsCutForegroundPatternVisible)
            {
                IsModified = true;
            }
            if (ogs.CutForegroundPatternColor.IsValid)
            {
                IsModified = true;
            }
            if (ogs.CutForegroundPatternId.IntegerValue != -1)
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

            ogs.SetSurfaceBackgroundPatternVisible(this.IsSurfaceBackgroundPatternVisible);
            ogs.SetSurfaceBackgroundPatternColor(this.SurfaceBackgroundPatternColor.ToColor());
            ogs.SetSurfaceBackgroundPatternId(this.SurfaceBackgroundPatternId.ToElementId());

            ogs.SetSurfaceForegroundPatternVisible(this.IsSurfaceForegroundPatternVisible);
            ogs.SetSurfaceForegroundPatternColor(this.SurfaceForegroundPatternColor.ToColor());
            ogs.SetSurfaceForegroundPatternId(this.SurfaceForegroundPatternId.ToElementId());

            ogs.SetProjectionLineColor(this.ProjectionLineColor.ToColor());
            ogs.SetProjectionLinePatternId(this.ProjectionLinePatternId.ToElementId());
            ogs.SetProjectionLineWeight(this.ProjectionLineWeight);

            ogs.SetCutBackgroundPatternVisible(this.IsCutBackgroundPatternVisible);
            ogs.SetCutBackgroundPatternColor(this.CutBackgroundPatternColor.ToColor());
            ogs.SetCutBackgroundPatternId(this.CutBackgroundPatternId.ToElementId());

            ogs.SetCutForegroundPatternVisible(this.IsCutForegroundPatternVisible);
            ogs.SetCutForegroundPatternColor(this.CutForegroundPatternColor.ToColor());
            ogs.SetCutForegroundPatternId(this.CutForegroundPatternId.ToElementId());

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
