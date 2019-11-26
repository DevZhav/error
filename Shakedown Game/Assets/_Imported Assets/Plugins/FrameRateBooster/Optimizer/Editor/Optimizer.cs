using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEditor.Callbacks;
using Debug = UnityEngine.Debug;

namespace ToolBuddy.FrameRateBooster.Optimizer
{

    public static class Optimizer
    {

        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.Android)
            {
                Debug.LogWarning("[Frame Rate Booster] Automatic optimization of Android builds is not supported");
                Debug.Log("[Frame Rate Booster] You can still optimize it manually, by unpacking the apk file, run the Optimize method on the content of the unpacked apk (specifically the assets\\bin\\Data folder) and then repack the apk. I personally have 0 experience making android apk files, so I can't help much more with this subject.");
                return;
            }

            if (PlayerSettings.GetScriptingBackend(BuildPipeline.GetBuildTargetGroup(target)) == ScriptingImplementation.IL2CPP)
            {
                Debug.LogWarning("[Frame Rate Booster] Automatic optimization of IL2CPP builds is not supported");
                Debug.Log("[Frame Rate Booster] You can still optimize it manually, by locating the Mono assemblies used by IL2CPP, running the Optimize method on them, and running the IL2CPP command on those modified assemblies. I have never done that, but in theory it is feasible. But before going through the trouble of making it work, just switch your project to Mono scripting backend, and test building with and without Frame Rate Booster to see if it actually noticeably enhances your performance.");
                return;
            }


#if UNITY_2017_3_OR_NEWER
            const bool optimizationInOwnAssembly = true;
#else
            const bool optimizationInOwnAssembly = false;
#endif

#if UNITY_2017_2_OR_NEWER
            const string targetAssemblyName = "UnityEngine.CoreModule.dll";
#else
            const string targetAssemblyName = "UnityEngine.dll";
#endif


            Debug.Log("[Frame Rate Booster] Started post build optimization");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string path = Path.GetFullPath(pathToBuiltProject);
            string buildDirectory = Path.GetDirectoryName(path);

            string dataDirectory;
            {
                string[] directories = Directory.GetDirectories(buildDirectory, "*Data");
                if (directories.Length == 1)
                    dataDirectory = directories[0];
                else if (directories.Length == 0)
                {
                    Debug.LogError(String.Format("[Frame Rate Booster] Couldn't locate the Data folder in {0}", buildDirectory));
                    return;
                }
                else
                {
                    Debug.LogError(String.Format("[Frame Rate Booster] Detected multiple Data folders in {0}. Please select an folder with no other build in it as your build target folder.", buildDirectory));
                    return;
                }
            }

            string[] allAssembliesPaths = Directory.GetFiles(dataDirectory, "*.dll", SearchOption.AllDirectories);
            string optimizationsAssemblyPath;
            {
                IEnumerable<string> optimizedAssemblies = allAssembliesPaths.Where(s => s.Contains(optimizationInOwnAssembly
                     ? "FrameRateBooster.Optimizations"
                     : "Assembly-CSharp-firstpass.dll"));
                if (optimizedAssemblies.Count() == 1)
                    optimizationsAssemblyPath = optimizedAssemblies.First();
                else
                {
                    Debug.LogError("[Frame Rate Booster] Couldn't locate the assembly containing the optimizations");
                    return;
                }
            }

            string targetAssemblyPath;
            {
                IEnumerable<string> assembliesToOptimize = allAssembliesPaths.Where(s => s.Contains(targetAssemblyName));
                if (assembliesToOptimize.Count() == 1)
                    targetAssemblyPath = assembliesToOptimize.First();
                else
                {
                    Debug.LogError("[Frame Rate Booster] Couldn't locate the assembly to optimize");
                    return;
                }
            }

            Optimize(optimizationsAssemblyPath, targetAssemblyPath, optimizationInOwnAssembly, !optimizationInOwnAssembly);

