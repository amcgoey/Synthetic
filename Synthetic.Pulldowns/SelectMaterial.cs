using System;

using Dynamo.Graph.Nodes;

using Synthetic.Pulldowns;
using Synthetic.Revit;

using revitDB = Autodesk.Revit.DB;

namespace Synthetic.Revit.Select
{
    /// <summary>
    /// 
    /// </summary>
    [NodeName("Select Material")]
    [NodeDescription("Choose a material from the current document.")]

    [IsDesignScriptCompatible]
    public class SelectMaterial : GenericRevitElementsDropDown
    {
        /// <summary>
        /// 
        /// </summary>
        public SelectMaterial () : base("Select Material",
            typeof(revitDB.Material),
            Synthetic.Revit.Material.GetByName) { }
    }
}
