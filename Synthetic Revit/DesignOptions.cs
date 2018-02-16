using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitElem = Autodesk.Revit.DB.Element;

using Revit.Elements;
using dynElem = Revit.Elements.Element;

namespace Synthetic.Revit
{
    /// <summary>
    /// 
    /// </summary>
    public class DesignOptions
    {
        internal DesignOptions () { }

        /// <summary>
        /// Given the name of the design option set, returns a list of set's design options.
        /// </summary>
        /// <param name="DesignOptionSetName">The name of the Design Option Set</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="DesignOptions">A list of design options that belong to the DesignOptionSet</returns>
        public static IList<dynElem> GetDesignOptionsBySet (string DesignOptionSetName,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            revitDB.ElementCategoryFilter catFilter = new revitDB.ElementCategoryFilter(revitDB.BuiltInCategory.OST_DesignOptionSets);
            revitDB.FilteredElementCollector collectSets = new revitDB.FilteredElementCollector(document);
            IList<revitElem> sets =  collectSets
                .WherePasses(catFilter)
                .Cast<revitDB.Element>()
                .Where(elem => elem.Name == DesignOptionSetName)
                .ToList();

            IList<revitDB.ElementFilter> paramFilters = new List<revitDB.ElementFilter>();

            foreach (revitElem set in sets)
            {
                revitDB.ElementId id = set.Id;
                revitDB.FilterNumericRuleEvaluator eval = new revitDB.FilterNumericEquals();
                revitDB.ParameterValueProvider provider = new revitDB.ParameterValueProvider(new revitDB.ElementId(revitDB.BuiltInParameter.OPTION_SET_ID));
                revitDB.FilterRule filterRule = new revitDB.FilterElementIdRule(provider, eval, id);
                
                paramFilters.Add(new revitDB.ElementParameterFilter(filterRule));
            }

            revitDB.ElementFilter orFilter = new revitDB.LogicalOrFilter(paramFilters);

            catFilter = new revitDB.ElementCategoryFilter(revitDB.BuiltInCategory.OST_DesignOptions);

            revitDB.LogicalAndFilter filter = new revitDB.LogicalAndFilter(new List<revitDB.ElementFilter>{ orFilter, catFilter });
            

            revitDB.FilteredElementCollector collectOptions = new revitDB.FilteredElementCollector(document);
            IList<revitDB.Element> elements = collectOptions.WherePasses(filter).ToElements();

            IList<dynElem> dynamoElements = new List<dynElem>();

            foreach (revitDB.Element elem in elements)
            {
                try
                {
                    dynamoElements.Add(elem.ToDSType(true));
                }
                catch { }
            }

            return dynamoElements;
        }

        /// <summary>
        /// Gets all DesignOptionSets in the document
        /// </summary>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="DesignOptionSets">The design option sets in the document</returns>
        public static IList<revitElem> DesignOptionSets ([DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            revitDB.ElementCategoryFilter catFilter = new revitDB.ElementCategoryFilter(revitDB.BuiltInCategory.OST_DesignOptionSets);
            revitDB.FilteredElementCollector collectSets = new revitDB.FilteredElementCollector(document);
            return collectSets
                .WherePasses(catFilter)
                .Cast<revitDB.Element>()
                .ToList();
        }
    }
}
