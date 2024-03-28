using DragynGames.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace DragynGames
{
    public class UnityBuiltInAssemblyIgnorer

    {
        public static List<Assembly> GetAssemblies()
        {
            List<Assembly> assemlbyResults = new List<Assembly>();
            CompareInfo caseInsensitiveComparer = new CultureInfo("en-US").CompareInfo;
#if UNITY_EDITOR || !NETFX_CORE
            // Find all [ConsoleMethod] functions
            // Don't search built-in assemblies for console methods since they can't have any
            string[] ignoredAssemblies = new string[]
            {
                "Unity",
                "System",
                "Mono.",
                "mscorlib",
                "netstandard",
                "TextMeshPro",
                "Microsoft.GeneratedCode",
                "I18N",
                "Boo.",
                "UnityScript.",
                "ICSharpCode.",
                "ExCSS.Unity",
#if UNITY_EDITOR
                "Assembly-CSharp-Editor",
                "Assembly-UnityScript-Editor",
                "nunit.",
                "SyntaxTree.",
                "AssetStoreTools",
#endif
            };
#endif

#if UNITY_EDITOR || !NETFX_CORE
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
#else
			foreach( Assembly assembly in new Assembly[] { typeof( DebugLogConsole ).Assembly } ) // On UWP, at least search this plugin's Assembly for console methods
#endif
            {
#if( NET_4_6 || NET_STANDARD_2_0 ) && ( UNITY_EDITOR || !NETFX_CORE )
                if (assembly.IsDynamic)
                    continue;
#endif

                string assemblyName = assembly.GetName().Name;

#if UNITY_EDITOR || !NETFX_CORE
                bool ignoreAssembly = false;
                for (int i = 0; i < ignoredAssemblies.Length; i++)
                {
                    if (caseInsensitiveComparer.IsPrefix(assemblyName, ignoredAssemblies[i], CompareOptions.IgnoreCase))
                    {
                        ignoreAssembly = true;
                        break;
                    }
                }

                if (ignoreAssembly)
                    continue;
#endif
                assemlbyResults.Add(assembly);


                //foreach (Type type in assembly.GetExportedTypes())
                


            }
            return assemlbyResults;
        }
    }
}
