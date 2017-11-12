using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Synthetic.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Assemblies
    {
        /// <summary>
        /// Returns all classes and methods in an assembly.  Modified from version found at http://venkateswarlu.net/DotNet/Get_all_methods_from_a_class.aspx.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public List<string> GetPublicClassesAndMethodsOfAssembly(string assemblyName)
        {
            List<string> assemblyInfo = new List<string>();

            //Code to load Assembly
            Assembly assem1 = Assembly.Load(AssemblyName.GetAssemblyName(assemblyName));

            //Get List of Class Name
            Type[] types = assem1.GetTypes();

            foreach (Type type in types)
            {
                string typeName = "";
                string typeNamespace = type.Namespace;
                List<string> methodsPublic = new List<string>();
                List<string> methodsNotPublic = new List<string>();

                if (type.IsPublic)
                {
                   assemblyInfo.Concat(GetPublicMethods(type));
                }
                else if (type.IsAbstract)
                {
                    //assemblyInfo.Concat(GetPublicMethods(tc));
                }
                else if (type.IsSealed)
                {
                    //assemblyInfo.Concat(GetPublicMethods(tc));
                }
            }

            return assemblyInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns name="Methods">List of the names of methods including Namespace, Type and Method.</returns>
        public List<string> GetPublicMethods(Type type)
        {
            string typeName = type.Name;
            string typeNamespace = type.Namespace;
            List<string> methodsPublic = new List<string>();
            List<string> methodsNotPublic = new List<string>();

            //Get List of Method Names of Class
            MemberInfo[] methodName = type.GetMethods();

            foreach (MemberInfo method in methodName)
            {
                if (method.ReflectedType.IsPublic)
                {
                    methodsPublic.Add(typeNamespace + typeName + method.Name.ToString());
                }
                else
                {
                    methodsNotPublic.Add(typeNamespace + typeName + method.Name.ToString());
                }
            }

            return methodsPublic;
        }
    }
}
}
