using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

using RevitDB = Autodesk.Revit.DB;
using RevitArch = Autodesk.Revit.DB.Architecture;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitFECollector = Autodesk.Revit.DB.FilteredElementCollector;
using RevitElem = Autodesk.Revit.DB.Element;

using Revit.Elements;
using DynElem = Revit.Elements.Element;
using DynCat = Revit.Elements.Category;
using RevitServices.Transactions;
using RevitServices.Persistence;

using SynthCollect = Synthetic.Revit.Collector;
using SynthCollectFilter = Synthetic.Revit.CollectorElementFilter;

namespace Synthetic.Revit
{
    /// <summary>
    /// Nodes that certain sets of elements using pre-configured Collectors and filters.
    /// </summary>
    [IsDesignScriptCompatible]
    public class Select
    {
        internal Select () { }

        /// <summary>
        /// Selects all elements of a type.  Works with documents other than the active document.
        /// </summary>
        /// <param name="type">The element type of the object, such as WallTypes or Walls.</param>
        /// <param name="inverted">If false, elements in the chosen category will be selected.  If true, elements NOT in the chosen category will be selected.</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="Elements">A list of Dynamo elements that pass the filer.</returns>
        public static IList<DynElem> AllElementsOfType (Type type,
            [DefaultArgument("false")] bool inverted,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            SynthCollect collector = new SynthCollect(document);
            List<RevitDB.ElementFilter> filters = new List<Autodesk.Revit.DB.ElementFilter>();
            filters.Add(SynthCollectFilter.FilterElementClass(type, inverted));

            SynthCollect.SetFilters(collector, filters);

            return SynthCollect.ToElements(collector);
        }

        /// <summary>
        /// Selects all instance elements in a category, excludes element types.
        /// </summary>
        /// <param name="category">The categoryId of the elements you wish to select.</param>
        /// <param name="inverted">If false, elements in the chosen category will be selected.  If true, elements NOT in the chosen category will be selected.</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="Elements">A list of Dynamo elements that pass the filer.</returns>
        public static IList<DynElem> AllElementsOfCategory(DynCat category,
            [DefaultArgument("false")] bool inverted,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            SynthCollect collector = new SynthCollect(document);

            // Select only elements that are NOT Types (the filter is inverted)
            List<RevitDB.ElementFilter> filters = new List<Autodesk.Revit.DB.ElementFilter>();
            filters.Add(SynthCollectFilter.FilterElementIsElementType(true));
            filters.Add(SynthCollectFilter.FilterElementCategory(category, inverted));

            SynthCollect.SetFilters(collector, filters);

            return SynthCollect.ToElements(collector);
        }

        /// <summary>
        /// Selects all Family Symbol types in a category, but excludes instances of those elements.  The node does not work with System familes because System Families do not have a Family Sybmol.
        /// </summary>
        /// <param name="category">The categoryId of the elements you wish to select.</param>
        /// <param name="inverted">If false, elements in the chosen category will be selected.  If true, elements NOT in the chosen category will be selected.</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="Elements">A list of Dynamo elements that pass the filer.</returns>
        public static IList<DynElem> AllFamilyTypesOfCategory(DynCat category,
            [DefaultArgument("false")] bool inverted,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            SynthCollect collector = new SynthCollect(document);

            // Select only elements that are Family Symbols
            List<RevitDB.ElementFilter> filters = new List<Autodesk.Revit.DB.ElementFilter>();
            filters.Add(SynthCollectFilter.FilterElementClass(typeof(RevitDB.FamilySymbol), false));
            filters.Add(SynthCollectFilter.FilterElementCategory(category, inverted));

            SynthCollect.SetFilters(collector, filters);

            return SynthCollect.ToElements(collector);
        }

