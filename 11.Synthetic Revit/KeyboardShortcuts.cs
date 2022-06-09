using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Autodesk.DesignScript.Runtime;

namespace Synthetic.Revit.KeyboardShortcuts
{
    /// <summary>
    /// An object that aggregates and modifies Revit Keyboard Shortcuts for generating new Keyboard Shortcut files.
    /// </summary>
    [XmlRoot("Shortcuts")]
    public class KeyboardShortcuts
    {
        private Lookup<string, KeyboardShortcutItem> lookupShortcuts;
        private List<KeyboardShortcutItem> shortcuts;

        /// <summary>
        /// List of ShortcutItems
        /// </summary>
        [XmlElement("ShortcutItem")]
        public List<KeyboardShortcutItem> ShortcutItems 
        { 
            get { return this.shortcuts; }
            set { this.shortcuts = value; }
        }

        /// <summary>
        /// Dictionary value when a shortcut isn't included.
        /// </summary>
        [SupressImportIntoVM]
        public const string EmptyShortcut = "!NO SHORTCUT!";

        /// <summary>
        /// Constructs an empty KeyboardShortcut object.
        /// </summary>
        [SupressImportIntoVM]
        public KeyboardShortcuts()
        {
            this.ShortcutItems = new List<KeyboardShortcutItem>();
        }

        /// <summary>
        /// Constructions a KeyboardShortcuts object from a list of ShortcutItem objects.
        /// </summary>
        /// <param name="shortcuts">A list of ShortcutItem objects</param>
        public KeyboardShortcuts(List<KeyboardShortcutItem> shortcuts)
        {
            ShortcutItems = shortcuts;
        }

        /// <summary>
        /// Add a shortcut to the list.  It will merge the shortcut into any existing shortcuts.
        /// </summary>
        /// <param name="ShortcutItems">A KeyboardShortcutItem with shortcuts to add.</param>
        /// <returns name="KeyboardShortcuts">The modified KeyboardShortcut object</returns>
        /// <returns name="Duplicate Shortcuts">A list of shortcuts that were already included in Keyboarshortucts</returns>
        /// <returns name="Success">Returns true if the shortcut was added.</returns>
        [MultiReturn(new[] { "KeyboardShortcuts", "Duplicate Shortcuts", "Success" })]
        public IDictionary AddShortcut(List<KeyboardShortcutItem> ShortcutItems)
        {
            Dictionary<string, List<KeyboardShortcutItem>> duplicateShortcuts = null;
            List<bool> results = null;
            bool result = false;            

            //Dictionary<string, List<KeyboardShortcutItem>> shortcutDict = this.ToDictionaryByShortcut();
            Dictionary<string, KeyboardShortcutItem> commandDict = this.ToDictionaryByCommand();

            // Verify that the input wasn't null.
            if (ShortcutItems != null)
            {

                duplicateShortcuts = new Dictionary<string, List<KeyboardShortcutItem>>();
                results = new List<bool>();

                foreach (KeyboardShortcutItem shortcutItem in ShortcutItems)
                {
                    result = false;

                    // If the command already existings in the keyboardShortcuts
                    if (commandDict.ContainsKey(shortcutItem.CommandName))
                    {
                        // Get the existing ShortcutItem
                        KeyboardShortcutItem existShortcutItem = commandDict[shortcutItem.CommandName];
                        
                        // For each shortcut in the new shortcutItem
                        foreach (string shortcut in shortcutItem.ShortcutList)
                        {
                            // Check if the shortcut existings in the existing ShortcutItem
                            // If it doesn't existing, add it.
                            if (!existShortcutItem.ContainsShortcut(shortcut))
                            {
                                result = existShortcutItem.AddShortcut(shortcut);
                                results.Add(result);
                            }
                            // Else the shortcut already exists so don't add it.
                            // Instead, add it to the list of duplicate shortcuts.
                            else
                            {
                                KeyboardShortcutItem duplicateShortcut = new KeyboardShortcutItem(
                                    shortcutItem.CommandName,
                                    shortcutItem.CommandId,
                                    shortcut,
                                    shortcutItem.Paths);

                                //  If the shortcut already had an duplicate entry, add the new item to that entry
                                if (duplicateShortcuts.ContainsKey(shortcut))
                                {
                                    List<KeyboardShortcutItem> shortcuts = duplicateShortcuts[shortcut];
                                    shortcuts.Add(duplicateShortcut);
                                    duplicateShortcuts[shortcut] = shortcuts;
                                }
                                // Else, the shotcut doesn't have an entry, so add a new entry.
                                else
                                {
                                    duplicateShortcuts.Add(shortcut, new List<KeyboardShortcutItem> { duplicateShortcut });
                                }
                                results.Add(false);
                            }
                        }
                    }
                    // Else the command doesn't existing yet so add it.
                    else
                    {
                        this.ShortcutItems.Add(shortcutItem);
                        result = true;
                        results.Add(result);
                    }
                }
            }
            return new Dictionary<string, object>
            {
                { "KeyboardShortcuts", this },
                { "Duplicate Shortcuts", duplicateShortcuts},
                { "Success", results }
            };
        }

