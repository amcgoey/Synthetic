using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using Autodesk.DesignScript.Runtime;

/// Class Aliases
using revitDoc = Autodesk.Revit.DB.Document;
using dynamoDoc = Revit.Application.Document;

namespace Synthetic.Revit
{
    /// <summary>
    /// Document utilities for managing Autodesk.DB.Document's for use with the Revit API.
    /// </summary>
    public class Document
    {
        internal revitDoc _revitDoc { get; private set; }

        [IsVisibleInDynamoLibrary(false)]
        internal Document(revitDoc doc)
        {
            _revitDoc = doc;
        }

        /// <summary>
        /// Retreives the current document for use with the Revit API rather than in Dynamo.
        /// </summary>
        /// <returns name="Document">Returns a Autodesk.DB.Document object of the current document.</returns>
        public static revitDoc Current ()
        {
            return DocumentManager.Instance.CurrentDBDocument;
        }

        /// <summary>
        /// Retreives the current document for use with the Revit API rather than in Dynamo.
        /// </summary>
        /// <returns name="Documents">Returns a Autodesk.DB.Document object of the current document.</returns>
        [MultiReturn(new[] { "Documents", "Titles", "Is a family" })]
        public static IDictionary GetAllOpen ()
        {
            Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            DocumentSet docSet = app.Documents;

            List<revitDoc> docs = new List<revitDoc>();
            List<string> names = new List<string>();
            List<bool> family = new List<bool>();

            foreach (revitDoc doc in docSet)
            {
                docs.Add(doc);
                names.Add(doc.Title);
                family.Add(doc.IsFamilyDocument);
            }
            return new Dictionary<string, object>
            {
                {"Documents", docs},
                {"Titles", names},
                {"Is a family", family}
            };
        }

        /// <summary>
        /// Unwraps a dynamo Revit document.  Document needs to be open in the applications, otherwise return null.
        /// </summary>
        /// <param name="document">A Dynamo wrapped Revit Document</param>
        /// <returns name="Revit Document">A unwrapped Revit document.  Document needs to be open in the application, otherwise return null.</returns>
        public static revitDoc Unwrap (dynamoDoc document)
        {
            string filePath = document.FilePath;

            Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            DocumentSet docSet = app.Documents;

            List<revitDoc> docs = new List<revitDoc>();
            foreach (revitDoc doc in docSet)
            {
                if (filePath == doc.PathName)
                {
                    return doc;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves the title of a Revit document.  Only works with Autodesk.DB.Document objects, not Dynamo based Revit.Document objects.
        /// </summary>
        /// <param name="doc">A Autodesk.DB.Document.  This will not work with a Dynamo based Revit.Document object. </param>
        /// <returns name="Title">The title of the document</returns>
        public static string Title (revitDoc doc)
        {
            return doc.Title;
        }

        /// <summary>
        /// Tests whether a document is a family or not.
        /// </summary>
        /// <param name="doc">A Autodesk.DB.Document.  This will not work with a Dynamo based Revit.Document object. </param>
        /// <returns name="Is a family">True is the document is a family, false if it not.</returns>
        public static bool IsFamilyDocument (revitDoc doc)
        {
            return doc.IsFamilyDocument;
        }

        /// <summary>
        /// Retrives the file path of the document
        /// </summary>
        /// <param name="document"></param>
        /// <returns name="File Path">Returns a string of the file path.</returns>
        public static string FilePath (revitDoc document)
        {
            return document.PathName ?? string.Empty;
        }

        /// <summary>
        /// Enables worksharing in the document.
        /// </summary>
        /// <param name="document">A Revit document.</param>
        /// <returns name="document">The Revit document.  Returns null if worksharing cannot be enabled.</returns>
        public static revitDoc EnableWorksharing (revitDoc document)
        {
            if (document.CanEnableWorksharing())
            {
                document.EnableWorksharing("Shared Levels and Grids", "Workset1");
                return document;
            }
            else { return null; }
        }

        /// <summary>
        /// Checks if worksharing is enabled in a document.
        /// </summary>
        /// <param name="document">A Revit document.</param>
        /// <returns name="bool">True is workshared, false if not.</returns>
        public static bool IsWorkshared (revitDoc document)
        {
            return document.IsWorkshared;
        }

        /// <summary>
        /// Opens a document from disk.  The document will not be visible to the user.
        /// </summary>
        /// <param name="modelPath">Path to the document.</param>
        /// <param name="reset">Resets the node to reopen the document.</param>
        /// <returns name="document">The opened revit document.</returns>
        public static revitDoc Open (string modelPath, bool reset)
        {
            Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;

            revitDoc doc = app.OpenDocumentFile(modelPath);
            
            return doc;
        }

        /// <summary>
        /// Closes a document if it isn't the active document.  By default, the document will NOT be saved when closed.
        /// </summary>
        /// <param name="document">A revit document.  Cannot be the active document.</param>
        /// <param name="save">If true, the document will be saved.  If false, the document will not be saved.</param>
        /// <returns name="bool">Returns true is the document was closed, false otherwise.  Returns false if saving was requested but failed</returns>
        public static bool Close (revitDoc document, [DefaultArgument("false")] bool save)
        {
            if (DocumentManager.Instance.ActiveDocumentHashCode != document.GetHashCode())
            {
                document.Close(save);
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Get all the linked Revit documents in a document.
        /// </summary>
        /// <param name="document">A revit document.</param>
        /// <returns name="Documents">Revit documents.</returns>
        /// <returns name="Titles">The file paths of the linked documents.</returns>
        [MultiReturn(new[] { "Documents", "Titles" })]
        public static IDictionary GetLinkedRevit (revitDoc document)
        {
            FilteredElementCollector links = new FilteredElementCollector(document);
            links.OfClass(typeof(RevitLinkInstance));

            List<revitDoc> linkDocs = new List<revitDoc>();
            List<string> linkNames = new List<string>();


            foreach (RevitLinkInstance link in links)
            {
                revitDoc linkDoc = link.GetLinkDocument();

                linkDocs.Add(link.GetLinkDocument());
                document.GetElement(link.GetTypeId());
                linkNames.Add(linkDoc.Title);
            }
            return new Dictionary<string, object>
            {
                {"Documents", linkDocs},
                {"Titles", linkNames}
            };
        }
    }
}
