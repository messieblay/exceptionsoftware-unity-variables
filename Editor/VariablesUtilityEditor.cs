using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ExceptionSoftware.Variables
{
    [InitializeOnLoad]
    public static class VariablesUtilityEditor
    {
        static VariablesSettingsAsset _settings = null;
        public static VariablesSettingsAsset Settings => LoadOrCreate();

        public const string VARIABLES_PATH = ExConstants.SETTINGS_PATH + "Variables/";
        public const string VARIABLES_PATH_RESOURCES = VARIABLES_PATH + "Resources/";
        public const string VARIABLES_MENU_ITEM = "Tools/Variables/";

        public const string VARIABLES_SETTINGS_FILENAME = "ExVariablesSettings";

        static VariablesUtilityEditor() => LoadOrCreate();

        internal static VariablesSettingsAsset LoadOrCreate()
        {
            if (!System.IO.Directory.Exists(VARIABLES_PATH))
                System.IO.Directory.CreateDirectory(VARIABLES_PATH);

            if (!System.IO.Directory.Exists(VARIABLES_PATH_RESOURCES))
                System.IO.Directory.CreateDirectory(VARIABLES_PATH_RESOURCES);

            if (_settings == null)
            {
                _settings = ExAssets.FindAssetsByType<VariablesSettingsAsset>().First();
            }

            if (_settings == null)
            {
                _settings = Resources.FindObjectsOfTypeAll<VariablesSettingsAsset>().FirstOrDefault();
            }

            if (_settings == null)
            {
                _settings = ExAssets.CreateAsset<VariablesSettingsAsset>(VARIABLES_PATH_RESOURCES, VARIABLES_SETTINGS_FILENAME, true, true);
            }

            if (_settings)
            {
                _settings.variables = ExAssets.FindAssetsByType<Variable>();
            }

            return _settings;
        }

        [MenuItem(VARIABLES_MENU_ITEM + "Settings", priority = ExConstants.MENU_ITEM_PRIORITY)]
        static void SelectAsset()
        {
            LoadOrCreate();
            Selection.activeObject = _settings;
        }

        public static System.Action onNewVariablesCreated = null;


        public static void RenameAsset(ScriptableObject scriptable, string newname)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(scriptable);
            UnityEditor.AssetDatabase.RenameAsset(path, newname);
        }


        public static string ProcessVariableFactoryInputVariables(string variables)
        {
            if (variables == null || variables.Trim() == string.Empty) return string.Empty;

            string[] vars = variables.Split('\n');
            string result = string.Empty;

            foreach (var v in vars)
            {
                if (v == string.Empty) continue;
                if (v.Trim() == string.Empty) continue;

                result += ProcessVariable(v) + "\n";
            }
            return result;

            string ProcessVariable(string variable) => variable.Trim().QuitAccentsAndN().Replace(" ", "_").ToUpper();
        }

        public static void CreateNewVariables(string variables)
        {
            if (variables == null || variables.Trim() == string.Empty) return;

            string[] vars = variables.Split('\n');
            string log = string.Empty;
            foreach (var v in vars)
            {
                if (v == null || v == string.Empty) continue;
                log += CreateVariable(v);
            }

            Debug.Log(log);
            AssetDatabase.Refresh();
            if (onNewVariablesCreated != null) onNewVariablesCreated();
        }

        public static string CreateVariable(string variableName)
        {
            if (_settings.variables.Exists(s => s.name == variableName))
            {
                return $"[Variable] {variableName} Creation failed: Already exist\n";
            }

            ExAssets.CreateAsset<Variable>(VARIABLES_PATH, variableName);
            return $"[Variable] {variableName} Created\n";
        }
    }
}
