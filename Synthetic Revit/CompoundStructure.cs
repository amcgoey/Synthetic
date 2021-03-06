﻿using System;
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
        /// Constructs a CompoundStructure given a Autodesk.Revit.DB.CompoundStructure and a Autodesk.Revit.DB.Document
        /// </summary>
        /// <param name="cs">Autodesk.Revit.DB.CompoundStructure</param>
        /// <param name="doc">Autodesk.Revit.DB.Document</param>
        internal CompoundStructure(revitCS cs, revitDoc doc)
        {
            internalCompoundStructure = cs;
            internalDocument = doc;
        }

        /// <summary>
        /// Creates a Autodesk.Revit.DB.CompoundStructure given a list of Autodesk.Revit.DB.CompoundStructureLayer elements
        /// </summary>
        /// <param name="layers">A list of CompoundStructureLayer elements</param>
        /// <param name="doc">Autodesk.Revit.DB.Document</param>
        /// <returns name="RevitCompoundStructure">A Autodesk.Revit.DB.CompoundStructure</returns>
        internal CompoundStructure(cg.List<revitCSLayer> layers, revitDoc doc)
        {
            internalCompoundStructure = revitCS.CreateSimpleCompoundStructure(layers);
            internalDocument = doc;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Returns the properties of a Autodesk.Revit.DB.CompoundStructureLayer as a dictionary.
        /// </summary>
        /// <param name="layer">Autodesk.Revit.DB.CompoundStructureLayer</param>
        /// <param name="doc">Autodesk.Revit.DB.Document</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a list of dictionaries with Autodesk.Revit.DB.CompoundStructureLayer properties.
        /// </summary>
        /// <param name="cs">Autodesk.Revit.DB.CompoundStructure</param>
        /// <param name="doc">Autodesk.Revit.DB.Document</param>
        /// <returns></returns>
        internal static cg.IList<cg.Dictionary<string, object>> _GetLayers(revitCS cs, revitDoc doc)
        {
            cg.IList<cg.Dictionary<string, object>> layers = new cg.List<cg.Dictionary<string, object>>();

            foreach (revitCSLayer layer in cs.GetLayers())
            {
                layers.Add(_RevitLayerToDictionary(layer, doc));
            }

            return layers;
        }

        /// <summary>
        /// Creates Synthetic.Revit.CompoundStructure in a destination document by changing the material ids to correspond to materials in the destination document.  Materials not in the destination document are copied into the document.
        /// </summary>
        /// <param name="compoundStructure">A Synthetic.Revit.CompoundStructure from the source document</param>
        /// <param name="destinationDoc">The document to copy the CompoundStructure into.</param>
        /// <returns name="compoundStructure">A Synthetic.Revit.CompoundStructure in the destination document.</returns>
        internal static CompoundStructure _CopyToDocument(CompoundStructure compoundStructure, revitDoc destinationDoc)
        {
            revitDoc doc = compoundStructure.internalDocument;

            cg.List<revitCSLayer> destinationLayers = new cg.List<revitCSLayer>();

            foreach (revitCSLayer sourceLayer in compoundStructure.internalCompoundStructure.GetLayers())
            {
                revitCSLayer destintationLayer = sourceLayer;

                revitDB.Material sourceMaterial = (revitDB.Material)doc.GetElement(sourceLayer.MaterialId);
                revitDB.Material destinationMaterial = Select.GetMaterialByName(Select.AllMaterials(destinationDoc), sourceMaterial.Name);

                if (destinationMaterial == null)
                {
                    cg.List<int> sourceMaterialIds = new cg.List<int>();
                    sourceMaterialIds.Add(sourceMaterial.Id.IntegerValue);
                    cg.List<revitDB.ElementId> destinationElemIds = Elements.CopyElements(compoundStructure.internalDocument, sourceMaterialIds, destinationDoc);
                    //destinationMaterial = Select.GetMaterialByName(Select.AllMaterials(destinationDoc), sourceMaterial.Name);
                    destintationLayer.MaterialId = destinationElemIds[0];
                }
                else
                {
                    destintationLayer.MaterialId = destinationMaterial.Id;
                }

                destinationLayers.Add(destintationLayer);
            }

            return new CompoundStructure(destinationLayers, destinationDoc);
        }

        #endregion

        #region Public Constructors
        /// <summary>
        /// Creates a Synthetic.Revit.CompoundStructure given lists of layer properties.  Please note that layers will only be made with the shortest number of complete layer properties.  For example if five widths and layer functions are provided but only four materials, only four layers will be created.
        /// </summary>
        /// <param name="width">List with the width of each layer.</param>
        /// <param name="layerFunction">List with the Autodesk.Revit.DB.MaterialFunctionAssignment enumerations for each layer.</param>
        /// <param name="material">List of Autodesk.Revit.DB.Materials for each layer.  Dynamo wrapped Revit.Material objects will not work.</param>
        /// <param name="document">An unwrapped document associated with the CompoundStructure.</param>
        /// <returns name="compoundStructure">A Compound Structure.</returns>
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

            //return new CompoundStructure(revitCS.CreateSimpleCompoundStructure(layerList), document);
            return new CompoundStructure(layerList, document);
        }

        /// <summary>
        /// Creates a compound structure from a list of dictionary layer properties.
        /// </summary>
        /// <param name="layers">A list of dictionary objects with layer properties.</param>
        /// <param name="document">An unwrapped document associated with the CompoundStructure.</param>
        /// <returns name="compoundStructure">A Compound Structure.</returns>
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

            //return new CompoundStructure(revitCS.CreateSimpleCompoundStructure(layerList), document);
            return new CompoundStructure(layerList, document);
        }

        /// <summary>
        /// Creates a CompoundStructure from a list of Autodesk.Revit.DB.CompoundStructureLayer elements.
        /// </summary>
        /// <param name="layers">A list of Autodesk.Revit.DB.CompoundStructureLayer elements.</param>
        /// <param name="document">An unwrapped document associated with the CompoundStructure.</param>
        /// <returns name="compoundStructure">A Compound Structure.</returns>
        public static CompoundStructure ByCompoundStructureLayers(cg.List<revitCSLayer> layers,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            //return new CompoundStructure(revitCS.CreateSimpleCompoundStructure(layers), document);
            return new CompoundStructure(layers, document);
        }

        /// <summary>
        /// Creates a compound structure from a wall type.
        /// </summary>
        /// <param name="wallType">A Dynamo wrapped Revit.WallType.</param>
        /// <param name="document">An unwrapped document associated with the CompoundStructure.</param>
        /// <returns name="compoundStructure">A Compound Structure.</returns>
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
        /// Given a Autodesk.Revit.DB.CompoundStructure, creates a Synthetic.Revit.CompoundStructure.
        /// </summary>
        /// <param name="compoundStructure">A Autodesk.Revit.DB.CompoundStructure</param>
        /// <param name="document">An unwrapped document associated with the CompoundStructure.</param>
        /// <returns name="compoundStructure">A Compound Structure.</returns>
        public static CompoundStructure Wrap(revitCS compoundStructure,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            return new CompoundStructure(compoundStructure, document);
        }

        /// <summary>
        /// Retrieves the Autodesk.Revit.DB.CompoundStructure from a Synthetic.Revit.CompoundStructure object.  This is useful for using CompoundStructures in python scripts or other methods of accessing the Revit API directly.
        /// </summary>
        /// <param name="compoundStructure">An Synthetic.Revit.CompoundStructure</param>
        /// <returns name="Unwrapped">Autodesk.Revit.DB.CompoundStructure</returns>
        public static revitCS UnwrapCompoundStructure(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalCompoundStructure;
        }

        #endregion

        #region Element Modification Methods
        /// <summary>
        /// Replaces a Wall Type's compound structure with the given one.  Please note that the compound structure's materials and the wall type must be in the same document or unexpected results may occur.
        /// </summary>
        /// <param name="WallType">The wall type to be modified.</param>
        /// <param name="compoundStructure">A compound structure</param>
        /// <returns name="wallType">The modified wall type.</returns>
        public static dynamoElements.WallType ToWallType(dynamoElements.WallType WallType, CompoundStructure compoundStructure)
        {
            revitDB.WallType revitWallType = (revitDB.WallType)WallType.InternalElement;

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
            return WallType;
        }

        #endregion

        #region Compound Structure Modification Methods

        /// <summary>
        /// Sets the properties of a layer at the specified index in the CompoundStructure.
        /// </summary>
        /// <param name="compoundStructure">The CompoundStructure to modify.</param>
        /// <param name="layerIndex">Index of the layer to be modified.</param>
        /// <param name="width">Width of the layer.</param>
        /// <param name="layerFunction">Autodesk.Revit.DB.MaterialFunctionAssignment enumeration of the layer.</param>
        /// <param name="material">Autodesk.Revit.DB.Materials of the layer.  Dynamo wrapped Revit.Material objects will not work.</param>
        /// <returns name="compoundStructure">The modified CompoundStructure.</returns>
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
        /// Creates a new layer at the specified index in the CompoundStructure.
        /// </summary>
        /// <param name="compoundStructure">The CompoundStructure to modify.</param>
        /// <param name="layerIndex">Index of the layer to be inserted.</param>
        /// <param name="width">Width of the layer.</param>
        /// <param name="layerFunction">Autodesk.Revit.DB.MaterialFunctionAssignment enumeration of the layer.</param>
        /// <param name="material">Autodesk.Revit.DB.Materials of the layer.  Dynamo wrapped Revit.Material objects will not work.</param>
        /// <returns name="compoundStructure">The modified CompoundStructure.</returns>
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
        /// Removes a layer at the specified index in the CompoudStructure.
        /// </summary>
        /// <param name="compoundStructure">The CompoundStructure to modify.</param>
        /// <param name="layerIndex">Index of the layer to be deleted.</param>
        /// <returns name="compoundStructure">The modified CompoundStructure.</returns>
        public static CompoundStructure DeleteLayer(CompoundStructure compoundStructure, int layerIndex)
        {
            bool result = compoundStructure.internalCompoundStructure.DeleteLayer(layerIndex);
            if (result == true) { return compoundStructure; }
            else { return null; }
        }

        /// <summary>
        /// Creates Synthetic.Revit.CompoundStructure in a destination document by changing the material ids to correspond to materials in the destination document.  Materials not in the destination document are copied into the document.
        /// </summary>
        /// <param name="compoundStructure">A Synthetic.Revit.CompoundStructure from the source document</param>
        /// <param name="destinationDocument">The document to copy the CompoundStructure into.</param>
        /// <returns name="compoundStructure">A Synthetic.Revit.CompoundStructure in the destination document.</returns>
        public static CompoundStructure CopyToDocument(CompoundStructure compoundStructure, revitDoc destinationDocument)
        {
            return _CopyToDocument(compoundStructure, destinationDocument);
        }

        #endregion

        #region Get CompoundStructure Methods

        /// <summary>
        /// Retrieves a list of the properties of the layers in the CompoundStructure.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <returns name="layers">A list of properties for each layer.  Note that the layers are dictionaries and properties can be retrieved using the keys "Width", "Layer Function", and "Material".</returns>
        public static cg.IList<cg.Dictionary<string, object>> GetLayers(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalLayers;
        }

        /// <summary>
        /// Retrieves the width of a layer at the specified index.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <param name="layerIndex">Index of the layer</param>
        /// <returns name="Width">Width of the layer</returns>
        public static double GetLayerWidth(CompoundStructure compoundStructure, int layerIndex)
        {
            return compoundStructure.internalCompoundStructure.GetLayerWidth(layerIndex);
        }

        /// <summary>
        /// Retrieves the Autodesk.Revit.DB.MaterialFunctionAssignment of the layer at the specified index.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <param name="layerIndex">Index of the layer</param>
        /// <returns name="layerFunction">The material function of the layer</returns>
        public static revitDB.MaterialFunctionAssignment GetLayerFunction(CompoundStructure compoundStructure,
            int layerIndex)
        {
            return compoundStructure.internalCompoundStructure.GetLayerFunction(layerIndex);
        }

        /// <summary>
        /// Retrieves the Autodesk.Revit.DB.Material of the layer at the specified index.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <param name="layerIndex">Index of the layer</param>
        /// <returns name="material">The material of the layer as a Autodesk.Revit.DB.Material, not a Dynamo wrapped Revit.Material.</returns>
        public static revitDB.Material GetLayerMaterial(CompoundStructure compoundStructure, int layerIndex)
        {
            revitDoc doc = compoundStructure.internalDocument;

            revitDB.ElementId materialId = compoundStructure.internalCompoundStructure.GetMaterialId(layerIndex);
            return (revitDB.Material)doc.GetElement(materialId);
        }

        /// <summary>
        /// Retrieves the material ID of the material for the layer at the specified index.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <param name="layerIndex">Index of the layer</param>
        /// <returns name="materialId">The Element ID of the material for the layer.</returns>
        public static int GetLayerMaterialId(CompoundStructure compoundStructure, int layerIndex)
        {
            return compoundStructure.internalCompoundStructure.GetMaterialId(layerIndex).IntegerValue;
        }

        /// <summary>
        /// Retrieves the index value of the most exterior core layer.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <returns name="index">List index</returns>
        public static int GetFirstCoreLayerIndex(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalFirstCoreLayerIndex;
        }

        /// <summary>
        /// Retrieves the index value of the most interior core layer.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <returns name="index">List index</returns>
        public static int GetLastCoreLayerIndex(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalLastCoreLayerIndex;
        }



        #endregion

        #region Set Compound Structure Methods

        /// <summary>
        /// Sets the width of the layer at the specified index.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <param name="layerIndex">Index of the layer</param>
        /// <param name="width">Width of the layer</param>
        /// <returns name="compoundStructure">The modifed CompoundStructure</returns>
        public static CompoundStructure SetLayerWidth(CompoundStructure compoundStructure, int layerIndex, double width)
        {
            compoundStructure.internalCompoundStructure.SetLayerWidth(layerIndex, width);
            return compoundStructure;
        }

        /// <summary>
        /// Sets the Autodesk.Revit.DB.MaterialFunctionAssignment of the layer at the specified index.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <param name="layerIndex">Index of the layer</param>
        /// <param name="layerFunction">The material function of the layer</param>
        /// <returns name="compoundStructure">The modifed CompoundStructure</returns>
        public static CompoundStructure SetLayerFunction(CompoundStructure compoundStructure,
            int layerIndex,
            revitDB.MaterialFunctionAssignment layerFunction)
        {
            compoundStructure.internalCompoundStructure.SetLayerFunction(layerIndex, layerFunction);
            return compoundStructure;
        }

        /// <summary>
        /// Sets the material of the layer at the specified index.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <param name="layerIndex">Index of the layer</param>
        /// <param name="material">A Autodesk.Revit.DB.Material element for the layer.  A Dynamo wrapped Revit.Material element will not work.</param>
        /// <returns name="compoundStructure">The modifed CompoundStructure</returns>
        public static CompoundStructure SetLayerMaterial(CompoundStructure compoundStructure, int layerIndex, revitDB.Material material)
        {
            compoundStructure.internalCompoundStructure.SetMaterialId(layerIndex, material.Id);
            return compoundStructure;
        }

        /// <summary>
        /// Sets the material of the layer at the specified index.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <param name="layerIndex">Index of the layer</param>
        /// <param name="materialId">The Element ID of the material of the layer.</param>
        /// <returns name="compoundStructure">The modifed CompoundStructure</returns>
        public static CompoundStructure SetLayerMaterialId(CompoundStructure compoundStructure, int layerIndex, int materialId)
        {
            compoundStructure.internalCompoundStructure.SetMaterialId(layerIndex, new revitDB.ElementId(materialId));
            return compoundStructure;
        }

        /// <summary>
        /// Sets the number of layers that will be exterior of the core.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <param name="numLayers">Number of exterior layers</param>
        /// <returns name="compoundStructure">The modifed CompoundStructure</returns>
        public static CompoundStructure SetNumberOfExteriorLayers(CompoundStructure compoundStructure, int numLayers)
        {
            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(compoundStructure.internalDocument))
            {
                compoundStructure.internalFirstCoreLayerIndex = numLayers;
            }
            return compoundStructure;
        }

        /// <summary>
        /// Sets the number of layers that will be exterior of the core.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <param name="numLayers">Number of interior layers</param>
        /// <returns name="compoundStructure">The modifed CompoundStructure</returns>
        public static CompoundStructure SetNumberOfInteriorLayers(CompoundStructure compoundStructure, int numLayers)
        {
            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(compoundStructure.internalDocument))
            {
                compoundStructure.internalLastCoreLayerIndex = numLayers;
            }
            return compoundStructure;
        }
        #endregion

        #region CompoundStructureLayer Modification Methods

        /// <summary>
        /// Retrieves the Autodesk.Revit.DB.CompoundStructureLayer elements from the CompoundStructure.  For use with python or other methods for using the Revit API directly.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <returns name="CompoundStructureLayers">A list of CompoundStructureLayer elements</returns>
        public static cg.IList<revitCSLayer> GetCompoundStructureLayers(CompoundStructure compoundStructure)
        {
            return compoundStructure.internalCompoundStructure.GetLayers();
        }

        /// <summary>
        /// Sets the layers for the CompoundStructure with a list of Autodesk.Revit.DB.CompoundStructureLayer elements from the CompoundStructure.  For use with python or other methods for using the Revit API directly.
        /// </summary>
        /// <param name="compoundStructure">A CompoundStructure</param>
        /// <param name="compoundStructureLayers">A list of Autodesk.Revit.DB.CompoundStructureLayer elements</param>
        /// <returns name="compoundStructure">The modified CompoundStructure</returns>
        public static CompoundStructure SetCompoundStructureLayers(CompoundStructure compoundStructure, cg.IList<revitCSLayer> compoundStructureLayers)
        {
            compoundStructure.internalCompoundStructure.SetLayers(compoundStructureLayers);
            return compoundStructure;
        }

        /// <summary>
        /// Retrives the properties of a Autodesk.Revit.DB.CompoundStructureLayer element.
        /// </summary>
        /// <param name="compoundStructureLayer">A CompoundStructureLayer</param>
        /// <param name="document">An unwrapped document associated with the CompoundStructure.</param>
        /// <returns name="dict">A dictionary element containing the layer properties.  Note that the layers are dictionaries and properties can be retrieved using the keys "Width", "Layer Function", and "Material".</returns>
        public static synthDict CompoundStructureLayerToDictionary(revitCSLayer compoundStructureLayer,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            return synthDict.Wrap((cg.Dictionary<string, object>)_RevitLayerToDictionary(compoundStructureLayer, document));
        }

        /// <summary>
        /// Retrives the properties of a Autodesk.Revit.DB.CompoundStructureLayer element.
        /// </summary>
        /// <param name="compoundStructureLayer">A CompoundStructureLayer</param>
        /// <param name="document">An unwrapped document associated with the CompoundStructure.</param>
        /// <returns name="Width">Width of the layer.</returns>
        /// <returns name="Layer Function">Autodesk.Revit.DB.MaterialFunctionAssignment enumeration of the layer.</returns>
        /// <returns name="Material">Autodesk.Revit.DB.Material of the layer.</returns>
        [MultiReturn(new[] { "Width", "Layer Function", "Material" })]
        public static IDictionary CompoundStructureLayerToList(revitCSLayer compoundStructureLayer,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            return _RevitLayerToDictionary(compoundStructureLayer, document);
        }

        /// <summary>
        /// Creates a CompoundStructureLayer from properties of the layer.
        /// </summary>
        /// <param name="width">Width of the layer.</param>
        /// <param name="layerFunction">Autodesk.Revit.DB.MaterialFunctionAssignment enumeration of the layer.</param>
        /// <param name="materialId">The Element ID of the material for the layer.</param>
        /// <returns name="layer">A CompoundStructureLayer element.</returns>
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
            string s = base.ToString();

            if (this.internalLayers.Count != 0)
            {
                s = s + ": Core Layers " + internalFirstCoreLayerIndex + " to " + internalLastCoreLayerIndex;

                foreach (cg.Dictionary<string, object> layer in this.internalLayers)
                {
                    string materialName = "<By Category>";
                    string functionName = "None";
                    string widthName = "None";

                    revitDB.Material material = (revitDB.Material)layer["Material"];
                    if (material != null) { materialName = material.Id.ToString(); }
                    if (layer["Layer Function"] != null) { functionName = layer["Layer Function"].ToString(); }
                    if (layer["Width"] != null) { widthName = layer["Width"].ToString(); }

                    s = s + "\n  Layer " + i + ": " + layer["Layer Function"] + ", Width-> " + layer["Width"] + ", MaterialId-> " + materialName;
                    i++;
                }
            }
            return s;
        }

        #endregion
    }
}