        /// <summary>
        /// Removes a shortcut from the list.
        /// </summary>
        /// <param name="ShortcutItems">A KeyboardShortcutItem with shortcuts to remove.</param>
        /// <returns name="KeyboardShortcuts">The modified KeyboardShortcut object</returns>
        /// <returns name="Success">Returns true if the shortcut was removed.</returns>
        [MultiReturn(new[] { "KeyboardShortcuts", "Success" })]
        public IDictionary RemoveShortcut(List<KeyboardShortcutItem> ShortcutItems)
        {
            List<bool> results = new List<bool>();
            bool result = false;

            //Dictionary<string, List<KeyboardShortcutItem>> shortcutDict = this.ToDictionaryByShortcut();
            Dictionary<string, KeyboardShortcutItem> commandDict = this.ToDictionaryByCommand();

            if (ShortcutItems != null)
            {
                foreach (KeyboardShortcutItem shortcutItem in ShortcutItems)
                {
                    if (commandDict.ContainsKey(shortcutItem.CommandName))
                    {
                        KeyboardShortcutItem existShortcutItem = commandDict[shortcutItem.CommandName];
                        if (existShortcutItem.ShortcutList != null)
                        {
                            List<string> existList = new List<string>(existShortcutItem.ShortcutList);
                            foreach (string shortcut in existList)
                            {
                                result = existShortcutItem.RemoveShortcut(shortcut);
                                results.Add(result);
                            }
                        }
                    }
                }
            }
            return new Dictionary<string, object>
            {
                { "KeyboardShortcuts", this },
                { "Success", results }
            };
        }

        /// <summary>
        /// Creates a new KeyboardShortcut object with all the empty shortcut commands removed.
        /// </summary>
        /// <returns name="KeyboardShortcuts">Creates a new KeyboardShortcut object with all the empty shortcut commands removed.</returns>
        public KeyboardShortcuts RemoveEmptyShortcuts ()
        {
            bool result = false;

            KeyboardShortcuts newKBS = new KeyboardShortcuts();

            List<KeyboardShortcutItem> purgedValues = null;

            foreach(KeyboardShortcutItem itemList in this.shortcuts)
            {
                if (itemList.ShortcutList != null)
                {
                    foreach (string shortcut in itemList.ShortcutList)
                    {
                        purgedValues = new List<KeyboardShortcutItem>();

                        if (shortcut != null && shortcut != "" && shortcut != KeyboardShortcuts.EmptyShortcut)
                        {
                            purgedValues.Add(new KeyboardShortcutItem(itemList.CommandName, itemList.CommandId, shortcut, itemList.Paths));
                            newKBS.AddShortcut(purgedValues);
                        }
                    }
                }
            }

            return newKBS;
        }

