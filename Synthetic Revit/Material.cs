using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitMaterial = Autodesk.Revit.DB.Material;
using Autodesk.Revit.DB.Visual;

using Revit.Elements;
using dynElem = Revit.Elements.Element;
using dynMaterial = Revit.Elements.Material;

namespace Synthetic.Revit
{
    /// <summary>
    /// Extensions of Dynamo Revit
    /// </summary>
    public class Material
    {
        internal Material () { }

        /// <summary>
        /// Gets a material given its name and document
        /// </summary>
        /// <param name="Name">Name of a material</param>
        /// <param name="Document">Document to get the material from</param>
        /// <returns name="Material">A Autodeks.Revit.DB.Material</returns>
        public static revitMaterial GetByNameDocument (string Name,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc Document)
        {
            revitDB.FilteredElementCollector collector
                = new revitDB.FilteredElementCollector(Document);

            collector
                .OfClass(typeof(revitDB.Material))
                .OfType<revitDB.Material>();

            return collector
                .OfType<revitDB.Material>()
                .FirstOrDefault(
                m => m.Name.Equals(Name));
        }

        /// <summary>
        /// Gets a material from the current doucment given its name
        /// </summary>
        /// <param name="Name">Name of material</param>
        /// <returns name="Material">A Autodeks.Revit.DB.Material</returns>
        public static revitMaterial GetByName (string Name)
        {
            return GetByNameDocument(Name, Synthetic.Revit.Document.Current());
        }

        /// <summary>
        /// Gets all the connected files with paths associated with a material.
        /// </summary>
        /// <param name="Material">A revit material</param>
        /// <returns name="Paths">Full file names with paths</returns>
        public static List<string> GetMaterialBitmapPaths (revitMaterial Material)
        {
            List<string> paths = new List<string>();

            revitDB.ElementId appearanceAssetID = Material.AppearanceAssetId;
            revitDB.AppearanceAssetElement assetElem = Material.Document.GetElement(appearanceAssetID) as revitDB.AppearanceAssetElement;

            Asset renderingAsset = assetElem.GetRenderingAsset();

            for (int idx=0; idx < renderingAsset.Size; idx++)
            {
                AssetProperty property = renderingAsset.Get(idx);
                paths.AddRange(_ReadAssetPropertyPaths(property));
            }

            return paths;
        }

        /// <summary>
        /// Checks if a AssetProperty has connected properties and if those connected properties have a bitmap path, it returns the path.
        /// </summary>
        /// <param name="assetProperty">A Revit AssetProperty element</param>
        /// <returns name="paths">Bitmap paths</returns>
        private static List<string> _ReadAssetPropertyPaths(AssetProperty assetProperty)
        {
            List<string> paths = new List<string>();

            Asset connectedAsset = assetProperty.GetSingleConnectedAsset();
            if (connectedAsset != null)
            {
                AssetPropertyString bitmapProperty = connectedAsset.FindByName(UnifiedBitmap.UnifiedbitmapBitmap) as AssetPropertyString;

                if (bitmapProperty == null)
                {
                    bitmapProperty = connectedAsset.FindByName(BumpMap.BumpmapBitmap) as AssetPropertyString;
                }
                if (bitmapProperty != null)
                {
                    paths.Add(bitmapProperty.Value);
                }                    
            }

            return paths;
        }
        //public static revitMaterial EditAppearancePaths (revitMaterial Material, List<string> Paths)
        //{
        //    revitDB.ElementId appearanceAssetID = Material.AppearanceAssetId;
        //    revitDB.AppearanceAssetElement assetElem = Material.Document.GetElement(appearanceAssetID) as revitDB.AppearanceAssetElement;

        //    using (revitDB.Transaction t = new revitDB.Transaction(Material.Document, "Change material bumpmap bitmap"))
        //    {
        //        t.Start();
        //        using (AppearanceAssetEditScope editScope = new AppearanceAssetEditScope(assetElem.Document))
        //        {
        //            Asset editableAsset = editScope.Start(assetElem.Id);

        //            //AssetProperty bumpMapProperty = editableAsset[Generic.GenericBumpMap];
                    

        //        }
        //    }

        //    return Material;
        //}

        
        
    }
}
