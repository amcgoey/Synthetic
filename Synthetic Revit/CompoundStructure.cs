using System;
using System.Linq;
using Autodesk.DesignScript.Runtime;

using System.Collections;
using c = System.Collections.Generic;

using revitDB = Autodesk.Revit.DB;
using revitCS = Autodesk.Revit.DB.CompoundStructure;
using revitCSLayer = Autodesk.Revit.DB.CompoundStructureLayer;

using dynamoElements = Revit.Elements;

namespace Synthetic.Revit
{
    /// <summary>
    /// 
    /// </summary>
    public class CompoundStructure
    {
        internal revitCS internalCompoundStructure { get; private set; }

        internal CompoundStructure (revitCS cs)
        {
            internalCompoundStructure = cs;
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
        /// <returns></returns>
        public static dynamoElements.WallType ToWall(dynamoElements.WallType wallType, CompoundStructure compoundStructure)
        {
            revitDB.WallType revitWallType = (revitDB.WallType)wallType.InternalElement;
            revitWallType.SetCompoundStructure(compoundStructure.internalCompoundStructure);

            return wallType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="layerFunction"></param>
        /// <param name="materialId"></param>
        /// <returns></returns>
        public static CompoundStructure ByLayers(c.List<double> width, c.List<revitDB.MaterialFunctionAssignment> layerFunction, c.List<revitDB.ElementId> materialId)
        {
            c.List<int> lenList = new c.List<int>();
            lenList.Add(layerFunction.Count);
            lenList.Add(width.Count);
            lenList.Add(materialId.Count);

            int l = lenList.Min();

            c.List<revitCSLayer> layerList = new c.List<revitCSLayer>();

            for (int i = 0; i < l; i++)
            {
                revitCSLayer layer = new revitCSLayer(width[i], layerFunction[i], materialId[i]);
                layerList.Add(layer);
            }

            return new CompoundStructure(revitCS.CreateSimpleCompoundStructure(layerList));
        }

        public static revitCS SetNumberOfExteriorLayers (revitCS compoudStructure, int numLayers)
        {
            compoudStructure.SetNumberOfShellLayers(Autodesk.Revit.DB.ShellLayerType.Exterior, numLayers);

            return compoudStructure;
        }

        public static revitCS SetNumberOfInteriorLayers(revitCS compoudStructure, int numLayers)
        {
            compoudStructure.SetNumberOfShellLayers(Autodesk.Revit.DB.ShellLayerType.Interior, numLayers);

            return compoudStructure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static c.IList<revitCSLayer> GetLayers (CompoundStructure compoundStructure)
        {
            return compoundStructure.internalCompoundStructure.GetLayers();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundStructure"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public static CompoundStructure SetLayers(CompoundStructure compoundStructure, c.IList<revitCSLayer> layers)
        {
            compoundStructure.internalCompoundStructure.SetLayers(layers);
            return compoundStructure;
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
            return new c.Dictionary<string, object>
            {
                {"Width", width},
                {"Layer Functions", layerFunction},
                {"Material IDs", materialId }
            };
        }

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
            c.IList<revitCSLayer> csLayers = cs.GetLayers();
            Type t = typeof(revitCS);

            string s = "";
            s = string.Concat(s, t.Namespace, ".", GetType().Name);
            int i = 0;

            foreach (revitCSLayer layer in csLayers)
            {
                s = string.Concat(s, string.Format("\n  Layer {0}: {1}, Width-> {2}, MaterialId-> {3}", i, layer.Function, layer.Width, layer.MaterialId));
                i++;
            }
            return s;
        }
    }
}
