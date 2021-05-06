
using ExceptionSoftware.TreeViewTemplate;
using System;


namespace ExceptionSoftware.Variables
{

    [Serializable]
    public class VariablesElement : TreeElement
    {
        [UnityEngine.SerializeField] public Variable variable = null;

        public VariablesElement(string name, int depth, int id) : base(name, depth, id)
        {
        }
    }
}
