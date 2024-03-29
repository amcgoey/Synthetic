﻿using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using RevitServices.Persistence;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

using RevitDB = Autodesk.Revit.DB;
using RevitServices.Transactions;
using revitExcept = Autodesk.Revit.Exceptions;

/// Class Aliases
using RevitDoc = Autodesk.Revit.DB.Document;
using DynamoDoc = Revit.Application.Document;
using tData = Autodesk.Revit.DB.TransmissionData;

namespace Synthetic.Revit
{
    /// <summary>
    /// Document utilities for managing Autodesk.DB.Document's for use with the Revit API.
    /// </summary>
    [IsDesignScriptCompatible]
    public class Document
    {
        internal RevitDoc _revitDoc { get; private set; }

        [IsVisibleInDynamoLibrary(false)]
        internal Document(RevitDoc doc)
        {
            _revitDoc = doc;
        }

        /// <summary>
        /// Retreives the current document for use with the Revit API rather than in Dynamo.
        /// </summary>
        /// <returns name="Document">Returns a Autodesk.DB.Document object of the current document.</returns>
        [IsDesignScriptCompatible]
        public static RevitDoc Current()
        {
            return DocumentManager.Instance.CurrentDBDocument;
        }

        /// <summary>
        /// Retreives the current document for use with the Revit API rather than in Dynamo.
        /// </summary>
        /// <returns name="Documents">Returns a Autodesk.DB.Document object of the current document.</returns>
        [IsDesignScriptCompatible]
        [MultiReturn(new[] { "Documents", "Titles", "Is a family" })]
        public static IDictionary GetAllOpen()
        {
            Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            RevitDB.DocumentSet docSet = app.Documents;

            List<RevitDoc> docs = new List<RevitDoc>();
            List<string> names = new List<string>();
            List<bool> family = new List<bool>();

            foreach (RevitDoc doc in docSet)
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
        /// If the document is currently open, returns the doucment object given the file path. 
        /// </summary> 
        /// <param name="filePath"></param> 
        /// <returns></returns> 
        [IsDesignScriptCompatible]
        public static RevitDoc DocumentFromPath(string filePath)
        {
            Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            RevitDB.DocumentSet docSet = app.Documents;

            foreach (RevitDoc doc in docSet)
            {
                if (doc.PathName == filePath)
                {
                    return doc;
                }
            }
            return null;
        }

        /// <summary>
        /// Unwraps a dynamo Revit document.  Document needs to be open in the applications, otherwise return null.
        /// </summary>
        /// <param name="document">A Dynamo wrapped Revit Document</param>
        /// <returns name="Revit Document">A unwrapped Revit document.  Document needs to be open in the application, otherwise return null.</returns>
        [IsDesignScriptCompatible]
        public static RevitDoc UnwrapDocument(DynamoDoc document)
        {
            string filePath = document.FilePath;

            Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            RevitDB.DocumentSet docSet = app.Documents;

            List<RevitDoc> docs = new List<RevitDoc>();
            foreach (RevitDoc doc in docSet)
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
        [IsDesignScriptCompatible]
        public static string Title(RevitDoc doc)
        {
            return doc.Title;
        }

        /// <summary>
        /// Tests whether a document is a family or not.
        /// </summary>
        /// <param name="doc">A Autodesk.DB.Document.  This will not work with a Dynamo based Revit.Document object. </param>
        /// <returns name="Is a family">True is the document is a family, false if it not.</returns>
        [IsDesignScriptCompatible]
        public static bool IsFamilyDocument(RevitDoc doc)
        {
            return doc.IsFamilyDocument;
        }

        /// <summary>
        /// Retrives the file path of the document.  If CentralFilePath is true, then the method returns the patht to the central file, if false, then it returns the path to the local file.  Detached files will return an empty string.
        /// </summary>
        /// <param name="document">An Autodsk.Revit.DB.Document element.</param>
        /// <returns name="File Path">Returns a string of the file path.</returns>
        [IsDesignScriptCompatible]
        public static string FilePath(RevitDoc document, bool CentralFilePath = true)
        {
            string path = null;

            if(CentralFilePath && document.IsWorkshared)
            {
                RevitDB.ModelPath modelPath = document.GetWorksharingCentralModelPath();
                path = RevitDB.ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
            }
            else
            {
                path = document.PathName ?? string.Empty;
            }

            return path;
        }

        /// <summary>
        /// Enables worksharing in the document.
        /// </summary>
        /// <param name="document">A Revit document.</param>
        /// <returns name="document">The Revit document.  Returns null if worksharing cannot be enabled.</returns>
        [IsDesignScriptCompatible]
        public static RevitDoc EnableWorksharing(RevitDoc document)
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
        [IsDesignScriptCompatible]
        public static bool IsWorkshared(RevitDoc document)
        {
            return document.IsWorkshared;
        }

        /// <summary>
        /// Opens a document from disk.  The document will not be visible to the user.
        /// </summary>
        /// <param name="modelPath">Path to the document.</param>
        /// <param name="reset">Resets the node to reopen the document.</param>
        /// <returns name="document">The opened revit document.</returns>
        [IsDesignScriptCompatible]
        public static RevitDoc Open(string modelPath, [DefaultArgument("true")] bool reset)
        {
            Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;

            RevitDoc doc = app.OpenDocumentFile(modelPath);

            return doc;
        }

        /// <summary>
        /// Opens a document from disk.  The document will not be visible to the user.  This version allows for opening or closing of worksets while opening the file.
        /// </summary>
        /// <param name="modelPath">Path to the document.</param>
        /// <param name="worksetConfiguration">An object that describes what worksets to open when the project is open.</param>
        /// <param name="audit">Set true to audit the file on opening</param>
        /// <param name="reset">Resets the node to reopen the document.</param>
        /// <returns name="document">The opened revit document.</returns>
        [IsDesignScriptCompatible]
        public static RevitDoc OpenWithOptions(string modelPath,
            [DefaultArgument("Synthetic.Revit.WorksetConfigurationOpenAll()")] RevitDB.WorksetConfiguration worksetConfiguration,
            [DefaultArgument("false")] bool audit,
            [DefaultArgument("true")] bool reset)
        {
            Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            RevitDoc doc = null;

            RevitDB.ModelPath path = RevitDB.ModelPathUtils.ConvertUserVisiblePathToModelPath(modelPath);

            RevitDB.OpenOptions openOptions = new RevitDB.OpenOptions();
            openOptions.Audit = audit;
            openOptions.SetOpenWorksetsConfiguration(worksetConfiguration);

            doc = app.OpenDocumentFile(path, openOptions);

            return doc;
        }

        public static RevitDoc OpenAndActivate(string modelPath, [DefaultArgument("true")] bool reset)
        {
            Autodesk.Revit.UI.UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;

            RevitDoc doc = uiapp.OpenAndActivateDocument(modelPath).Document;

            return doc;
        }

        /// <summary>
        /// Closes a document if it isn't the active document.  By default, the document will NOT be saved when closed.
        /// </summary>
        /// <param name="document">A revit document.  Cannot be the active document.</param>
        /// <param name="save">If true, the document will be saved.  If false, the document will not be saved.</param>
        /// <returns name="bool">Returns true is the document was closed, false otherwise.  Returns false if saving was requested but failed</returns>
        [IsDesignScriptCompatible]
        public static bool Close(RevitDoc document, [DefaultArgument("false")] bool save)
        {
            if (DocumentManager.Instance.ActiveDocumentHashCode != document.GetHashCode())
            {
                bool results = document.Close(save);
                return results;
            }
            else { return false; }
        }

        /// <summary>
        /// Opens a document with all user worksets closed, , upgrading the document in the process, then closes and saves the file.  With all worksets closed, none of the links will open, improving the speed of the process.
        /// </summary>
        /// <param name="modelPath">Path to the document.</param>
        /// <param name="reset">Resets the node to reopen the document.</param>
        /// <returns name="bool">Returns true is the document was closed, false otherwise.  Returns false if saving was requested but failed</returns>
        [IsDesignScriptCompatible]
        public static bool Upgrade(string modelPath, [DefaultArgument("true")] bool reset)
        {
            RevitDoc doc = OpenWithOptions(modelPath, WorksetConfigurationCloseAll(), false, true);
            bool results = doc.Close(true);
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelPath"></param>
        /// <param name="reset"></param>
        /// <returns name="bool">Returns true if document successfully closed.</returns>
        public static bool UpgradeAuditCompact(string modelPath, [DefaultArgument("true")] bool reset)
        {
            RevitDoc doc = OpenWithOptions(modelPath, WorksetConfigurationCloseAll(), true, true);
            RevitDB.SaveAsOptions saveOptions = new RevitDB.SaveAsOptions();
            RevitDB.WorksharingSaveAsOptions worksharingSaveAsOptions = saveOptions.GetWorksharingOptions();

            worksharingSaveAsOptions.ClearTransmitted = true;
            worksharingSaveAsOptions.SaveAsCentral = true;

            saveOptions.SetWorksharingOptions(worksharingSaveAsOptions);

            doc.SaveAs(modelPath, saveOptions);

            bool results = doc.Close(true);
            return results;
        }

        /// <summary>
        /// Synchronize a document with central
        /// </summary>
        /// <param name="document">A Autodesk.Revit.DB.Document object</param>
        /// <param name="syncOptions">A Autodesk.Revit.DB.SynchronizeWithCentralOptions object.  Creates a default object by default.</param>
        /// <param name="comment">Syncrhonization comments.</param>
        /// <param name="compact">If true, compact the model while saving.</param>
        /// <param name="execute">If True synchoronize with central.</param>
        [IsDesignScriptCompatible]
        public static void SynchronizeWithCentral(
            RevitDoc document,
            [DefaultArgument("Synthetic.Revit.Document.SynchronizeWithCentralOptions()")] RevitDB.SynchronizeWithCentralOptions syncOptions,
            string comment,
            [DefaultArgument("false")] bool compact,
            bool execute
            )
        {
            if (execute)
            {
                RevitDB.TransactWithCentralOptions transOptions = new RevitDB.TransactWithCentralOptions();
                syncOptions.Compact = compact;
                syncOptions.Comment = comment;

                document.SynchronizeWithCentral(transOptions, syncOptions);
            }
        }

        /// <summary>
        /// Get all the linked Revit documents in a document.
        /// </summary>
        /// <param name="Document">A revit document.</param>
        /// <returns name="Documents">Revit documents.</returns>
        /// <returns name="Titles">The file paths of the linked documents.</returns>
        [MultiReturn(new[] { "Documents", "Titles" })]
        [IsDesignScriptCompatible]
        public static IDictionary GetLinkedRevit([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            RevitDB.FilteredElementCollector links = new RevitDB.FilteredElementCollector(Document);
            links.OfClass(typeof(RevitDB.RevitLinkInstance));

            List<RevitDoc> linkDocs = new List<RevitDoc>();
            List<string> linkNames = new List<string>();


            foreach (RevitDB.RevitLinkInstance link in links)
            {
                RevitDoc linkDoc = link.GetLinkDocument();

                linkDocs.Add(link.GetLinkDocument());
                //document.GetElement(link.GetTypeId());
                linkNames.Add(linkDoc.Title);
            }
            return new Dictionary<string, object>
            {
                {"Documents", linkDocs},
                {"Titles", linkNames}
            };
        }

        /// <summary>
        /// Get all the linked Revit type elements in a document.
        /// </summary>
        /// <param name="Document">A revit document.</param>
        /// <returns name="Links">Revit RevitLinkType</returns>
        /// <returns name="Titles">The file paths of the linked documents.</returns>
        [MultiReturn(new[] { "Links", "Titles" })]
        [IsDesignScriptCompatible]
        public static IDictionary GetRevitLinkTypes([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            RevitDB.FilteredElementCollector links = new RevitDB.FilteredElementCollector(Document);
            links.OfClass(typeof(RevitDB.RevitLinkType));

            List<RevitDB.RevitLinkType> linkDocs = new List<RevitDB.RevitLinkType>();
            List<string> linkNames = new List<string>();


            foreach (RevitDB.RevitLinkType link in links)
            {
                if (link !=null && link.IsExternalFileReference())
                {
                    linkDocs.Add(link);
                    linkNames.Add(link.Name);
                }
            }
            return new Dictionary<string, object>
            {
                {"Links", linkDocs},
                {"Titles", linkNames}
            };
        }

        /// <summary>
        /// Get all the linked CAD file in a document.
        /// </summary>
        /// <param name="Document">A revit document.</param>
        /// <returns name="CAD">Linked CAD files.</returns>
        /// <returns name="Titles">The file paths of the linked CAD files.</returns>
        [MultiReturn(new[] { "CAD", "Titles" })]
        [IsDesignScriptCompatible]
        public static IDictionary GetCADLinkTypes([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            RevitDB.FilteredElementCollector links = new RevitDB.FilteredElementCollector(Document);
            links.OfClass(typeof(RevitDB.CADLinkType));

            List<RevitDB.CADLinkType> linkDocs = new List<RevitDB.CADLinkType>();
            List<string> linkNames = new List<string>();


            foreach (RevitDB.CADLinkType link in links)
            {
                if (link != null && link.IsExternalFileReference())
                {
                    linkDocs.Add(link);
                    linkNames.Add(link.Name);
                }
            }
            return new Dictionary<string, object>
            {
                {"CAD", linkDocs},
                {"Titles", linkNames}
            };
        }

        /// <summary>
        /// Given a CADLinkType element, returns it's path.
        /// </summary>
        /// <param name="RevitLink">A CADLinkType element</param>
        /// <returns name="Path">The path of the CADLinkType element</returns>
        public static string GetPathRevitLinkType(RevitDB.RevitLinkType RevitLink)
        {
            string pathString = null;

            if (RevitLink != null)
            {
                if (RevitLink.IsExternalFileReference())
                {
                    RevitDB.ExternalFileReference exFiRef = RevitLink.GetExternalFileReference();
                    RevitDB.ModelPath path = exFiRef.GetPath();

                    if (path.ServerPath == true)
                    {
                        pathString = path.CentralServerPath;
                    }
                    else
                    {
                        pathString = RevitDB.ModelPathUtils.ConvertModelPathToUserVisiblePath(exFiRef.GetAbsolutePath());
                    }
                }
            }
            return pathString;
        }

        /// <summary>
        /// Given a CADLinkType element, returns it's path.
        /// </summary>
        /// <param name="CADLink">A CADLinkType element</param>
        /// <returns name="Path">The path of the CADLinkType element</returns>
        public static string GetPathCADLinkTypes(RevitDB.CADLinkType CADLink)
        {
            string pathString = null;

            if (CADLink != null)
            {
                if (CADLink.IsExternalFileReference())
                {
                    RevitDB.ExternalFileReference exFiRef = CADLink.GetExternalFileReference();
                    RevitDB.ModelPath path = exFiRef.GetPath();

                    if (path.ServerPath == true)
                    {
                        pathString = path.CentralServerPath;
                    }
                    else
                    {
                        pathString = RevitDB.ModelPathUtils.ConvertModelPathToUserVisiblePath(exFiRef.GetAbsolutePath());
                    }
                }
            }
            return pathString;
        }

        /// <summary>
        /// Reloads the CAD Link from a new path.
        /// </summary>
        /// <param name="RevitLink">CADLinkType element</param>
        /// <param name="path">new path to load link from.</param>
        /// <returns name="RevitLink">The reloaded CADLinkType</returns>
        public static RevitDB.RevitLinkType LoadFromRevitLinkType(RevitDB.RevitLinkType RevitLink, string path)
        {
            RevitDoc Document = RevitLink.Document;

            if (RevitLink != null
                && path != null
                && RevitDB.RevitLinkType.IsLoaded(Document, RevitLink.Id))
            {
                RevitDB.ModelPath modelPath = RevitDB.ModelPathUtils.ConvertUserVisiblePathToModelPath(path);

                string transactionName = "Reload Revit Link " + Document.Title;
                if (Document.IsModifiable)
                {
                    TransactionManager.Instance.EnsureInTransaction(Document);
                    RevitLink.LoadFrom(modelPath, new RevitDB.WorksetConfiguration());
                    TransactionManager.Instance.TransactionTaskDone();
                    //TransactionManager.Instance.ForceCloseTransaction();
                }
                else
                {
                    using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(Document))
                    {
                        trans.Start(transactionName);
                        RevitLink.LoadFrom(modelPath, new RevitDB.WorksetConfiguration());
                        trans.Commit();
                    }
                }
                return RevitLink;
            }
            else { return null; }
        }

        /// <summary>
        /// Reloads the CAD Link from a new path.
        /// </summary>
        /// <param name="CADLink">CADLinkType element</param>
        /// <param name="path">new path to load link from.</param>
        /// <returns name="CADLink">The reloaded CADLinkType</returns>
        public static RevitDB.CADLinkType LoadFromCADLinkType(RevitDB.CADLinkType CADLink, string path)
        {
            RevitDoc Document = CADLink.Document;

            if (CADLink != null
                && path != null
                && CADLink.IsExternalFileReference()
                && CADLink.GetExternalFileReference().GetLinkedFileStatus() != RevitDB.LinkedFileStatus.Unloaded)
            {
                string transactionName = "Reload Revit Link " + Document.Title;
                if (Document.IsModifiable)
                {
                    TransactionManager.Instance.EnsureInTransaction(Document);
                    CADLink.LoadFrom(path);
                    TransactionManager.Instance.TransactionTaskDone();
                    //TransactionManager.Instance.ForceCloseTransaction();
                }
                else
                {
                    using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(Document))
                    {
                        trans.Start(transactionName);
                        CADLink.LoadFrom(path);
                        trans.Commit();
                    }
                }
                return CADLink;
            }
            else { return null; }
        }

        /// <summary>
        /// Gets the ElementId of the current starting view for the document.  If InvalidElementId is returned, then no view is specified and the last view opened will be used.
        /// </summary>
        /// <param name="document">A Autodesk.Revit.DB.Document object</param>
        /// <returns name="ViewId">The ElementId of the view or sheet</returns>
        [IsDesignScriptCompatible]
        public static RevitDB.ElementId GetStartViewId(RevitDoc document)
        {
            RevitDB.StartingViewSettings startViewSettings = RevitDB.StartingViewSettings.GetStartingViewSettings(document);
            return startViewSettings.ViewId;
        }

        /// <summary>
        /// Sets the starting view for the document using the view's ElementId.  If InvalidElementId is set, then no view is specified and the last view opened will be used.
        /// </summary>
        /// <param name="document">A Autodesk.Revit.DB.Document object</param>
        /// <param name="viewId">The ElementId of the view or sheet</param>
        /// <returns name="ViewId">The ElementId of the view currently set as the starting view.  If null, then the view could not be set.</returns>
        [IsDesignScriptCompatible]
        public static RevitDB.ElementId SetStartViewId(RevitDoc document, RevitDB.ElementId viewId)
        {
            RevitDB.StartingViewSettings startViewSettings = RevitDB.StartingViewSettings.GetStartingViewSettings(document);
            if (startViewSettings.IsAcceptableStartingView(viewId))
            {
                return startViewSettings.ViewId = viewId;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a WorksetConfiguration that opens all worksets by default.
        /// </summary>
        /// <returns name="WorksetConfiguration">A WorksetConfiguration that opens all worksets</returns>
        [IsDesignScriptCompatible]
        public static RevitDB.WorksetConfiguration WorksetConfigurationOpenAll()
        {
            return new RevitDB.WorksetConfiguration();
        }

        /// <summary>
        /// Returns a WorksetConfiguration that closes all worksets by default.
        /// </summary>
        /// <returns name="WorksetConfiguration">A WorksetConfiguration that closes all worksets</returns>
        [IsDesignScriptCompatible]
        public static RevitDB.WorksetConfiguration WorksetConfigurationCloseAll()
        {
            return new RevitDB.WorksetConfiguration(RevitDB.WorksetConfigurationOption.CloseAllWorksets);
        }

        /// <summary>
        /// Creates a default SynchronizeWithCentralOptions object.
        /// </summary>
        /// <returns name="SyncOptions">A default SynchronizeWithCentralOptions object</returns>
        [IsDesignScriptCompatible]
        public static RevitDB.SynchronizeWithCentralOptions SynchronizeWithCentralOptions()
        {
            return new RevitDB.SynchronizeWithCentralOptions();
        }

        /// <summary>
        /// Opens all the Families in a project to test if any have been corrupted.
        /// </summary>
        /// <param name="document">A Revit Document</param>
        /// <param name="execute">If true, run the node, otherwise the node will not run.</param>
        /// <returns name="results">Returns a text string of the results of opening each family.</returns>
        [MultiReturn(new[] { "Results", "Errors" })]
        public static Dictionary<string, object> AuditProjectFamilies(RevitDoc document, bool execute)
        {
            List<List<string>> results = new List<List<string>>();
            List<List<string>> errors = new List<List<string>>();

            if (execute)
            {
                RevitDB.FilteredElementCollector familyCollector = new RevitDB.FilteredElementCollector(document);
                familyCollector.OfClass(typeof(RevitDB.Family)).ToElements();

                foreach (RevitDB.Family family in familyCollector)
                {
                    List<string> familyResult = new List<string>();
                    familyResult.Add(family.Name);
                    familyResult.Add(family.Id.ToString());
                    familyResult.Add(family.UniqueId);

                    if (family.IsEditable)
                    {
                        try
                        {
                            var familyDoc = document.EditFamily(family);
                            familyResult.Add("Opened successfully");

                            IList<RevitDB.FailureMessage> warnings = familyDoc.GetWarnings();

                            if (warnings.Count > 0)
                            {
                                StringBuilder warningString = new StringBuilder();

                                foreach (RevitDB.FailureMessage warning in warnings)
                                {
                                    warningString.AppendLine(warning.GetDescriptionText());
                                }
                                familyResult.Add(warningString.ToString());
                            }

                            familyDoc.Close(false);
                        }
                        catch (Exception ex)
                        {
                            familyResult.Add("Error: " + ex.Message);
                            errors.Add(familyResult);
                        }
                        results.Add(familyResult);

                    }
                }
            }
            return new Dictionary<string, object>
            {
                {"Results", results},
                {"Errors", errors}
            };
        }
    }
}
