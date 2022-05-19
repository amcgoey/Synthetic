using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using RevitServices.Transactions;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitMaterial = Autodesk.Revit.DB.Material;
using Autodesk.Revit.DB.Visual;

using Synthetic.Core;

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
        /// Retrives the Autodesk.Revit.DB.Material element from the Dynamo Material element.
        /// </summary>
        /// <param name="DynamoMaterial">A Dynamo wrapped material element</param>
        /// <returns name="Revit Materials">Autodesk.Revit.DB.Material element</returns>
        public static revitMaterial UnwrapMaterial (dynMaterial DynamoMaterial)
        {
            return DynamoMaterial.InternalElement as revitMaterial;
        }

        /// <summary>
        /// Gets all the connected files with paths associated with a material.
        /// </summary>
        /// <param name="Material">A revit material</param>
        /// <returns name="Paths">Full file names with paths</returns>
        public static List<string> GetMaterialBitmapPaths (revitMaterial Material)
        {
            List<string> paths = null;

            revitDB.ElementId appearanceAssetID = Material.AppearanceAssetId;

            if (appearanceAssetID.IntegerValue != -1)
            {
                paths = new List<string>();
                revitDB.AppearanceAssetElement assetElem = Material.Document.GetElement(appearanceAssetID) as revitDB.AppearanceAssetElement;

                if (assetElem != null)
                {
                    Asset renderingAsset = assetElem.GetRenderingAsset();

                    for (int idx = 0; idx < renderingAsset.Size; idx++)
                    {
                        AssetProperty property = renderingAsset.Get(idx);
                        paths.AddRange(_ReadAssetPropertyPaths(property));
                    }
                }
            }
            return paths;
        }

        /// <summary>
        /// Given a material and a SearchPaths object, the method will replace the path of any bitmaps in the material based on the SearchPaths.
        /// </summary>
        /// <param name="Material">A Autodesk.Revit.DB.Material element</param>
        /// <param name="searchPaths">A SearchPaths object that includes a prioritzed list of search paths</param>
        /// <param name="ReplaceRelativePaths">If True, replace files that are Relative Paths, otherwise don't replace.</param>
        /// <returns name="Paths Replaced">A list of the paths replaced</returns>
        /// <returns name="Paths NOT Replaced">A list of the paths that were left unchanged.</returns>
        [MultiReturn(new[] { "Paths Replaced", "Paths NOT Replaced" })]
        public static Dictionary<string, object> ReplaceBitmapPaths (revitMaterial Material,
            SearchPaths searchPaths,
            bool ReplaceRelativePaths = false
            )
        {
            List<List<string>> results = null;

            if (searchPaths != null && Material != null)
            {
                revitDB.ElementId appearanceAssetID = Material.AppearanceAssetId;

                if (appearanceAssetID.IntegerValue != -1)
                {
                    revitDB.AppearanceAssetElement assetElem = Material.Document.GetElement(appearanceAssetID) as revitDB.AppearanceAssetElement;

                    if (assetElem != null)
                    {
                        string transactionName = "Replace Bitmap Paths on Material " + Material.Name;
                        revitDoc document = Material.Document;

                        if (document.IsModifiable)
                        {
                            TransactionManager.Instance.EnsureInTransaction(document);
                            results = _ReplaceBitmapPaths(assetElem, searchPaths, ReplaceRelativePaths);
                            TransactionManager.Instance.TransactionTaskDone();
                            //TransactionManager.Instance.ForceCloseTransaction();
                        }
                        else
                        {
                            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                            {
                                trans.Start(transactionName);
                                results = _ReplaceBitmapPaths(assetElem, searchPaths, ReplaceRelativePaths);
                                trans.Commit();
                            }
                        }
                    }
                }
            }
            if (results != null)
            {
                return new Dictionary<string, object>
            {
                {"Paths Replaced", results[0]},
                {"Paths NOT Replaced", results[1] }
            };
            }
            else
            {
                return new Dictionary<string, object>
            {
                {"Paths Replaced", null},
                {"Paths NOT Replaced", null}
            };
            }
        }

        private static List<List<string>> _ReplaceBitmapPaths (revitDB.AppearanceAssetElement assetElem, SearchPaths searchPaths, bool ReplaceRelativePaths)
        {
            List<string> pathsReplaced = new List<string>();
            List<string> pathsNotReplaced = new List<string>();
            string file = null;
            string filePath = null;
            string newFilePath = null;

            using (AppearanceAssetEditScope editScope = new AppearanceAssetEditScope(assetElem.Document))
            {
                // returns an editable copy of the appearance asset
                Asset editableAsset = editScope.Start(assetElem.Id);

                for (int idx = 0; idx < editableAsset.Size; idx++)
                {
                    AssetProperty property = editableAsset.Get(idx);
                    Asset connectedAsset = property.GetSingleConnectedAsset();

                    if (connectedAsset != null)
                    {
                        AssetPropertyString bitmapProperty = connectedAsset.FindByName(UnifiedBitmap.UnifiedbitmapBitmap) as AssetPropertyString;

                        if (bitmapProperty == null)
                        {
                            bitmapProperty = connectedAsset.FindByName(BumpMap.BumpmapBitmap) as AssetPropertyString;
                        }
                        if (bitmapProperty != null)
                        {
                            char[] separator = { '|' };

                            filePath = bitmapProperty.Value;

                            if (filePath != "")
                            {
                                // If the Path is from the Revit Library it will contain a '|' character.
                                // If ReplaceRelativePaths is True, then generate a new path
                                // Otherwise, if the path is absolute, then generate a new path
                                if (filePath.Contains(separator[0]) && ReplaceRelativePaths)
                                {
                                    filePath = filePath.Split(separator).Last();
                                    file = Path.GetFileName(filePath);
                                    newFilePath = searchPaths.GetFilePath(file);
                                }
                                else if (Material.IsPathFullyQualified(filePath) || ReplaceRelativePaths)
                                {
                                    file = Path.GetFileName(filePath);
                                    newFilePath = searchPaths.GetFilePath(file);
                                }

                                // If a new path is found and it is a valid proerpty value, the edit the path.
                                // Else, record that the path was not changed.
                                if (newFilePath != null && bitmapProperty.IsValidValue(newFilePath))
                                {
                                    bitmapProperty.Value = newFilePath;
                                    pathsReplaced.Add(newFilePath);
                                }
                                else
                                {
                                    pathsNotReplaced.Add(filePath);
                                }
                            }
                        }
                    }
                }
                editScope.Commit(true);
            }
            return new List<List<string>> { { pathsReplaced}, { pathsNotReplaced} };
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

        private static bool IsPathFullyQualified(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (path.Length < 2) return false; //There is no way to specify a fixed path with one character (or less).
            if (path.Length == 2 && IsValidDriveChar(path[0]) && path[1] == System.IO.Path.VolumeSeparatorChar) return true; //Drive Root C:
            if (path.Length >= 3 && IsValidDriveChar(path[0]) && path[1] == System.IO.Path.VolumeSeparatorChar && IsDirectorySeperator(path[2])) return true; //Check for standard paths. C:\
            if (path.Length >= 3 && IsDirectorySeperator(path[0]) && IsDirectorySeperator(path[1])) return true; //This is start of a UNC path
            return false; //Default
        }

        private static bool IsDirectorySeperator(char c) => c == System.IO.Path.DirectorySeparatorChar | c == System.IO.Path.AltDirectorySeparatorChar;
        private static bool IsValidDriveChar(char c) => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
    }
}