        /// <summary>
        /// Checks for any commands that share the same shortcuts.
        /// </summary>
        /// <returns name="DuplicateCommands">A dictionary of KeyboardShortcutItems keyed by command name that are duplicates.</returns>
        public Dictionary<string, List<KeyboardShortcutItem>> GetDuplicateShortcuts ()
        {
            //this.lookupShortcuts = (Lookup<string, KeyboardShortcutItem>)

            Dictionary<string, List<KeyboardShortcutItem>> duplicateCommands = new Dictionary<string, List<KeyboardShortcutItem>>();
            Dictionary<string, List<KeyboardShortcutItem>> shortcutDict = this.ToDictionaryByShortcut();

            foreach(string shortcut in shortcutDict.Keys)
            {
                if (shortcutDict[shortcut].Count > 1)
                {
                    duplicateCommands.Add(shortcut, shortcutDict[shortcut]);
                }
            }

            return duplicateCommands;
        }

        /// <summary>
        /// Compares two KeyboardShortcut objects to find duplicate entries in both files as well as entries of the same shortcut but different commands
        /// </summary>
        /// <param name="First">A KeyboardShortcut object</param>
        /// <param name="Second">A KeyboardShortcut object</param>
        /// <returns name="Duplicates">Returns a KeyboardShortcut object that includes all the duplicate shortcuts with commands.</returns>
        /// <returns name="Conflicts">Returns a KeyboardShortcut object that includes the commands with shortcuts from both lists that have the same shortcuts.</returns>
        [MultiReturn(new[] { "Duplicates", "Conflicts" })]
        public static IDictionary Compare (KeyboardShortcuts First, KeyboardShortcuts Second)
        {
            KeyboardShortcuts Duplicates = null;
            KeyboardShortcuts Conflicts = null;

            if (First != null && Second != null)
            {
                Duplicates = new KeyboardShortcuts();
                Conflicts = new KeyboardShortcuts();

                Dictionary<string, List<KeyboardShortcutItem>> firstShortcuts = First.ToDictionaryByShortcut();
                Dictionary<string, List<KeyboardShortcutItem>> secondShortcuts = Second.ToDictionaryByShortcut();

                // Check ever entry in the First KeyboardShortcuts
                foreach (KeyValuePair<string, List<KeyboardShortcutItem>> firstPairs in firstShortcuts)
                {
                    // The string value of the individual shortcut to check against.
                    string shortcut = firstPairs.Key;

                    List<KeyboardShortcutItem> firstCommands = firstPairs.Value;

                    // Only proceed if the there is a shortcut and that shortcut is in Second.
                    if (shortcut != KeyboardShortcuts.EmptyShortcut && secondShortcuts.ContainsKey(shortcut))
                    {
                        // Create separate KeyboardShortcuts from Second to compare the current First entry against
                        List<KeyboardShortcutItem> secondCommands = secondShortcuts[shortcut];
                        KeyboardShortcuts secondToSearch = new KeyboardShortcuts(secondCommands);

                        // Check each firstCommand that has the shortcut
                        foreach (KeyboardShortcutItem firstCommand in firstCommands)
                        {
                            //  Check each secondCommand against the firstCommand
                            foreach (KeyboardShortcutItem secondCommand in secondToSearch.ShortcutItems)
                            {
                                // If Second has the same command, then it is a duplicate
                                if (firstCommand.CommandName == secondCommand.CommandName)
                                {
                                    // Create a new KeyboardShortcutItem that includes only the shortcut that is duplicate.
                                    KeyboardShortcutItem item = new KeyboardShortcutItem();
                                    item.CommandName = firstCommand.CommandName;
                                    item.ShortcutString = shortcut;
                                    item.CommandId = firstCommand.CommandId;
                                    item.Paths = firstCommand.Paths;

                                    List<KeyboardShortcutItem> l = new List<KeyboardShortcutItem>();
                                    l.Add(item);
                                    Duplicates.AddShortcut(l);
                                }
                                // Else, Second has a different command with the same shortcut.
                                else
                                {
                                    // Create a new KeyboardShortcutItems that includes only the shortcut in conflict.
                                    KeyboardShortcutItem firstItem = new KeyboardShortcutItem();
                                    firstItem.CommandName = firstCommand.CommandName;
                                    firstItem.ShortcutString = shortcut;
                                    firstItem.CommandId = firstCommand.CommandId;
                                    firstItem.Paths = firstCommand.Paths;

                                    KeyboardShortcutItem secondItem = new KeyboardShortcutItem();
                                    secondItem.CommandName = secondCommand.CommandName;
                                    secondItem.ShortcutString = shortcut;
                                    secondItem.CommandId = secondCommand.CommandId;
                                    secondItem.Paths = secondCommand.Paths;

                                    List<KeyboardShortcutItem> l = new List<KeyboardShortcutItem>();
                                    l.Add(firstItem);
                                    l.Add(secondItem);
                                    Conflicts.AddShortcut(l);
                                }
                            }

                        }
                    }
                }
            }

            return new Dictionary<string, object>
            {
                { "Duplicates", Duplicates },
                { "Conflicts", Conflicts }
            };
        }

