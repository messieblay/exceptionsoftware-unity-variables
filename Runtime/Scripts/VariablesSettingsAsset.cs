using System.Collections.Generic;
using UnityEngine;

namespace ExceptionSoftware.Variables
{
    [System.Serializable]
    public class VariablesSettingsAsset : SettingsAsset
    {
        [SerializeField] public List<Variable> variables = new List<Variable>();
    }
}
