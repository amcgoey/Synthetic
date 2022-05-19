using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;

namespace Synthetic.Core
{
    /// <summary>
    /// Wrapper for using enumerations
    /// </summary>
    public class Enumeration
    {
        [IsVisibleInDynamoLibrary(false)]
        internal Enumeration () { }

        /// <summary>
        /// Retrieves a enum of the given type and name.
        /// </summary>
        /// <param name="enumTypeName">The enum type as a string. Requires the full assembly path in front of the type.</param>
        /// <param name="name">Name of the enum.</param>
        /// <returns name="enum">Returns a enum.</returns>
        public static System.Object Parse (string enumTypeName, string name)
        {
            System.Object e = null;
            try
            {
                Type et = GetEnumType(enumTypeName);
                e = Enum.Parse(et,name);
            }
            catch (ArgumentException)
            {
                e = null;
            }
            return e;
        }

        /// <summary>
        /// Retrieves a list of the names of the constants in a specified enumeration.
        /// </summary>
        /// <param name="enumTypeName">The enum type as a string. Requires the full assembly path in front of the type.</param>
        /// <returns name="names">A string list of the names of the constants in enumType.</returns>
        public static List<string> GetNames(string enumTypeName)
        {
            List<string> e = new List<string>();
            Type eType = GetEnumType(enumTypeName);
            foreach (string name in Enum.GetNames(eType))
            {
                e.Add(name);
            }
            return e;
        }

        /// <summary> 
        /// Retrieves the name as a string of the enumeration. 
        /// </summary> 
        /// <param name="enumeration">A enum</param> 
        /// <returns name="name">Returns the name of the enum as a string</returns> 
        public static string GetName(Enum enumeration)
        {
            var enumType = enumeration.GetType();
            return Enum.GetName(enumType, enumeration);
        }

        /// <summary>
        /// Retrieves all enums of a given type.
        /// </summary>
        /// <param name="enumTypeName">The enum type as a string. Requires the full assembly path in front of the type.</param>
        /// <returns name="enums">A list of enums.</returns>
        public static List<System.Object> GetEnums(string enumTypeName)
        {
            List<System.Object> eList = new List<System.Object>();
            Type eType = GetEnumType(enumTypeName);
            foreach (string name in Enum.GetNames(eType))
            {
                System.Object e = Parse(enumTypeName, name);
                eList.Add(e);
            }
            return eList;
        }

        /// <summary>
        /// Evaluates a enum and returns its value.
        /// </summary>
        /// <param name="enumeration">A enum</param>
        /// <returns name="value">Returns the value of the enum</returns>
        public static object GetValue(object enumeration)
        {
            //int i = Convert.ToInt32(enumeration);
            //return i;
            var enumType = enumeration.GetType();
            var underlyingType = Enum.GetUnderlyingType(enumType);
            var numericValue = System.Convert.ChangeType(enumeration, underlyingType);
            return numericValue;
        }

        /// <summary>
        /// Retrieves the Enum Type from a string name.
        /// </summary>
        /// <param name="enumTypeName">The enum type as a string. Requires the full assembly path in front of the type.</param>
        /// <returns name="enumType">Returns the enum type if it exists in the current domain.</returns>
        public static Type GetEnumType(string enumTypeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumTypeName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }

        /// <summary>
        /// Returns an indication whether a constant with a specified value exists in a specified enumeration.
        /// </summary>
        /// <param name="enumTypeName">The enum type as a string. Requires the full assembly path in front of the type.</param>
        /// <param name="name">Name of the enum.</param>
        /// <returns name="bool">True if a constant in enumType has a value equal to value; otherwise, false.</returns>
        public static bool IsDefined(string enumTypeName, string name)
        {
            Type eType = GetEnumType(enumTypeName);
            System.Object e = Parse(enumTypeName, name);

            return Enum.IsDefined(eType, e);
        }
    }
}