        /// <summary>
        /// Checks if the command is in the KeyboardShortcuts object.
        /// </summary>
        /// <param name="CommandName">The name of a Revit command as listed in the Keyboard Shortcuts</param>
        /// <returns name="Contains">Returns True if the command is in the KeyboardShortcut object, otherwise returns False.</returns>
        public bool ContainsCommand (string CommandName)
        {
            bool result = false;

            Dictionary<string, KeyboardShortcutItem> dictCommands = this.ToDictionaryByCommand();

            if(dictCommands.ContainsKey(CommandName)) { result = true; }

            return result;
        }

        /// <summary>
        /// Checks to see if the key combitnation is included in the KeyboardShortcuts object
        /// </summary>
        /// <param name="Shortcut">A key combniation</param>
        /// <returns name="Contains">Returns True if the key combination is in the KeyboardShortcut object, otherwise returns False.</returns>
        public bool ContainsShortcut (string Shortcut)
        {
            bool result = false;

            Dictionary<string, List<KeyboardShortcutItem>> dictShortcuts = this.ToDictionaryByShortcut();

            if (dictShortcuts.ContainsKey(Shortcut)) { result = true; }

            return result;
        }

        /// <summary>
        /// Given a command name, returns the KeyboardShortcutItem
        /// </summary>
        /// <param name="CommandName">Name of the Revit command</param>
        /// <returns name="KeyboardShortcutItem">The KeyboardShortcutItem associated with the command.</returns>
        public KeyboardShortcutItem GetByCommand (string CommandName)
        {
            KeyboardShortcutItem shortcut = null;

            Dictionary<string, KeyboardShortcutItem> dictCommands = this.ToDictionaryByCommand();

            if (dictCommands.ContainsKey(CommandName)) { shortcut = dictCommands[CommandName]; }

            return shortcut;
        }

        /// <summary>
        /// Given a key combination representing a shortcut, returns a list of KeyboardShortcutItems that match.
        /// </summary>
        /// <param name="Shortcut">A key combination representing a shortcut</param>
        /// <returns name="KeyboardShortcutItems">A list of KeyboardShortcutItems associated with the command.</returns>
        public List<KeyboardShortcutItem> GetByShortcut(string Shortcut)
        {
            List<KeyboardShortcutItem> command = null;

            Dictionary<string, List<KeyboardShortcutItem>> dictShorcuts = this.ToDictionaryByShortcut();

            if (dictShorcuts.ContainsKey(Shortcut)) { command = dictShorcuts[Shortcut]; }

            return command;
        }

