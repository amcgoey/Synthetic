using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Revit.Elements;
using RevitServices.Transactions;
using dynaElem = Revit.Elements.Element;
using dynaWallType = Revit.Elements.WallType;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitCS = Autodesk.Revit.DB.CompoundStructure;

namespace Synthetic.Revit
{
    /// <summary>
    /// 
    /// </summary>
    public class WallType
    {
        internal WallType () {  }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="WallType">A Dynamo wrapped Revit.WallType</param>
        /// <returns></returns>
        public static revitDoc Document (dynaWallType WallType)
        {
            return WallType.InternalElement.Document;
        }

        /// <summary>
        /// Gets the compound structure from a wall type.
        /// </summary>
        /// <param name="WallType">A Dynamo wrapped Revit.WallType</param>
        /// <returns name="CompoundStructure">A Compound Structure</returns>
        public static CompoundStructure GetCompoundStructure (dynaWallType WallType)
        {
            revitDB.WallType unwrappedWall = (revitDB.WallType)WallType.InternalElement;
            revitDoc document = unwrappedWall.Document;
            revitCS compoundStructure = unwrappedWall.GetCompoundStructure();

            if (compoundStructure != null)
            {
                return new CompoundStructure(compoundStructure, document);
            }
            else { return null; }
        }

        /// <summary>
        /// Replaces a Wall Type's compound structure with the given one.  Please note that the compound structure's materials and the wall type must be in the same document or unexpected results may occur.
        /// </summary>
        /// <param name="WallType">The wall type to be modified.</param>
        /// <param name="compoundStructure">A compound structure</param>
        /// <returns name="WallType">The modified wall type.</returns>
        public static dynaWallType SetCompoundStructure (dynaWallType WallType, CompoundStructure compoundStructure)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SourceWallType"></param>
        /// <param name="DestinationWallType"></param>
        /// <returns name="DestinationWallType"></returns>
        public static dynaWallType TransferWallTypeProperties (dynaWallType SourceWallType, dynaWallType DestinationWallType)
        {
            Elements.TransferParameters(SourceWallType, DestinationWallType);
            WallType.SetCompoundStructure(DestinationWallType, WallType.GetCompoundStructure(SourceWallType));
            return DestinationWallType;
        }
    }
}
