using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using Autodesk.DesignScript.Runtime;
using Revit.Elements;
using Dynamo.Graph.Nodes;

//Aliases for Revit Classes
using revitDoc = Autodesk.Revit.DB.Document;
using revitWorkset = Autodesk.Revit.DB.Workset;
using revitWorksetId = Autodesk.Revit.DB.WorksetId;

//Aliases for Dynamo Classes
using dynamoElement = Revit.Elements.Element;

namespace Synthetic.Revit
{
    /// <summary>
    /// Workset Class
    /// </summary>
    public class Workset
    {
        private revitWorkset _workset;
        private revitWorksetId _id;
        private revitDoc _doc;
        private string _name;

        [IsVisibleInDynamoLibrary(false)]
        internal Workset(revitDoc doc, revitWorkset workset)
        {
            _workset = workset;
            _id = workset.Id;
            _doc = doc;
            _name = workset.Name;
        }

        internal revitWorkset internalWorkset
        {
            get { return _workset; }
            private set { }
        }

        internal revitWorksetId internalId
        {
            get { return _id; }
            private set { }
        }

        internal revitDoc internalDocument
        {
            get { return _doc; }
            private set { }
        }

        internal string internalName
        {
            get { return _name; }
            private set { }
        }

        /// <summary>
        /// Creates a workset in current Revit document.  The default visibility of the workset can be set.  If an alias is provided that exists in the current document, the alias workset will be renamed rather than a new workset made.
        /// </summary>
        /// <param name="name">The name of the workset</param>
        /// <param name="visible">The default visibilty of the workset</param>
        /// <param name="alias">If the alias already exists in the document, rename the alias to the workset instead of making it.  Provide an empty string "" to specify no alias.</param>
        /// <returns name="workset">A Revit Workset</returns>
        /// <returns name="created">True if the workset was created, false if the workset was already in the document</returns>
        [MultiReturn(new[] { "workset", "created"})]
        public static IDictionary ByName(
            string name,
            [DefaultArgument("true")] bool visible,
            [DefaultArgument("\"\"")] string alias
            )
        {
            //Get Revit Document object
            revitDoc doc = DocumentManager.Instance.CurrentDBDocument;

            Workset workset = null;
            bool created = false;

            //Only create workset if it's name isn't an empty string
            if (name != "")
            {
                //Verify that each workset isn't already in the document
                //If the workset is unique, create it
                if (WorksetTable.IsWorksetNameUnique(doc, name))
                {
                    //If the alias is already in the document
                    if (alias != null && WorksetTable.IsWorksetNameUnique(doc, alias) == false)
                    {
                        workset = GetByName(alias);
                        Rename(workset, name);
                        created = true;
                    }
                    else
                    {
                        using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(doc))
                        {
                            trans.Start("Create Workset");

                            workset = new Workset (doc, revitWorkset.Create(doc, name));

                            trans.Commit();
                        }
                        SetDefaultVisibility(workset, visible);
                        created = true;
                    }
                }
                // If the workset is already in the document, retrieve the workset
                else
                {
                    workset = GetByName(name);
                    created = false;
                }
            }

            return new Dictionary<string, object>
            {
                {"workset", workset},
                {"created", created}
            };
        }

        /// <summary>
        /// Retrieves the workset with the given name.
        /// </summary>
        /// <param name="name">A workset name</param>
        /// <returns name="workset">Returns a workset.  Returns null if workset does not exist.</returns>
        public static Workset GetByName(string name)
        {
            Workset foundWorkset = null;

            if (name != null)
            {
                //Get Revit Document object
                revitDoc doc = DocumentManager.Instance.CurrentDBDocument;
                FilteredWorksetCollector fwCollector = new FilteredWorksetCollector(doc);

                foreach (revitWorkset workset in fwCollector)
                {
                    if (workset.Name == name)
                    {
                        foundWorkset = new Workset(doc, workset);
                    }
                }
                fwCollector.Dispose();
            }
            return foundWorkset;
        }

        /// <summary>
        /// Retrieves the workset with the given name.
        /// </summary>
        /// <param name="worksetId">The workset ID</param>
        /// <returns name="workset">Returns a workset.  Returns null if workset does not exist.</returns>
        public static Workset GetByWorksetId(WorksetId worksetId)
        {
            //Get Revit Document object
            revitDoc doc = DocumentManager.Instance.CurrentDBDocument;
            return new Workset(doc, doc.GetWorksetTable().GetWorkset(worksetId));
        }