            stopWatch.Stop();
            Debug.Log("[Frame Rate Booster] Finished post build optimization. Operation took " + stopWatch.ElapsedMilliseconds + " milliseconds");

        }
        /// <summary>
        /// Replaces non optimized Unity operators (in target assembly) with optimized ones (from optimizations assembly)
        /// </summary>
        /// <param name="optimizationsAssemblyPath">The path to the assembly containing the optimized version of Unity's opertors</param>
        /// <param name="targetAssemblyPath">The path of the assembly to apply the optimizations on</param>
        /// <param name="deleteOptimizationsAssembly">Should the optimizations assembly be deleted after the optimization process is finished?</param>
        /// <param name="trimOptimizationsAssembly">Should the optimized operators be removed from the optimizations module after the  optimization process is finished?</param>
        static public void Optimize(string optimizationsAssemblyPath, string targetAssemblyPath, bool deleteOptimizationsAssembly, bool trimOptimizationsAssembly)
        {
            const string optimizedNameSpace = "ToolBuddy.FrameRateBooster.Optimizations";
            const string originalNameSpace = "UnityEngine";

            ModuleDefinition optimizedModuleDefinition = ModuleDefinition.ReadModule(optimizationsAssemblyPath, new ReaderParameters() { ReadWrite = trimOptimizationsAssembly });
            ModuleDefinition originalModule = ModuleDefinition.ReadModule(targetAssemblyPath, new ReaderParameters() { ReadWrite = true });
            int optimizedMethods = 0;
            foreach (TypeDefinition optimizedType in optimizedModuleDefinition.Types)
            {
                if (optimizedType.Namespace == optimizedNameSpace)
                {
                    TypeDefinition originalType = GetOriginalTypeIfAny(originalModule, originalNameSpace, optimizedType);

                    if (originalType != null)
                    {
                        foreach (MethodDefinition optimizedMethod in optimizedType.Methods)
                        {
                            MethodDefinition method = originalType.Methods.SingleOrDefault(m => m.Name == optimizedMethod.Name && m.ReturnType.Name == optimizedMethod.ReturnType.Name && m.Parameters.Count == optimizedMethod.Parameters.Count && m.Parameters.Select(p => p.ParameterType.Name).SequenceEqual(optimizedMethod.Parameters.Select(p => p.ParameterType.Name)));


                            if (method == null)
                            {
                                Debug.LogWarning(String.Format("[Frame Rate Booster] Couldn't find match for {0}.{1}", optimizedMethod.DeclaringType, optimizedMethod.Name));
                                continue;
                            }

                            method.Body.Variables.Clear();
                            foreach (VariableDefinition variable in optimizedMethod.Body.Variables)
                            {
                                if (variable.VariableType.Namespace == optimizedNameSpace)
                                    variable.VariableType = GetOriginalType(originalModule, originalNameSpace, variable.VariableType);

                                method.Body.Variables.Add(variable);
                            }

                            method.Body.MaxStackSize = optimizedMethod.Body.MaxStackSize;

                            method.Body.Instructions.Clear();
                            foreach (Instruction instruction in optimizedMethod.Body.Instructions)
                            {
                                Instruction newInstruction;

                                FieldReference fieldReference = (instruction.Operand as FieldReference);
                                MethodReference methodReference = (instruction.Operand as MethodReference);
                                if (fieldReference != null && fieldReference.DeclaringType.Namespace == optimizedNameSpace)
                                    newInstruction = Instruction.Create(instruction.OpCode, new FieldReference(fieldReference.Name, fieldReference.FieldType, GetOriginalType(originalModule, originalNameSpace, fieldReference.DeclaringType)));
                                else if (methodReference != null)
                                {
                                    methodReference.DeclaringType = originalModule.ImportReference(methodReference.DeclaringType.Resolve());
                                    newInstruction = instruction;
                                }
                                else
                                    newInstruction = instruction;
                                method.Body.GetILProcessor().Append(newInstruction);

                                optimizedMethods++;
                            }
                        }
                    }
                }
            }

            if (optimizedMethods == 0)
                Debug.LogError("[Frame Rate Booster] Couldn't find any method to optimize. This is not supposed to happen. Please report that to the asset creator.");

            originalModule.Write();

            if (deleteOptimizationsAssembly)
            {
                optimizedModuleDefinition.Dispose();
                File.Delete(optimizationsAssemblyPath);
            }
            else if (trimOptimizationsAssembly)
            {
                List<int> indicesToRemove = new List<int>();
                for (int i = 0; i < optimizedModuleDefinition.Types.Count; i++)
                {
                    TypeDefinition optimizedType = optimizedModuleDefinition.Types[i];
                    if (optimizedType.Namespace == optimizedNameSpace)
                        indicesToRemove.Add(i);
                }

                indicesToRemove.Sort();
                while (indicesToRemove.Any())
                {
                    optimizedModuleDefinition.Types.RemoveAt(indicesToRemove.Last());
                    indicesToRemove.Remove(indicesToRemove.Last());

                }
                //TODO assure toi que la taille de assemblyCsharp ne grandit pas
                optimizedModuleDefinition.Write();
            }
        }

        static private TypeReference GetOriginalType(ModuleDefinition originalModule, string originalNameSpace, TypeReference optimizedType)
        {
            return originalModule.Types.Single(t => t.Name == optimizedType.Name && t.Namespace == originalNameSpace);

        }

        static private TypeDefinition GetOriginalTypeIfAny(ModuleDefinition originalModule, string originalNameSpace, TypeDefinition optimizedType)
        {
            return originalModule.Types.SingleOrDefault(t => t.Name == optimizedType.Name && t.Namespace == originalNameSpace);
        }

    }
}