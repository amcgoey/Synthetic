using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitCS = Autodesk.Revit.DB.CompoundStructure;
using RevitCSLayer = Autodesk.Revit.DB.CompoundStructureLayer;

namespace Synthetic.Serialize.Revit
{
    public class SerialCompoundStructure
    {
        public List<SerialCompoundStructureLayer> Layers { get; set; }
        public List<string> WallSweeps { get; set; }

        public SerialCompoundStructure () { }

        public SerialCompoundStructure (RevitCS CompoundStructure,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            this.Layers = new List<SerialCompoundStructureLayer>();
            this.WallSweeps = new List<string>();
            IList<RevitCSLayer> csLayers = CompoundStructure.GetLayers();

            foreach(RevitCSLayer csLayer in csLayers)
            {
                this.Layers.Add(new SerialCompoundStructureLayer(csLayer, Document));
            }
        }

        public RevitCS CreateCompoundStructure(
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            IList<RevitCSLayer> csLayers = new List<RevitCSLayer>();

            foreach (SerialCompoundStructureLayer layer in this.Layers)
            {
                csLayers.Add(layer.CreateCompoundStructureLayer(Document));
            }

            RevitCS cs = RevitCS.CreateSimpleCompoundStructure(csLayers);

            return cs;
        }
    }

    public class SerialCompoundStructureLayer
    {
        public SerialElementId MaterialId { get; set; }
        public string Function { get; set; }
        public double Width { get; set; }
        public string DeckEmbeddingType { get; set; }
        public SerialElementId DeckProfileId { get; set; }
        public bool LayerCapFlag { get; set; }

        public SerialCompoundStructureLayer () { }

        public SerialCompoundStructureLayer (RevitCSLayer Layer,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            this.MaterialId = new SerialElementId(Layer.MaterialId, Document, false);
            this.Function = Layer.Function.ToString();
            this.Width = Layer.Width;
            this.DeckEmbeddingType = Layer.DeckEmbeddingType.ToString();
            this.DeckProfileId = new SerialElementId(Layer.DeckProfileId, Document, false);
            this.LayerCapFlag = Layer.LayerCapFlag;
        }

        public RevitCSLayer CreateCompoundStructureLayer (
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            RevitCSLayer layer = new RevitCSLayer();

            layer.DeckEmbeddingType =
                (RevitDB.StructDeckEmbeddingType)
                Enum.Parse(
                    typeof(RevitDB.StructDeckEmbeddingType),
                    this.DeckEmbeddingType);

            RevitDB.Element deck = this.DeckProfileId.GetElem(Document);
            if (deck != null)
            {
                layer.DeckProfileId = deck.Id;
            }

            layer.Function =
                (RevitDB.MaterialFunctionAssignment)
                Enum.Parse(
                    typeof(RevitDB.MaterialFunctionAssignment),
                    this.Function);

            layer.LayerCapFlag = this.LayerCapFlag;

            RevitDB.Material mat = (RevitDB.Material) this.MaterialId.GetElem(Document);
            if (mat != null)
            {
                layer.MaterialId = mat.Id;
            }

            layer.Width = this.Width;

            return layer;
        }
    }
}
