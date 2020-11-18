using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using RevitServices.Transactions;

using Revit.Elements;
using DynElem = Revit.Elements.Element;
using DynParam = Revit.Elements.Parameter;
using DynCat = Revit.Elements.Category;

using RevitDB = Autodesk.Revit.DB;
using RevitElem = Autodesk.Revit.DB.Element;
using RevitElemId = Autodesk.Revit.DB.ElementId;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitCat = Autodesk.Revit.DB.Category;


namespace Synthetic.Revit
{
    /// <summary>
    /// Wrapper for Revit Categories.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// The native Revit Category object
        /// </summary>
        public RevitCat RevitCategory { get; set; }

        /// <summary>
        /// Dummy constructor for the class.  Not used.
        /// </summary>
       public Category(RevitCat category)
        {
            this.RevitCategory = category;
        }

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public Category() { }

        /// <summary>
        /// Name of the category as a string
        /// </summary>
        public string Name
        {
            get
            {
                return RevitCategory.Name;
            }
        }

        /// <summary>
        /// The line weight of the given graphics type.
        /// </summary>
        /// <param name="type">Autodesk.Revit.DB.GraphicsStyleType</param>
        /// <returns name="lineweight">An integer of the lineweight</returns>
        public int? LineWeightProjection (RevitDB.GraphicsStyleType type)
        {
            return RevitCategory.GetLineWeight(type);
        }

        /// <summary>
        /// The Line Pattern Id of the given graphics type
        /// </summary>
        /// <param name="type">Autodesk.Revit.DB.GraphicsStyleType</param>
        /// <returns name="linePatternId">An integer of the Line Pattern ElementId</returns>
        public int LinePatternId (RevitDB.GraphicsStyleType type)
        {
                return RevitCategory.GetLinePatternId(type).IntegerValue;
        }

        /// <summary>
        /// Creates a new subcategory in the file
        /// </summary>
        /// <param name="ParentCategory">A Synthetic wrapped Category object of a built-in category.</param>
        /// <param name="Name">Name of the new Subcategory</param>
        /// <param name="document">Document to make the subcategory in.</param>
        /// <returns name="Subcategory">Returns the newly made subcategory</returns>
        public static Category SubcategoryByName(Synthetic.Revit.Category ParentCategory, string Name, [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            string transactionName = "Create Subcategory" + Name;

            RevitDB.Categories categories = document.Settings.Categories;

            Category category = new Category();

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                category.RevitCategory = categories.NewSubcategory(ParentCategory.RevitCategory, Name);
                document.Regenerate();
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    category.RevitCategory = categories.NewSubcategory(ParentCategory.RevitCategory, Name);
                    document.Regenerate();
                    trans.Commit();
                }
            }

            if (category.RevitCategory != null)
            {
                return category;
            }
            else { return null; }
        }

        /// <summary>
        /// Sets the line weight of a category
        /// </summary>
        /// <param name="category">A Synthetic wrapped Category object</param>
        /// <param name="lineWeight">An integer of the lineweight</param>
        /// <param name="type">The Revit GraphicsStyleType for either projection or cut</param>
        /// <returns name="Category">The modified category</returns>
        public static Category SetLineWeight(Category category, int lineWeight, RevitDB.GraphicsStyleType type)
        {
            category.RevitCategory.SetLineWeight(lineWeight, type);
            return category;
        }

        /// <summary>
        /// Sets the line pattern id
        /// </summary>
        /// <param name="category">A Synthetic wrapped Category object</param>
        /// <param name="linePatternId">An integer representing the ElementId of the Line Pattern</param>
        /// <param name="type">The Revit GraphicsStyleType for either projection or cut</param>
        /// <returns name="Category">The modified category</returns>
        public static Category SetLinePatternId(Category category, int linePatternId, RevitDB.GraphicsStyleType type)
        {
            category.RevitCategory.SetLinePatternId(new RevitElemId(linePatternId), type);
            return category;
        }

        /// <summary>
        /// Creates a Revit Projection GraphicsStyelType
        /// </summary>
        /// <returns name="ProjectionType">Autodesk.Revit.DB.GraphicsStyleType.Projection</returns>
        public static RevitDB.GraphicsStyleType ProjectionType()
        {
            return RevitDB.GraphicsStyleType.Projection;
        }

        /// <summary>
        /// Creates a Revit Cut GraphicsStyelType
        /// </summary>
        /// <returns name="CutType">Autodesk.Revit.DB.GraphicsStyleType.Cut</returns>
        public static RevitDB.GraphicsStyleType CutType()
        {
            return RevitDB.GraphicsStyleType.Cut;
        }
    }
}
