using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExceptionSoftware.Variables
{
    public class VariablesUtility
    {
        static VariablesSettingsAsset _settings = null;
        public static VariablesSettingsAsset Settings => LoadAsset();
        internal static VariablesSettingsAsset LoadAsset()
        {
            if (_settings == null)
            {
                _settings = ExAssets.FindAssetsByType<VariablesSettingsAsset>().FirstOrDefault();
            }

            if (_settings == null)
            {
                _settings = Resources.FindObjectsOfTypeAll<VariablesSettingsAsset>().FirstOrDefault();
            }

            return _settings;
        }

        public static VariablesSerialization GetVariablesSerialized()
        {
            VariablesSerialization variables = new VariablesSerialization();
            List<VariableSerialized> list = new List<VariableSerialized>();

            variables.variables = list;
            foreach (var v in Settings.variables)
            {
                list.Add(v.GetSerialized());
            }
            return variables;
        }

        public static void LoadSerialization(VariablesSerialization variables)
        {
            foreach (var v in variables.variables)
            {
                _settings.variables.Find(s => s.name == v.name)?.LoadSerializedValue(v.value);
            }
        }

    }
}
