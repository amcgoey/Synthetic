using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using synthDict = Synthetic.Core.Dictionary;

namespace Synthetic.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Assemblies
    {
        internal Assemblies() { }

        /// <summary>
        /// Gets all loaded assemblies
        /// </summary>
        /// <returns name="Assemblies">Returns a dictionary of assemblies with assembly full name as key.</returns>
        public static synthDict GetAssemblies()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //var assems = assemblies.Select((value, index) => new { value, index });
            //Dictionary<string, System.Object> dict = assems.ToDictionary(pair => pair.value.FullName, pair => (System.Object)pair.value);

            Dictionary<string, System.Object> dict = new Dictionary<string, System.Object>();
            synthDict sDict = null;
            foreach ( Assembly assembly in assemblies)
            {
                string assemblyName = assembly.FullName;
                if (!dict.ContainsKey(assemblyName))
                {
                    dict.Add(assemblyName, assembly);
                }
            }

            sDict = new synthDict(dict);
            return sDict;
        }

        /// <summary>
        /// Gets all loaded assemblies
        /// </summary>
        /// <returns name="Assemblies">Returns a dictionary of assemblies with assembly full name as key.</returns>
        public static List<Assembly> GetAssembliesAsList()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.ToList();
        }

        /// <summary>
        /// Returns the full name of a DLL assembly
        /// </summary>
        /// <param name="assembly">A DLL assembly</param>
        /// <returns name="Name">Returns the full name of the assembly</returns>
        public static string FullName (Assembly assembly)
        {
            return assembly.FullName;
        }

        /// <summary>
        /// Given a DLL assembly, returns the types
        /// </summary>
        /// <param name="assembly">A DLL assembly</param>
        /// <returns name="Types">Returns the types within an assembly</returns>
        public static Type[] GetTypes (Assembly assembly)
        {
            return assembly.GetTypes();
        }

        /// <summary>
        /// Loads a DLL assembly given it's long name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Assembly LoadByName (string name)
        {
            return Assembly.Load(name);
        }

        /// <summary>
        /// Returns all classes and methods in an assembly.  Modified from version found at http://venkateswarlu.net/DotNet/Get_all_methods_from_a_class.aspx.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static List<string> GetPublicClassesAndMethods(Assembly assembly)
        {
            List<string> assemblyInfo = new List<string>();

            //Code to load Assembly
            //Assembly assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyName));

            //Get List of Class Name
            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                if (type.IsPublic)
                {
                   assemblyInfo.Concat(GetPublicStaticMethods(type));
                }
            }

            return assemblyInfo;
        }

        /// <summary>
        /// Returns all the methods of a Type that are public and static.
        /// </summary>
        /// <param name="type">A Type</param>
        /// <returns name="Methods">List of the names of methods including Namespace, Type and Method.</returns>
        public static List<string> GetPublicStaticMethods(Type type)
        {
            string typeName = type.Name;
            string typeNamespace = type.Namespace;
            List<string> methodsPublic = new List<string>();

            //Get List of Method Names of Class
            MemberInfo[] methodName = type.GetMethods();

            foreach (MethodInfo method in methodName)
            {
                if (method.ReflectedType.IsPublic)
                {
                    if (method.IsStatic)
                    {
                        methodsPublic.Add(typeNamespace + "." + typeName + "." + method.Name.ToString());
                    }
                }
            }

            return methodsPublic;
        }

        /// <summary>
        /// Gets the Enumerations of a Type.
        /// </summary>
        /// <param name="objectType">A Type</param>
        /// <returns name="Types">Enumerations of a Type.</returns>
        public static List<Type> GetEnumerableOfType(Type objectType)
        {
            List<Type> objects = new List<Type>();
            foreach (Type type in
                Assembly.GetAssembly(objectType).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(objectType)))
            {
                //objects.Add((T)Activator.CreateInstance(type, constructorArgs));
                objects.Add(type);
            }
            //objects.Sort();
            return objects;
        }
    }
}