        /// <summary>
        /// Creates a dictionary with keys by shortcut and a list of commands using that shortcut.  Commands without a shortcut are listed under "NO SHORTCUT"
        /// </summary>
        /// <returns name="Shortcut Dictionary">A dictionary keyed to the shortcuts.</returns>
        public Dictionary<string, List<KeyboardShortcutItem>> ToDictionaryByShortcut ()
        {
            Dictionary<string, List<KeyboardShortcutItem>> dict = new Dictionary<string, List<KeyboardShortcutItem>>();

            if (this.shortcuts != null)
            {
                foreach (KeyboardShortcutItem command in this.shortcuts)
                {
                    List<string> shortcutList = command.ShortcutList;
                    if (shortcutList != null)
                    {
                        if (shortcutList.Count == 0)
                        {
                            shortcutList.Add(KeyboardShortcuts.EmptyShortcut);
                        }

                        foreach (string shortcut in shortcutList)
                        {
                            string sc = shortcut;
                            if (sc == null || sc == "" || sc == KeyboardShortcuts.EmptyShortcut)
                            {
                                sc = KeyboardShortcuts.EmptyShortcut;
                            }
                            if (dict.ContainsKey(sc))
                            {
                                dict[sc].Add(command);
                            }
                            else
                            {
                                List<KeyboardShortcutItem> commandList = new List<KeyboardShortcutItem>();
                                commandList.Add(command);
                                dict.Add(sc, commandList);
                            }
                        }
                    }
                }
            }
            if (dict.Count == 0) { dict = null; }

            return dict;
        }

        /// <summary>
        /// Creates a Dictionary keyed to the command name.
        /// </summary>
        /// <returns name="Command Dictionary">Creates a Dictionary keyed to the command name.</returns>
        public Dictionary<string, KeyboardShortcutItem> ToDictionaryByCommand()
        {
            Dictionary<string, KeyboardShortcutItem> dict = new Dictionary<string, KeyboardShortcutItem>();

            if (this.shortcuts != null)
            {
                foreach (KeyboardShortcutItem command in this.shortcuts)
                {
                    string name = command.CommandName;
                    if (!dict.ContainsKey(name))
                    {
                        dict.Add(name, command);
                    }
                }
            }
            else { dict = null; }

            return dict;
        }

        /// <summary>
        /// Converts the Keyboard Shortcuts to XML
        /// </summary>
        /// <returns name="Xml">Returns a string of the XML representation of the shortucts</returns>
        public string ConvertToXML()
        {
            string xml = null;

            // Remove Declaration
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
                //ConformanceLevel = ConformanceLevel.Fragment
            };
            // Remove Namespace
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            XmlSerializer serializer = new XmlSerializer(typeof(KeyboardShortcuts));
            using (var sWriter = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sWriter, settings))
                {
                    serializer.Serialize(writer, this, emptyNamespaces);
                    xml = sWriter.ToString();
                }
            }
            return xml;
        }

        /// <summary>
        /// Deserializes a KeyboardShortcuts object from an string of XML
        /// </summary>
        /// <param name="Xml">A string of the XML representing a KeyboardShortcuts bojects</param>
        /// <returns name="KeyboardShortcuts">A KeyboardShortcuts object</returns>
        public static KeyboardShortcuts ConvertFromXML(string Xml)
        {
            KeyboardShortcuts ks = null;

            if (Xml != null)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(KeyboardShortcuts));

                using (TextReader reader = new StringReader(Xml))
                {
                    ks = (KeyboardShortcuts)serializer.Deserialize(reader);
                }
            }
            return ks;
        }

        /// <summary>
        /// Prints the KeyboardShortcuts as a string.
        /// </summary>
        /// <returns name="string">Converts to a string.</returns>
        public override string ToString()
        {
            Type t = typeof(KeyboardShortcuts);

            string s = t.Namespace + "." + GetType().Name;

            int i = 0;
            foreach (KeyboardShortcutItem shortcut in this.ShortcutItems)
            {
                s = s + string.Format("\n  {0} shorcut-> \"{1}\"", i, shortcut.ToString());
                i++;
            }
            return s;
        }
    }
}
