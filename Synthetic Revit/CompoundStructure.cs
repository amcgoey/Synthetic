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
    /// Several Revit system families can be represnted as an assembly of layers called a CompoundStructure.
    /// </summary>
    public class CompoundStructure
    {
        #region Internal Properties

        internal revitCS internalCompoundStructure { get; private set; }
        internal revitDoc internalDocument { get; private set; }

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
            private set { this.internalCompoundStructure.SetNumberOfShellLayers(Autodesk.Revit.DB.ShellLayerType.Interior, value); }
        }

        #endregion

        #region Internal Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="doc"></param>
        internal CompoundStructure(revitCS cs, revitDoc doc)
        {
            internalCompoundStructure = cs;
            internalDocument = doc;
        }

        #endregion

        #region Internal Methods

        internal static cg.Dictionary<string, object> _RevitLayerToDictionary(revitCSLayer layer, revitDoc doc)
        {
            double width = layer.Width;
            revitDB.MaterialFunctionAssignment layerFunction = layer.Function;
            revitDB.Material material = (revitDB.Material)doc.GetElement(layer.MaterialId);
            return new cg.Dictionary<string, object>
            {
                {"Width", width},
                {"Layer Function", layerFunction},
                {"Material", material }
            };
        }

        internal static cg.IList<cg.Dictionary<string, object>> _GetLayers(revitCS cs, revitDoc doc)
        {
            cg.IList<cg.Dictionary<string, object>> layers = new cg.List<cg.Dictionary<string, object>>();

            foreach (revitCSLayer layer in cs.GetLayers())
            {
                layers.Add(_RevitLayerToDictionary(layer, doc));
            }

            return layers;
        }



        internal static CompoundStructure _CopyToDocument(CompoundStructure compoundStructure, revitDoc destinationDoc)
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

        #region Public Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="layerFunction"></param>
        /// <param name="material"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static CompoundStructure ByLayerProperties(cg.IList<double> width,
            cg.IList<revitDB.MaterialFunctionAssignment> layerFunction,
            cg.IList<revitDB.Material> material,
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
        public static CompoundStructure ByLayerDictionary(cg.IList<cg.Dictionary<string, object>> layers,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            cg.List<revitCSLayer> layerList = new cg.List<revitCSLayer>();

            foreach (cg.Dictionary<string, object> layerDict in layers)
            {
                revitDB.Material material = (revitDB.Material)layerDict["Material"];
                revitCSLayer layer = new revitCSLayer((double)layerDict["Width"], (revitDB.MaterialFunctionAssignment)layerDict["Layer Function"], material.Id);
                layerList.Add(layer);
            }

            return new CompoundStructure(revitCS.CreateSimpleCompoundStructure(layerList), document);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallType"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static CompoundStructure FromWallType(dynamoElements.WallType wallType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            revitDB.WallType unwrappedWall = (revitDB.WallType)wallType.InternalElement;
            revitCS compoundStructure = unwrappedWall.GetCompoundStructure();

            if (compoundStructure != null)
            {
                return new CompoundStructure(compoundStructure, document);
            }
            else { return null; }

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
        public static revitCS Unwrap(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalCompoundStructure;
        }

        #endregion

        #region Element Modification Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallType"></param>
        /// <param name="compoundStructure"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static dynamoElements.WallType ToWallType(dynamoElements.WallType wallType, CompoundStructure compoundStructure)
        {
            revitDB.WallType revitWallType = (revitDB.WallType)wallType.InternalElement;

            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(compoundStructure.internalDocument))
            {
                try
                {
                    trans.Start("Apply Structure to Wall Type");
                    revitWallType.SetCompoundStructure(compoundStructure.internalCompoundStructure);
                    trans.Commit();
                }
                catch
                {
                    revitWallType.SetCompoundStructure(compoundStructure.internalCompoundStructure);
                }
            }
            return wallType;
        }

        #endregion

        #region Compound Structure Modification Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layerIndex"></param>
        /// <param name="width"></param>
        /// <param name="layerFunction"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public static CompoundStructure SetLayer(CompoundStructure compoundStructure,
           int layerIndex,
           double width,
           revitDB.MaterialFunctionAssignment layerFunction,
           revitDB.Material material)
        {
            CompoundStructure.SetLayerWidth(compoundStructure, layerIndex, width);
            CompoundStructure.SetLayerFunction(compoundStructure, layerIndex, layerFunction);
            CompoundStructure.SetLayerMaterial(compoundStructure, layerIndex, material);
            return compoundStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layerIndex"></param>
        /// <param name="width"></param>
        /// <param name="layerFunction"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public static CompoundStructure InsertLayerAtIndex(CompoundStructure compoundStructure,
           int layerIndex,
           double width,
           revitDB.MaterialFunctionAssignment layerFunction,
           revitDB.Material material)
        {
            revitCSLayer layer = new revitCSLayer(width, layerFunction, material.Id);

            cg.IList<revitCSLayer> layers = compoundStructure.internalCompoundStructure.GetLayers();
            layers.Insert(layerIndex, layer);

            compoundStructure.internalCompoundStructure.SetLayers(layers);
            return compoundStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        public static CompoundStructure DeleteLayer(CompoundStructure compoundStructure, int layerIndex)
        {
            bool result = compoundStructure.internalCompoundStructure.DeleteLayer(layerIndex);
            if (result == true) { return compoundStructure; }
            else { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="destinationDocument"></param>
        /// <returns></returns>
        public static CompoundStructure CopyToDocument(CompoundStructure compoundStructure, revitDoc destinationDocument)
        {
            return _CopyToDocument(compoundStructure, destinationDocument);
        }

        #endregion

        #region Get CompoundStructure Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static cg.IList<cg.Dictionary<string, object>> GetLayers(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalLayers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        public static double GetLayerWidth(CompoundStructure compoundStructure, int layerIndex)
        {
            return compoundStructure.internalCompoundStructure.GetLayerWidth(layerIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        public static revitDB.MaterialFunctionAssignment GetLayerFunction(CompoundStructure compoundStructure,
            int layerIndex)
        {
            return compoundStructure.internalCompoundStructure.GetLayerFunction(layerIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        public static revitDB.Material GetLayerMaterial(CompoundStructure compoundStructure, int layerIndex)
        {
            revitDoc doc = compoundStructure.internalDocument;

            revitDB.ElementId materialId = compoundStructure.internalCompoundStructure.GetMaterialId(layerIndex);
            return (revitDB.Material)doc.GetElement(materialId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        public static int GetLayerMaterialId(CompoundStructure compoundStructure, int layerIndex)
        {
            return compoundStructure.internalCompoundStructure.GetMaterialId(layerIndex).IntegerValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static int GetFirstCoreLayerIndex(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalFirstCoreLayerIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static int GetLastCoreLayerIndex(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalLastCoreLayerIndex;
        }



        #endregion

        #region Set Compound Structure Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layerIndex"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static CompoundStructure SetLayerWidth(CompoundStructure compoundStructure, int layerIndex, double width)
        {
            compoundStructure.internalCompoundStructure.SetLayerWidth(layerIndex, width);
            return compoundStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layerIndex"></param>
        /// <param name="layerFunction"></param>
        /// <returns></returns>
        public static CompoundStructure SetLayerFunction(CompoundStructure compoundStructure,
            int layerIndex,
            revitDB.MaterialFunctionAssignment layerFunction)
        {
            compoundStructure.internalCompoundStructure.SetLayerFunction(layerIndex, layerFunction);
            return compoundStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layerIndex"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public static CompoundStructure SetLayerMaterial(CompoundStructure compoundStructure, int layerIndex, revitDB.Material material)
        {
            compoundStructure.internalCompoundStructure.SetMaterialId(layerIndex, material.Id);
            return compoundStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layerIndex"></param>
        /// <param name="materialId"></param>
        /// <returns></returns>
        public static CompoundStructure SetLayerMaterialId(CompoundStructure compoundStructure, int layerIndex, int materialId)
        {
            compoundStructure.internalCompoundStructure.SetMaterialId(layerIndex, new revitDB.ElementId(materialId));
            return compoundStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="numLayers"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static CompoundStructure SetNumberOfExteriorLayers(CompoundStructure compoundStructure, int numLayers)
        {
            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(compoundStructure.internalDocument))
            {
                trans.Start("Set Number of Exterior Layers");
                //compoundStructure.internalCompoundStructure.SetNumberOfShellLayers(Autodesk.Revit.DB.ShellLayerType.Exterior, numLayers);
                compoundStructure.internalFirstCoreLayerIndex = numLayers;
                trans.Commit();
            }
            return compoundStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="numLayers"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static CompoundStructure SetNumberOfInteriorLayers(CompoundStructure compoundStructure, int numLayers)
        {
            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(compoundStructure.internalDocument))
            {
                trans.Start("Set Number of Interior Layers");
                //compoundStructure.internalCompoundStructure.SetNumberOfShellLayers(Autodesk.Revit.DB.ShellLayerType.Interior, numLayers);
                compoundStructure.internalLastCoreLayerIndex = numLayers;
                trans.Commit();
            }
            return compoundStructure;
        }
        #endregion

        #region CompoundStructureLayer Modification Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <returns name="CompoundStructureLayers"></returns>
        public static cg.IList<revitCSLayer> GetCompoundStructureLayers(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalCompoundStructure.GetLayers();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="compoundStructureLayers"></param>
        /// <returns></returns>
        public static CompoundStructure SetCompoundStructureLayers(CompoundStructure compoundStructure, cg.IList<revitCSLayer> compoundStructureLayers)
        {
            compoundStructure.internalCompoundStructure.SetLayers(compoundStructureLayers);
            return compoundStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructureLayer"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static synthDict CompoundStructureLayerToDictionary(revitCSLayer compoundStructureLayer,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            return synthDict.Wrap((cg.Dictionary<string, object>)_RevitLayerToDictionary(compoundStructureLayer, document));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructureLayer"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        [MultiReturn(new[] { "Width", "Layer Functions", "Material IDs" })]
        public static IDictionary CompoundStructureLayerToList(revitCSLayer compoundStructureLayer,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            //double width = compoundStructureLayer.Width;
            //revitDB.MaterialFunctionAssignment layerFunction = compoundStructureLayer.Function;
            //revitDB.ElementId materialId = compoundStructureLayer.MaterialId;
            //return new cg.Dictionary<string, object>
            //{
            //    {"Width", width},
            //    {"Layer Functions", layerFunction},
            //    {"Material IDs", materialId }
            //};
            return _RevitLayerToDictionary(compoundStructureLayer, document);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="layerFunction"></param>
        /// <param name="materialId"></param>
        /// <returns></returns>
        public static revitCSLayer CompoundStructureLayerByList(double width, revitDB.MaterialFunctionAssignment layerFunction, revitDB.ElementId materialId)
        {
            return new revitCSLayer(width, layerFunction, materialId);
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Creates a string representation of the object.
        /// </summary>
        /// <returns nampe="string">A string representation of the object.</returns>
        public override string ToString()
        {
            int i = 0;
            string s = base.ToString() + ": Core Layers " + internalFirstCoreLayerIndex + "to " + internalLastCoreLayerIndex;
            
            foreach (cg.Dictionary<string, object> layer in this.internalLayers)
            {
                revitDB.Material material = (revitDB.Material)layer["Material"];
                s = s + "\n  Layer " + i + ": " + layer["Layer Function"] + ", Width-> " + layer["Width"] + ", MaterialId-> " + material.Id.ToString();
                i++;
            }
            return s;
        }

        #endregion
    }
}