        /// <summary>
        /// Retrieves the workset with the given name.
        /// </summary>
        /// <param name="id">A integer of the id.</param>
        /// <returns name="workset">Returns a workset.  Returns null if workset does not exist.</returns>
        public static Workset GetById(int id)
        {
            //Get Revit Document object
            revitDoc doc = DocumentManager.Instance.CurrentDBDocument;
            return new Workset(doc, doc.GetWorksetTable().GetWorkset(new WorksetId(id)));
        }

        /// <summary>
        /// Retrieves all the user worksets from a document.  Excludes view and family worksets.
        /// </summary>
        /// <returns name="worksets">Returns all user worksets in the document.</returns>
        public static List<Workset> GetUserWorksets()
        {
            //Get Revit Document object
            revitDoc doc = DocumentManager.Instance.CurrentDBDocument;
            FilteredWorksetCollector fwCollector = new FilteredWorksetCollector(doc);
            List<Workset> worksets = new List<Workset>();

            foreach (revitWorkset workset in fwCollector.OfKind(WorksetKind.UserWorkset))
            {
                worksets.Add(new Workset(doc, workset));
            }
            return worksets;
        }

        /// <summary>
        /// Renames a workset.
        /// </summary>
        /// <param name="workset">A workset</param>
        /// <param name="name">A workset name</param>
        /// <returns name="workset">renamed workeset.</returns>
        /// <returns name="renamed">renamed workeset.</returns>
        [MultiReturn(new[] { "workset", "renamed" })]
        public static IDictionary Rename(Workset workset, string name)
        {
            //Get Revit Document object
            revitDoc doc = DocumentManager.Instance.CurrentDBDocument;
            Workset renamedWorkset = null;
            bool renamed = false;

            if (name != null && workset != null)
            {
                //Verify that the existing workset is in the document.
                //If the workset is unique, create it
                if (WorksetTable.IsWorksetNameUnique(doc, workset.internalWorkset.Name) == false)
                {
                    // Verify that the new name doesn't already exist
                    if (WorksetTable.IsWorksetNameUnique(doc, name) == true)
                    {
                        //Only rename workset if it's name isn't an empty string
                        if (name != "")
                        {
                            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(doc))
                            {
                                trans.Start("Rename Workset");

                                WorksetTable.RenameWorkset(doc, workset.internalId, name);
                                workset.internalName = workset.internalWorkset.Name;
                                renamedWorkset = workset;
                                renamed = true;

                                trans.Commit();
                            }
                        }
                    }
                }
            }

