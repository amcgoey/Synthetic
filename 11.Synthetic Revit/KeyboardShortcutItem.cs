using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using Autodesk.DesignScript.Runtime;

namespace Synthetic.Revit.KeyboardShortcuts
{
    public class KeyboardShortcutItem
    {
        private List<string> shortcuts;

        /// <summary>
        /// A Revit command
        /// </summary>
        [XmlAttribute]
        public string CommandName { get; set; }

        /// <summary>
        /// The Revit Command ID
        /// </summary>
        [XmlAttribute]
        public string CommandId { get; set; }

        /// <summary>
        /// Dictionary value when a shortcut isn't included.
        /// </summary>
        [SupressImportIntoVM]
        public const string EmptyShortcut = "!NO SHORTCUT!";

        /// <summary>
        /// The Shortcuts as a deliminted string.
        /// </summary>
        [XmlAttribute(AttributeName = "Shortcuts")]
        public string ShortcutString
        {
            get { return KeyboardShortcutItem.ToShortcutString(this.shortcuts); }
            set { this.shortcuts = KeyboardShortcutItem.ToShortcutList(value); }
        }

        /// <summary>
        /// The Shortcuts as a list.
        /// </summary>
        [XmlIgnoreAttribute]
        public List<string> ShortcutList
        {
            get { return shortcuts; }
            set { shortcuts = value; }
        }

        /// <summary>
        /// The namespace path to the command
        /// </summary>
        [XmlAttribute]
        public string Paths { get; set; }

        /// <summary>
        /// Creates an empty Keyboard shortcut item.
        /// </summary>
        [SupressImportIntoVM]
        public KeyboardShortcutItem ()
        {
            this.CommandName = null;
            this.CommandId = null;
            this.ShortcutList = new List<string>();
            this.Paths = null;
        }

        /// <summary>
        /// Creates a new Keyboard Shortcut Item.
        /// </summary>
        /// <param name="commandName">A revit command</param>
        /// <param name="CommandId">The Revit Command ID</param>
        /// <param name="shortcuts">A string of shortcuts with multiple shortcuts delimited by a '#' character</param>
        /// <param name="path">The namespace path to the command</param>
        public KeyboardShortcutItem (string commandName,
            string CommandId,
            [DefaultArgument("Synthetic.Core.DynamoUtility.GetNull()")] string shortcuts,
            string path)
        {
            this.CommandName = commandName;
            this.CommandId = CommandId;
            this.ShortcutList= KeyboardShortcutItem.ToShortcutList(shortcuts);
            this.Paths = path;
        }

        /// <summary>
        /// Takes a delimited string of shortcuts and returns a list of shortcuts.
        /// </summary>
        /// <param name="ShortcutString">A delimited string of shortcuts</param>
        /// <param name="seperator">String used as a delimitor for the shortcuts</param>
        /// <returns name="Shortcut List">List of shortcuts</returns>
        public static List<string> ToShortcutList (string ShortcutString, string seperator = "#")
        {
            List<string> results = null;

            if (ShortcutString != null && ShortcutString != "")
            {
                List<string> shortcuts = new List<string>(ShortcutString.Split(new[] { seperator }, StringSplitOptions.None));
                
                if(shortcuts.Contains(KeyboardShortcutItem.EmptyShortcut))
                {
                    shortcuts.Remove(KeyboardShortcutItem.EmptyShortcut);
                }

                if (shortcuts != null && shortcuts.Count > 0)
                {
                    results = shortcuts;
                }
            }
            return results;
        }

        /// <summary>
        /// Returns the shortcuts seperated by the seperator string
        /// </summary>
        /// <param name="ShortCutList">List of shortcut strings</param>
        /// <param name="seperator">A string used to delimit the shortcuts</param>
        /// <returns name="ShortcutString">A delimited string of shortcuts</returns>
        public static string ToShortcutString (List<string> ShortCutList, string seperator = "#")
        {
            string combined = "";

            if (ShortCutList != null)
            {
                foreach (string shortcut in ShortCutList)
                {
                    if (shortcut != KeyboardShortcutItem.EmptyShortcut)
                    {
                        if (combined != "")
                        {
                            combined = combined + seperator;
                        }
                        combined = combined + shortcut;
                    }
                }
            }
            if (combined == "") { combined = null; }

            return combined;
        }

        /// <summary>
        /// Adds a shortcut to the command.
        /// </summary>
        /// <param name="Shortcut">A valid Revit shortcut combination</param>
        /// <returns name="Success">True if the shortcut was added.  False if the shortcut already exists or the shortcut is null or an empty string</returns>
        public bool AddShortcut (string Shortcut)
        {
            bool result = false;

            if (this.ShortcutList != null)
            {
                if (Shortcut != null
                    && Shortcut != ""
                    && !this.ShortcutList.Contains(Shortcut)
                    && Shortcut != KeyboardShortcutItem.EmptyShortcut)
                {
                    this.ShortcutList.Add(Shortcut);
                    result = true;
                }
            }
            else
            {
                List<string> l = new List<string>();
                l.Add(Shortcut);
                this.ShortcutList = l;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Removes a shortcut from the command.
        /// </summary>
        /// <param name="Shortcut">A valid Revit shortcut combination</param>
        /// <returns name="Success">True if the shortcut was removed.  False if the shortcut didn't exist or the shortcut was null or an empty string</returns>
        public bool RemoveShortcut (string Shortcut)
        {
            bool result = false;

            if (Shortcut != null
                && Shortcut != ""
                && this.ShortcutList != null
                && this.ShortcutList.Contains(Shortcut))
            {
                this.ShortcutList.Remove(Shortcut);
                if(this.ShortcutList.Count == 0) { this.ShortcutList = null; }
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Checks whether the Shortcut value exists for the command.
        /// </summary>
        /// <param name="Shortcut">A valid Revit shortcut combination</param>
        /// <returns name"Exists">True if the shortcut exists for the command.  False if it doesn't exist or the shorct is null or an empty string</returns>
        public bool ContainsShortcut(string Shortcut)
        {
            bool result = false;

            if (Shortcut != null
                && Shortcut != ""
                && this.ShortcutList.Contains(Shortcut))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Prints the command name and the namespace with type.
        /// </summary>
        /// <returns name="string">Converts to a string.</returns>
        public override string ToString()
        {
            Type t = typeof(KeyboardShortcutItem);

            string s = t.Namespace + "." + GetType().Name + " ( " + this.CommandName + ", " + this.ShortcutString + " )";

            return s;
        }
    }
}
