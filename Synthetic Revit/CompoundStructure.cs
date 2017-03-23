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
        internal revitCS internalCompoundStructure { get; private set; }
        internal cg.IList<revitDB.CompoundStructureLayer> internalLayers { get; private set; }

        internal CompoundStructure (revitCS cs)
        {
            internalCompoundStructure = cs;
            internalLayers = cs.GetLayers();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compound"></param>
        /// <returns></returns>
        public static CompoundStructure Wrap(revitCS compound)
        {
            return new CompoundStructure(compound);
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
        /// <returns></returns>
        public static CompoundStructure FromWall(dynamoElements.WallType wallType)
        {
            revitDB.WallType unwrapped = (revitDB.WallType)wallType.InternalElement;

            return new CompoundStructure(unwrapped.GetCompoundStructure());
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
                trans.Start("Set Number of Exterior Layers");
                revitWallType.SetCompoundStructure(compoundStructure.internalCompoundStructure);
                trans.Commit();
            }
            return wallType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="layerFunction"></param>
        /// <param name="materialId"></param>
        /// <returns></returns>
        public static CompoundStructure ByLayers(cg.List<double> width, cg.List<revitDB.MaterialFunctionAssignment> layerFunction, cg.List<revitDB.ElementId> materialId)
        {
            cg.List<int> lenList = new cg.List<int>();
            lenList.Add(layerFunction.Count);
            lenList.Add(width.Count);
            lenList.Add(materialId.Count);

            int l = lenList.Min();

            cg.List<revitCSLayer> layerList = new cg.List<revitCSLayer>();

            for (int i = 0; i < l; i++)
            {
                revitCSLayer layer = new revitCSLayer(width[i], layerFunction[i], materialId[i]);
                layerList.Add(layer);
            }

            return new CompoundStructure(revitCS.CreateSimpleCompoundStructure(layerList));
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
            return compoundStructure.internalLayers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public static CompoundStructure SetLayers(CompoundStructure compoundStructure, cg.IList<revitCSLayer> layers)
        {
            compoundStructure.internalLayers = layers;
            compoundStructure.internalCompoundStructure.SetLayers(layers);
            return compoundStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static synthDict LayerToDictionary(revitCSLayer layer)
        {
            return synthDict.Wrap((cg.Dictionary<string, object>)LayerToList(layer));
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

            foreach (revitCSLayer layer in this.internalLayers)
            {
                s = string.Concat(s, string.Format("\n  Layer {0}: {1}, Width-> {2}, MaterialId-> {3}", i, layer.Function, layer.Width, layer.MaterialId));
                i++;
            }
            return s;
        }
    }
}