            return new Dictionary<string, object>
            {
                {"workset", renamedWorkset},
                {"renamed", renamed}
            };
        }

        /// <summary>
        /// Gets the workset's name as a string.
        /// </summary>
        /// <param name="workset">A workset.</param>
        /// <returns name="name">The workset's name as a string</returns>
        public static string Name(Workset workset)
        {
            string name = null;
            try
            {
                name = workset.internalName;
            }
            catch
            {
                name = null;
            }
            return name;
        }

        /// <summary>
        /// Gets the workset ID of a workset.
        /// </summary>
        /// <param name="workset">The workset that you wish to set the visibility of.</param>
        /// <returns name="workset ID">The worksetID</returns>
        public static revitWorksetId Id(Workset workset)
        {
            revitWorksetId id = null;
            try
            {
                id = workset.internalWorkset.Id;
            }
            catch
            {
                id = null;
            }
            return id;
        }

        /// <summary>
        /// Gets the GUID of the workset.  The GUID is stable between syncs.
        /// </summary>
        /// <param name="workset">The workset that you wish to set the visibility of.</param>
        /// <returns name="GUID">The worksetID</returns>
        public static Guid UniqueId(Workset workset)
        {
            Guid id = Guid.Empty;
            try
            {
                id = workset.internalWorkset.UniqueId;
            }
            catch
            {
                id = Guid.Empty;
            }
            return id;
        }

        /// <summary>
        /// Determines whether a given name is already used in the document.
        /// </summary>
        /// <param name="name">A workset name</param>
        /// <returns name="bool">True if workset name is unique in the document, false if already used.</returns>
        public static bool IsWorksetNameUnique(string name)
        {
            //Get Revit Document object
            revitDoc doc = DocumentManager.Instance.CurrentDBDocument;
            return WorksetTable.IsWorksetNameUnique(doc, name);
        }

        /// <summary>
        /// Sets the Default Visibility of a workset within a document.
        /// </summary>
        /// <param name="workset">The workset that you wish to set the visibility of.</param>
        /// <param name="visible">The visibility of the workset</param>
        /// <returns name="workset">A Revit workset</returns>
        public static Workset SetDefaultVisibility (Workset workset, bool visible)
        {
            //Get Revit Document object
            revitDoc doc = DocumentManager.Instance.CurrentDBDocument;

            using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(doc))
            {
                trans.Start("Set Workset Default Visibility");

                // Set the workset’s default visibility      
                WorksetDefaultVisibilitySettings defaultVisibility = WorksetDefaultVisibilitySettings.GetWorksetDefaultVisibilitySettings(doc);
                defaultVisibility.SetWorksetVisibility(workset.internalId, visible);

                trans.Commit();
                defaultVisibility.Dispose();
            }
            return workset;
        }

        /// <summary>
        /// Sets the Default Visibility of a workset within a document.
        /// </summary>
        /// <param name="workset">A workset.</param>
        /// <returns name="visibility">Whether the workset is visible by default.</returns>
        public static bool GetDefaultVisibility(Workset workset)
        {
            //Get Revit Document object
            revitDoc doc = DocumentManager.Instance.CurrentDBDocument;

            // Get the workset’s default visibility      
            WorksetDefaultVisibilitySettings defaultVisibility = WorksetDefaultVisibilitySettings.GetWorksetDefaultVisibilitySettings(doc);
            bool visibility = defaultVisibility.IsWorksetVisible(workset.internalId);

            defaultVisibility.Dispose();
            return visibility;
        }

        /// <summary>
        /// Converts a integer to a Workset ID.  It does not check whether the workset exists in the document.
        /// </summary>
        /// <param name="id">The id as a integer.</param>
        /// <returns name="WorksetId">A WorksetId</returns>
        public static WorksetId ConvertIntToWorksetId (int id)
        {
            return new WorksetId(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static revitWorkset Create(revitDoc doc, string name)
        {
            return revitWorkset.Create(doc, name);
        }

        /// <summary>
        /// Retrieves an element's workset.
        /// </summary>
        /// <param name="element">A Revit element.</param>
        /// <returns name="workset">A Revit workset.</returns>
        public static Workset GetElementWorkset(dynamoElement element)
        {
            //Get Revit Document object
            revitDoc doc = DocumentManager.Instance.CurrentDBDocument;

            WorksetId wid = element.InternalElement.WorksetId;
            Workset workset = GetByWorksetId(wid);

            return workset;
        }

        /// <summary>
        /// Changes the workset of an element.
        /// </summary>
        /// <param name="element">Dynamo Elements.</param>
        /// <param name="workset">A revit workset</param>
        /// <returns name="element">The element that was changed.  Returns null if the change was unsuccessfull.</returns>
        public static dynamoElement SetElementWorkset(dynamoElement element, Workset workset)
        {
            //Get Revit Document object
            revitDoc doc = DocumentManager.Instance.CurrentDBDocument;

            Autodesk.Revit.DB.Element unwrapped = element.InternalElement;

            WorksetId wId = unwrapped.WorksetId;
            Autodesk.Revit.DB.Parameter wsParam = unwrapped.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
            if (wsParam == null)
            {
                return null;
            }
            if (doc.IsModifiable)
            {
                wsParam.Set(workset.internalId.IntegerValue);
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction tx = new Autodesk.Revit.DB.Transaction(doc))
                {
                    tx.Start("Change Element's Workset");
                    wsParam.Set(workset.internalId.IntegerValue);
                    tx.Commit();
                }
            }
            return unwrapped.ToDSType(true); ;
        }

        /// <summary>
        /// Provides access to the Autodesk.Revit.DB.Workset object.
        /// </summary>
        /// <param name="workset">A workset.</param>
        /// <returns name="unwrapped">A Autodesk.Revit.DB.Workset object.</returns>
        public static revitWorkset Unwrap (Workset workset)
        {
            return workset.internalWorkset;
        }

        /// <summary>
        /// Converts a Autodesk.Revit.DB.Workset to a Workset object.
        /// </summary>
        /// <param name="document">A workset.</param>
        /// <param name="workset">A Autodesk.Revit.DB.Workset.</param>
        /// <returns name="wrapped">A Autodesk.Revit.DB.Workset object.</returns>
        public static Workset Wrap(revitDoc document, revitWorkset workset)
        {
            return new Workset(document, workset);
        }

        /// <summary>
        /// Creates a string representation of the workset.
        /// </summary>
        /// <returns name="string">Returns a string representation of the workset.</returns>
        public override string ToString()
        {
            return string.Format("Workset(Name=\"{0}\", ID={1})", this.internalName, this.internalId);
        }
    }
}
