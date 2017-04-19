using System;
using System.Linq;
using Autodesk.DesignScript.Runtime;

using System.Collections;
using cg = System.Collections.Generic;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitCS = Autodesk.Revit.DB.CompoundStructure;
using revitCSLayer = Autodesk.Revit.DB.CompoundStructureLayer;

using dynamoElements = Revit.Elements;

using synthDict = Synthetic.Core.Dictionary;

namespace Synthetic.Revit
{
    /// <summary>
    /// 
    /// </summary>
    public class CompoundStructure
    {
        #region Internal Properties

        internal revitCS internalCompoundStructure { get; private set; }
        //internal cg.IList<revitDB.CompoundStructureLayer> internalLayers { get; private set; }

        internal cg.IList<cg.Dictionary<string, object>> internalLayers
        {
            get { return CompoundStructure._GetLayers(this.internalCompoundStructure, this.internalDocument); }
        }

        internal int internalFirstCoreLayerIndex
        {
            get { return this.internalCompoundStructure.GetFirstCoreLayerIndex(); }
            private set { this.internalCompoundStructure.SetNumberOfShellLayers(Autodesk.Revit.DB.ShellLayerType.Exterior, value); }
        }

        internal int internalLastCoreLayerIndex
        {
            get { return this.internalCompoundStructure.GetLastCoreLayerIndex(); }
            private set { this.internalCompoundStructure.SetNumberOfShellLayers(Autodesk.Revit.DB.ShellLayerType.Exterior, value); }
        }

        internal revitDoc internalDocument { get; private set; }

        #endregion
        #region Internal Constructors

        internal CompoundStructure (revitCS cs, revitDoc doc)
        {
            internalCompoundStructure = cs;

            internalDocument = doc;
            //internalLayers = _GetLayers(cs, doc);
            internalFirstCoreLayerIndex = cs.GetFirstCoreLayerIndex();
            internalLastCoreLayerIndex = cs.GetLastCoreLayerIndex();
        }

        #endregion
        #region Internal Methods

        internal static cg.Dictionary<string, object> _RevitLayerToDictionary (revitCSLayer layer, revitDoc doc)
        {
            double width = layer.Width;
            revitDB.MaterialFunctionAssignment layerFunction = layer.Function;
            revitDB.Material material = (revitDB.Material)doc.GetElement(layer.MaterialId);
            return new cg.Dictionary<string, object>
            {
                {"Layer Function", layerFunction},
                {"Width", width},
                {"Material", material }
            };
        }

        internal static cg.IList<cg.Dictionary<string, object>> _GetLayers (revitCS cs, revitDoc doc)
        {
            cg.IList<cg.Dictionary<string, object>> layers = new cg.List<cg.Dictionary<string, object>>();

            foreach (revitCSLayer layer in cs.GetLayers())
            {
                layers.Add(_RevitLayerToDictionary(layer, doc));
            }

            return layers;
        }

        

        internal static CompoundStructure _CopyToDocument (CompoundStructure compoundStructure, revitDoc destinationDoc)
        {
            CompoundStructure destinationCS;

            cg.IList<cg.Dictionary<string, object>> destinationLayers = new cg.List<cg.Dictionary<string, object>>();

            foreach (cg.Dictionary<string, object> sourceLayer in compoundStructure.internalLayers)
            {
                cg.Dictionary<string, object> destintationLayer = sourceLayer;

                revitDB.Material sourceMaterial = (revitDB.Material)sourceLayer["Material"];
                revitDB.Material destinationMaterial = Select.GetMaterialByName(Select.AllMaterials(destinationDoc), sourceMaterial.Name);

                if (destinationMaterial == null)
                {
                    Elements.CopyElementsBetweenDocs(compoundStructure.internalDocument, new cg.List<int>(sourceMaterial.Id.IntegerValue), destinationDoc);
                    destinationMaterial = Select.GetMaterialByName(Select.AllMaterials(destinationDoc), sourceMaterial.Name);
                }
                destintationLayer["Material"] = destinationMaterial;

                destinationLayers.Add(destintationLayer);
            }

            destinationCS = CompoundStructure.ByLayerDictionary(destinationLayers, destinationDoc);

            return destinationCS;
        }

        #endregion
        #region Public Static Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="layerFunction"></param>
        /// <param name="material"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static CompoundStructure ByLayerProperties(cg.List<double> width,
            cg.List<revitDB.MaterialFunctionAssignment> layerFunction,
            cg.List<revitDB.Material> material,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            cg.List<int> lenList = new cg.List<int>();
            lenList.Add(layerFunction.Count);
            lenList.Add(width.Count);
            lenList.Add(material.Count);

            int l = lenList.Min();

            cg.List<revitCSLayer> layerList = new cg.List<revitCSLayer>();

            for (int i = 0; i < l; i++)
            {
                revitCSLayer layer = new revitCSLayer(width[i], layerFunction[i], material[i].Id);
                layerList.Add(layer);
            }

