using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ExceptionSoftware.Variables
{
    public class VariablesFetch
    {


        [MenuItem("Tools/Variables/Manager", priority = 3000)]
        public static VariablesWindow GetWindow()
        {
            var window = EditorWindow.GetWindow<VariablesWindow>();
            window.titleContent = new GUIContent("Variables");
            window.Focus();
            window.Repaint();
            return window;
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var Inputdb = EditorUtility.InstanceIDToObject(instanceID) as VariablesSettingsAsset;
            if (Inputdb != null)
            {
                VariablesWindow window = GetWindow();
                window.SetTreeAsset(Inputdb);
                return true;
            }
            return false; // we did not handle the open
        }

    }
}
