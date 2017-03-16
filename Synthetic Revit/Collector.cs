using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitFECollector = Autodesk.Revit.DB.FilteredElementCollector;

namespace Synthetic.Revit
{
    /// <summary>
    /// 
    /// </summary>
    public class Collector
    {
        internal revitFECollector internalCollector { get; private set; }

        internal Collector (revitDoc doc)
        {
            internalCollector = new revitFECollector(doc);
        }

        internal Collector (revitFECollector collector)
        {
            internalCollector = collector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="revitCollector"></param>
        /// <returns></returns>
        public static Collector Wrap (revitFECollector revitCollector)
        {
            return new Collector(revitCollector);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collector"></param>
        /// <returns></returns>
        public static revitFECollector Unwrap (Collector collector)
        {
            return collector.internalCollector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static Collector ByDocument ([DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            return new Collector(document);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static Collector ByFilter (revitDB.ElementFilter filter,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            Collector collector = new Collector(document);
            collector.internalCollector.WherePasses(filter);
            return collector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collector"></param>
        /// <returns></returns>
        public static IList<revitDB.Element> ToElements (Collector collector)
        {
            return collector.internalCollector.ToElements();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collector"></param>
        /// <returns></returns>
        public static IList<revitDB.ElementId> ToElementIds(Collector collector)
        {
            return (IList<revitDB.ElementId>)collector.internalCollector.ToElementIds();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collector"></param>
        /// <param name="filter"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static Collector WherePasses (Collector collector,
            revitDB.ElementFilter filter)
        {
            collector.internalCollector.WherePasses(filter);
            return collector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inverted"></param>
        /// <returns></returns>
        public static revitDB.ElementFilter FilterElementClass(Type type, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementClassFilter(type, inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static revitDB.ElementFilter FilterElementIsElementType ([DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementIsElementTypeFilter(inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static revitDB.ElementFilter FilterElementIsNotElementType()
        {
            return new revitDB.ElementIsElementTypeFilter(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
