using System;
using System.Collections;
using System.Collections.Generic;

using revitViews = Autodesk.Revit.DB.View;
using revitDoc = Autodesk.Revit.DB.Document;
using revitElemId = Autodesk.Revit.DB.ElementId;

using RevitServices.Transactions;
using dynaView = Revit.Elements.Views.View;

namespace Synthetic.Revit
{
    /// <summary>
    /// Revit API extensions to the Dynamo View wrapper
    /// </summary>
    public class View
    {
        internal View () { }

        /// <summary>
        /// Applies source view's parameters to the view.  Applies parameters based on GetTemplateParameterIds.
        /// </summary>
        /// <param name="View">View to modify based on source view's parameters</param>
        /// <param name="SourceView">Source view to take parameters from</param>
        /// <returns name="View">The modified view</returns>
        public static dynaView ApplyViewParameters (dynaView View, dynaView SourceView)
        {
            revitViews dView = (revitViews)View.InternalElement;
            revitViews sview = (revitViews)SourceView.InternalElement;

            revitDoc document = dView.Document;
            
            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                dView.ApplyViewTemplateParameters(sview);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start("Apply View Parameters");
                    dView.ApplyViewTemplateParameters(sview);
                    trans.Commit();
                }
            }

            return View;
        }

        /// <summary>
        /// Gets the ElementIds of the view's parameters that are controlled by a view template.
        /// </summary>
        /// <param name="View">A dynamo wrapped View</param>
        /// <returns name="ElementIds">A list of ElementIds that are controlled by a view template.</returns>
        public static IList<revitElemId> GetTemplateParameterIds(dynaView View)
        {
            revitViews rView = (revitViews)View.InternalElement;
            return rView.GetTemplateParameterIds();
        }

        /// <summary>
        /// Gets the ElementIds of the view's parameters that are NOT controlled by a view template.
        /// </summary>
        /// <param name="View">A dynamo wrapped View</param>
        /// <returns name="ElementIds">A list of ElementIds that are NOT controlled by a view template.</returns>
        public static IList<revitElemId> GetNonControlledTemplateParameterIds(dynaView View)
        {
            revitViews rView = (revitViews)View.InternalElement;
            return (IList<revitElemId>)rView.GetNonControlledTemplateParameterIds();
        }

        /// <summary>
        /// Sets the view's parameters not controlled by templates.
        /// </summary>
        /// <param name="View">A dynamo wrapped View</param>
        /// <param name="ElementIds">A list of ElementIds</param>
        public static void SetNonControlledTemplateParameterIds(dynaView View, IList<revitElemId> ElementIds)
        {
            revitViews rView = (revitViews)View.InternalElement;
            rView.SetNonControlledTemplateParameterIds(ElementIds);
        }
    }
}
