using System.Collections.Generic;
using System.Linq;

namespace ExceptionSoftware.Variables
{

    static class VariablesTableGenerator
    {
        static int IDCounter;
        static bool _dataloaded = false;
        public static List<VariablesElement> GenerateTree()
        {
            IDCounter = 0;
            VariablesElement root = new VariablesElement("Root", -1, IDCounter);
            List<VariablesElement> roots = new List<VariablesElement>();
            roots.Add(root);
            GenerateItems(roots);
            return roots;
        }

        static List<VariablesElement> GenerateItems(List<VariablesElement> roots)
        {
            VariablesSettingsAsset _idb = VariablesUtilityEditor.Settings;
            foreach (Variable variable in _idb.variables.OrderBy(s => s.group).ThenBy(s => s.name))
            {
                roots.Add(CreateItem(variable, 0));
            }
            return roots;
        }

        static VariablesElement _variables = null;
        static VariablesElement CreateItem(Variable item, int depth)
        {
            _variables = new VariablesElement(item.name, depth, ++IDCounter);
            _variables.variable = item;

            return _variables;
        }

    }
}