            return new CompoundStructure(revitCS.CreateSimpleCompoundStructure(layerList), document);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layers"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static CompoundStructure ByLayerDictionary (cg.IList<cg.Dictionary<string, object>> layers,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            cg.List<revitCSLayer> layerList = new cg.List<revitCSLayer>();

            foreach (cg.Dictionary<string, object> layerDict in layers)
            {
                revitDB.Material material = (revitDB.Material)layerDict["Material"];
                revitCSLayer layer = new revitCSLayer((double)layerDict["Layer Function"], (revitDB.MaterialFunctionAssignment)layerDict["Width"], material.Id);
            }

            return new CompoundStructure(revitCS.CreateSimpleCompoundStructure(layerList), document);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compound"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static CompoundStructure Wrap(revitCS compound,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            return new CompoundStructure(compound, document);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static revitCS Unwrap (CompoundStructure compoundStructure)
        {
            return compoundStructure.internalCompoundStructure;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallType"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static CompoundStructure FromWall(dynamoElements.WallType wallType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            revitDB.WallType unwrappedWall = (revitDB.WallType)wallType.InternalElement;

            return new CompoundStructure(unwrappedWall.GetCompoundStructure(), document);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallType"></param>
        /// <param name="compoundStructure"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static dynamoElements.WallType ToWall(dynamoElements.WallType wallType,
            CompoundStructure compoundStructure,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            revitDB.WallType revitWallType = (revitDB.WallType)wallType.InternalElement;

            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
            {
                trans.Start("Apply Structure to Wall Type");
                revitWallType.SetCompoundStructure(compoundStructure.internalCompoundStructure);
                trans.Commit();
            }
            return wallType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoudStructure"></param>
        /// <param name="numLayers"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static CompoundStructure SetNumberOfExteriorLayers (CompoundStructure compoudStructure, int numLayers, revitDoc doc)
        {
            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(doc))
            {
                trans.Start("Set Number of Exterior Layers");
                compoudStructure.internalCompoundStructure.SetNumberOfShellLayers(Autodesk.Revit.DB.ShellLayerType.Exterior, numLayers);
                compoudStructure.internalFirstCoreLayerIndex = compoudStructure.internalCompoundStructure.GetFirstCoreLayerIndex();
                trans.Commit();
            }
            return compoudStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoudStructure"></param>
        /// <param name="numLayers"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static CompoundStructure SetNumberOfInteriorLayers(CompoundStructure compoudStructure, int numLayers, revitDoc doc)
        {
            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(doc))
            {
                trans.Start("Set Number of Interior Layers");
                compoudStructure.internalCompoundStructure.SetNumberOfShellLayers(Autodesk.Revit.DB.ShellLayerType.Interior, numLayers);
                compoudStructure.internalLastCoreLayerIndex = compoudStructure.internalCompoundStructure.GetLastCoreLayerIndex();
                trans.Commit();
            }
            return compoudStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static int GetFirstCoreLayerIndex(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalCompoundStructure.GetFirstCoreLayerIndex();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static int GetLastCoreLayerIndex(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalCompoundStructure.GetLastCoreLayerIndex();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static cg.IList<revitCSLayer> GetLayers (CompoundStructure compoundStructure)
        {
            return compoundStructure.internalCompoundStructure.GetLayers();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public static CompoundStructure SetLayers(CompoundStructure compoundStructure, cg.IList<revitCSLayer> layers)
        {
            compoundStructure.internalCompoundStructure.SetLayers(layers);
            return compoundStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static synthDict LayerToDictionary(revitCSLayer layer,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            return synthDict.Wrap((cg.Dictionary<string, object>)_RevitLayerToDictionary(layer, document));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        [MultiReturn(new[] { "Width", "Layer Functions", "Material IDs" })]
        public static IDictionary LayerToList (revitCSLayer layer)
        {
            double width = layer.Width;
            revitDB.MaterialFunctionAssignment layerFunction = layer.Function;
            revitDB.ElementId materialId = layer.MaterialId;
            return new cg.Dictionary<string, object>
            {
                {"Width", width},
                {"Layer Functions", layerFunction},
                {"Material IDs", materialId }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="layerFunction"></param>
        /// <param name="materialId"></param>
        /// <returns></returns>
        public static revitCSLayer LayerByList (double width, revitDB.MaterialFunctionAssignment layerFunction, revitDB.ElementId materialId)
        {
            return new revitCSLayer(width, layerFunction, materialId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            revitCS cs = this.internalCompoundStructure;
            Type t = typeof(CompoundStructure);

            string s = "";
            s = string.Concat(s, string.Format("{1}.{2}: Core Layers {3} to {4}", t.Namespace, GetType().Name, cs.GetFirstCoreLayerIndex(), cs.GetLastCoreLayerIndex()));
            int i = 0;

            foreach (cg.Dictionary<string, object> layer in this.internalLayers)
            {
                s = string.Concat(s, string.Format("\n  Layer {0}: {1}, Width-> {2}, MaterialId-> {3}", i, layer["Layer Function"], layer["Width"], layer["Material"]));
                i++;
            }
            return s;
        }
        #endregion
    }
}