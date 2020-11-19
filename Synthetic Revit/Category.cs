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
using RevitLinePatternElem = Autodesk.Revit.DB.LinePatternElement;
using RevitCollector = Autodesk.Revit.DB.FilteredElementCollector;


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
        /// Creates a Synthetic.Revit.Categoray wrapper given a Dynamo Category.  It searches for the Category name.
        /// </summary>
        /// <param name="category">A dynamo category wrapper.</param>
        /// <param name="document">The document to search for the category in.</param>
        public Category(DynCat category,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            RevitDB.Categories categories = document.Settings.Categories;

            RevitCat cat = categories.get_Item(category.Name);

            this.RevitCategory = cat;
        }

        /// <summary>
        /// Creates a Synthetic.Revit.Categoray wrapper by searching for the category name in the document.
        /// </summary>
        /// <param name="Name">The name of the category</param>
        /// <param name="document">The document to search for the category in.</param>
        public Category(string Name,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            RevitDB.Categories categories = document.Settings.Categories;

            RevitCat cat;
            if (categories.Contains(Name))
            {
                cat = categories.get_Item(Name);
                this.RevitCategory = cat;
            }            
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
                if (RevitCategory != null) { return RevitCategory.Name; }
                else { return null; }
            }
        }

        /// <summary>
        /// The line weight of the given graphics type.
        /// </summary>
        /// <param name="type">Autodesk.Revit.DB.GraphicsStyleType</param>
        /// <returns name="lineweight">An integer of the lineweight</returns>
        public int? LineWeightProjection (RevitDB.GraphicsStyleType type)
        {
            if (RevitCategory != null)
            {
                return RevitCategory.GetLineWeight(type);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// The Line Pattern Id of the given graphics type
        /// </summary>
        /// <param name="type">Autodesk.Revit.DB.GraphicsStyleType</param>
        /// <returns name="linePatternId">An integer of the Line Pattern ElementId</returns>
        public int? LinePatternId (RevitDB.GraphicsStyleType type)
        {
            if (RevitCategory != null)
            {
                return RevitCategory.GetLinePatternId(type).IntegerValue;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new subcategory in the file
        /// </summary>
        /// <param name="ParentCategory">A Synthetic wrapped Category object of a built-in category.</param>
        /// <param name="Name">Name of the new Subcategory</param>
        /// <param name="document">Document to make the subcategory in.</param>
        /// <returns name="Subcategory">Returns the newly made subcategory</returns>
        public static Category SubcategoryByName(string parentCategory,
            string Name,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            string transactionName = "Create Subcategory" + Name;

            Category category = new Category();

            RevitDB.Categories categories = document.Settings.Categories;
            RevitDB.Category pCat;
            if (categories.Contains(parentCategory))
            {
                pCat = categories.get_Item(parentCategory);

                RevitDB.CategoryNameMap categoryNameMap = pCat.SubCategories;

                // If the project does not contain the subcategory already, create it.
                if (!categoryNameMap.Contains(Name))
                {
                    if (document.IsModifiable)
                    {
                        TransactionManager.Instance.EnsureInTransaction(document);
                        category.RevitCategory = categories.NewSubcategory(pCat, Name);
                        document.Regenerate();
                        TransactionManager.Instance.TransactionTaskDone();
                    }
                    else
                    {
                        using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                        {
                            trans.Start(transactionName);
                            category.RevitCategory = categories.NewSubcategory(pCat, Name);
                            document.Regenerate();
                            trans.Commit();
                        }
                    }
                }
                // Else the project contains the category so just get the category.
                else
                {
                    category.RevitCategory = categoryNameMap.get_Item(Name);
                }
            }

            if (category.RevitCategory != null)
            {
                return category;
            }
            else { return null; }
        }

        /// <summary>
        /// Gets a subcategory from a parent category
        /// </summary>
        /// <param name="ParentCategory">A Synthetic wrapped Category object of a built-in category.</param>
        /// <param name="Name">Name of the new Subcategory</param>
        /// <param name="document">Document to make the subcategory in.</param>
        /// <returns name="Subcategory">Returns the newly made subcategory</returns>
        public static Category GetSubCategoryByName(string parentCategory,
            string Name,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            Category category = new Category();

            RevitDB.Categories categories = document.Settings.Categories;
            RevitDB.Category pCat;
            if (categories.Contains(parentCategory))
            {
                pCat = categories.get_Item(parentCategory);

                RevitDB.CategoryNameMap categoryNameMap = pCat.SubCategories;

                if (categoryNameMap.Contains(Name))
                {
                    category.RevitCategory = categoryNameMap.get_Item(Name);
                }
            }

            if (category.RevitCategory != null)
            {
                return category;
            }
            else { return null; }
        }

        /// <summary>
        /// Sets the line color of a category
        /// </summary>
        /// <param name="category">A Synthetic wrapped Category object</param>
        /// <param name="red">Red channel, 0 to 255</param>
        /// <param name="green">Green channel, 0 to 255</param>
        /// <param name="blue">Blue channel, 0 to 255</param>
        /// <param name="document">Autodesk.Revit.DB.Document to modify</param>
        /// <returns name="Category">The modified category</returns>
        public static Category SetLineColor(Category category,
            int red,
            int green,
            int blue,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            string transactionName = "Set " + category.Name + "Category's color";

            RevitDB.Color color = new RevitDB.Color((byte)red, (byte)green, (byte)blue);

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                category.RevitCategory.LineColor = color;
                document.Regenerate();
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    category.RevitCategory.LineColor = color;
                    document.Regenerate();
                    trans.Commit();
                }
            }

            return category;
        }

        /// <summary>
        /// Sets the line weight of a category
        /// </summary>
        /// <param name="category">A Synthetic wrapped Category object</param>
        /// <param name="lineWeight">An integer of the lineweight</param>
        /// <param name="type">The Revit GraphicsStyleType for either projection or cut</param>
        /// <param name="document">Autodesk.Revit.DB.Document to modify</param>
        /// <returns name="Category">The modified category</returns>
        public static Category SetLineWeight(Category category, int lineWeight,
            RevitDB.GraphicsStyleType type,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            string transactionName = "Set " + category.Name + "Category's " + type.ToString() + "Line Weight";
            
            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                category.RevitCategory.SetLineWeight(lineWeight, type);
                document.Regenerate();
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    category.RevitCategory.SetLineWeight(lineWeight, type);
                    document.Regenerate();
                    trans.Commit();
                }
            }
            
            return category;
        }

        /// <summary>
        /// Sets the line pattern id
        /// </summary>
        /// <param name="category">A Synthetic wrapped Category object</param>
        /// <param name="linePatternName">An integer representing the ElementId of the Line Pattern</param>
        /// <param name="type">The Revit GraphicsStyleType for either projection or cut</param>
        /// <param name="document">Autodesk.Revit.DB.Document to modify</param>
        /// <returns name="Category">The modified category</returns>
        public static Category SetLinePatternId(Category category,
            string linePatternName,
            RevitDB.GraphicsStyleType type,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            string transactionName = "Set " + category.Name + "Category's " + type.ToString() + "Line Weight";

            RevitElemId linePatternElemId = null;

            if (linePatternName == "Solid" || linePatternName == null)
            {
                linePatternElemId = RevitLinePatternElem.GetSolidPatternId();
            }
            else
            {
                RevitLinePatternElem linePatternElem;

                RevitCollector collector = new RevitCollector(document);

                RevitDB.ElementClassFilter elementClassFilter =
                    new RevitDB.ElementClassFilter(typeof(RevitDB.LinePatternElement));

                linePatternElem = (RevitLinePatternElem)collector.WhereElementIsNotElementType()
                    .WherePasses(elementClassFilter)
                    .FirstOrDefault(elem => elem.Name.Equals(linePatternName));

                if (linePatternElem != null)
                {
                    linePatternElemId = linePatternElem.Id;
                }
                else
                {
                    linePatternElem = LinePatternElement.ByName(linePatternName, document);
                    if (linePatternElem != null)
                    {
                        linePatternElemId = linePatternElem.Id;
                    }
                }
            }

            if (linePatternElemId == null)
            {
                linePatternElemId = RevitLinePatternElem.GetSolidPatternId();
            }

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                category.RevitCategory.SetLinePatternId(linePatternElemId, type);
                document.Regenerate();
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    category.RevitCategory.SetLinePatternId(linePatternElemId, type);
                    document.Regenerate();
                    trans.Commit();
                }
            }
            
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
