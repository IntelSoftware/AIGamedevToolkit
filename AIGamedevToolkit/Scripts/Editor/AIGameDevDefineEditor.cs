using UnityEditor;
using UnityEditor.Compilation;

namespace AIGamedevToolkit
{
    /// <summary>
    /// Injects GAIA_PRESENT define into project
    /// </summary>
    [InitializeOnLoad]
    public class AIGameDevDefineEditor : Editor
    {
        static AIGameDevDefineEditor()
        {

            bool updateScripting = false;
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            
            // always add AIGAMEDEV define, that is what this package is
            Utils.AddDefine(ref symbols, "AIGAMEDEV", ref updateScripting);
            
            if (BarracudaPackageCheck())
            {
                Utils.AddDefine(ref symbols,"AIGAMEDEV_BARRACUDA", ref updateScripting);
            }
            else
            {
                Utils.RemoveDefine(ref symbols, "AIGAMEDEV_BARRACUDA", ref updateScripting);
            }

            if (PlayerSettings.allowUnsafeCode)
            {
                Utils.AddDefine(ref symbols, "AIGAMEDEV_UNSAFE", ref updateScripting);
            }
            else
            {
                Utils.RemoveDefine(ref symbols, "AIGAMEDEV_UNSAFE", ref updateScripting);
            }

            if (updateScripting)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
        }

       
        /// <summary>
        /// Checks if the barracuda package is installed via reflection
        /// </summary>
        /// <returns></returns>
        public static bool BarracudaPackageCheck()
        {
            //Look for assembly
            var assemblies = CompilationPipeline.GetAssemblies();
            foreach (UnityEditor.Compilation.Assembly assembly in assemblies)
            {
                if (assembly.name.Contains("Unity.Barracuda"))
                {
                    //was found -> we are done
                    return true;
                }
            }
            return false;
        }

    }
}