        /// <summary>
        /// Retrieve all materials in the document.
        /// </summary>
        /// <param name="document">>A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="Materials">A list of Auotdesk.Revit.DB.Materials</returns>
        public static IEnumerable<RevitDB.Material> AllMaterials([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            RevitFECollector collector
                = new RevitFECollector(document);

            return collector
                .OfClass(typeof(RevitDB.Material))
                .OfType<RevitDB.Material>();
        }

        /// <summary>
        /// Given a list of materials, returns the material that matches the given name.
        /// </summary>
        /// <param name="materials">A list of Autodesk.Revit.DB.Materials</param>
        /// <param name="materialName">The name of the material</param>
        /// <returns name="Material">A Autodesk.Revit.DB.Material that matches the given name.</returns>
        public static RevitDB.Material GetMaterialByName(IEnumerable<RevitDB.Material> materials, string materialName)
        {
            return materials
                .OfType<RevitDB.Material>()
                .FirstOrDefault(
                m => m.Name.Equals(materialName));
        }

        public static RevitDB.Element RevitElementByNameClass( string Name, Type Class,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            RevitFECollector collector
                = new RevitFECollector(document);

            RevitDB.Element elem = collector
                .OfClass(Class)
                .FirstOrDefault(e => e.Name.Equals(Name));

            return elem;
        }

        public static DynElem DynamoElementByNameClass(string Name, Type Class,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            RevitFECollector collector
                = new RevitFECollector(document);

            RevitDB.Element elem = collector
                .OfClass(Class)
                .FirstOrDefault(e => e.Name.Equals(Name));

            DynElem dElem = elem.ToDSType(true);

            return dElem;
        }

        /// <summary>
        /// Get the Type of a Revit Class from RevitAPI.dll given its name.
        /// </summary>
        /// <param name="typeName">Name of the Autodesk.Revit.DB Class</param>
        /// <returns name="Type">The Type of a Revit Class</returns>
        public static Type RevitClassByString(string typeName)
        {
            // Assembly and Class that the Element should be
            Assembly assembly = typeof(RevitElem).Assembly;
            Type elemClass = assembly.GetType(typeName);

            return elemClass;
        }

        /// <summary>
        /// Given the an ElementType Type retrieves the corresponding Instance Type.  For example Type WallType returns Type Wall or Type TextNoteType returns Type TextNote.
        /// </summary>
        /// <param name="elementType">A Type of ElementType</param>
        /// <returns name="instanceType">The Type of Instance</returns>
        public static Type InstanceClassFromTypeClass (Type elementType)
        {
            Type instanceType = null;

            // Create a dictionary of ElementTypes and InstanceTypes.
            Dictionary<Type, Type> types = new Dictionary<Type, Type>();
            types.Add(typeof(RevitDB.FamilySymbol), typeof(RevitDB.FamilyInstance));
            types.Add(typeof(RevitDB.TextNoteType), typeof(RevitDB.TextNote));
            types.Add(typeof(RevitDB.DimensionType), typeof(RevitDB.Dimension));
            types.Add(typeof(RevitDB.WallType), typeof(RevitDB.Wall));
            types.Add(typeof(RevitDB.FloorType), typeof(RevitDB.Floor));
            types.Add(typeof(RevitDB.CeilingType), typeof(RevitDB.Ceiling));
            types.Add(typeof(RevitDB.RoofType), typeof(RevitDB.RoofBase));
            types.Add(typeof(RevitDB.BuildingPadType), typeof(RevitArch.BuildingPad));
            types.Add(typeof(RevitDB.WallFoundationType), typeof(RevitDB.WallFoundation));
            types.Add(typeof(RevitDB.BeamSystemType), typeof(RevitDB.BeamSystem));
            types.Add(typeof(RevitDB.GroupType), typeof(RevitDB.Group));
            types.Add(typeof(RevitDB.ViewFamilyType), typeof(RevitDB.View));
            types.Add(typeof(RevitDB.FilledRegionType), typeof(RevitDB.FilledRegion));
            types.Add(typeof(RevitDB.GridType), typeof(RevitDB.Grid));
            types.Add(typeof(RevitDB.LevelType), typeof(RevitDB.Level));
            types.Add(typeof(RevitDB.TextElementType), typeof(RevitDB.TextElement));
            types.Add(typeof(RevitDB.RevitLinkType), typeof(RevitDB.RevitLinkInstance));
            types.Add(typeof(RevitDB.AssemblyType), typeof(RevitDB.AssemblyInstance));

            if (types.ContainsKey(elementType))
            {
                instanceType = types[elementType];
            }

            return instanceType;
        }
    }
}
