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
        /// Get the document that the wall type belongs too.
        /// </summary>
        /// <param name="WallType">A Dynamo wrapped Revit.WallType</param>
        /// <returns name="Document">The Autodesk.Revit.DB.Document that the wall type belongs too.</returns>
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
            revitDoc document = compoundStructure.internalDocument;

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                revitWallType.SetCompoundStructure(compoundStructure.internalCompoundStructure);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start("Apply Structure to Wall Type");
                    revitWallType.SetCompoundStructure(compoundStructure.internalCompoundStructure);
                    trans.Commit();
                }
            }
            return WallType;
        }

        /// <summary>
        /// Overwrites the parameters and compound structure of the destintation wall type with the source wall type.
        /// </summary>
        /// <param name="wallType">A dynamo wrappped WallType to overwrite</param>
        /// <param name="SourceWallType">A dynamo wrapped WallType to use as the source</param>
        /// <returns name="DestinationWallType">The destinatio WallType</returns>
        public static dynaWallType TransferWallTypeProperties (dynaWallType wallType, dynaWallType SourceWallType)
        {
            Elements.TransferParameters(SourceWallType, wallType);
            WallType.SetCompoundStructure(wallType, WallType.GetCompoundStructure(SourceWallType));
            return wallType;
        }
    }
}
