using UnityEditor;
using UnityEngine;

namespace ExceptionSoftware.Variables
{
    [CustomEditor(typeof(Variable))]
    [CanEditMultipleObjects]
    public class VariableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Variable gamevar = (Variable)target;

            gamevar.Type = (Variable.VariableType)EditorGUILayout.EnumPopup(gamevar.Type);

            DrawSerializedVariable("Default", gamevar.Type, gamevar.DefaultValue);

            GUI.enabled = Application.isPlaying;
            {
                DrawSerializedVariable("In Game", gamevar.Type, gamevar.InGameValue);
            }
            GUI.enabled = true;
        }

        VariableValue DrawSerializedVariable(string label, Variable.VariableType type, VariableValue gamevar)
        {
            switch (type)
            {
                case Variable.VariableType.String:
                    gamevar.Value = EditorGUILayout.DelayedTextField(label, gamevar.Value);
                    break;
                case Variable.VariableType.Int:
                    gamevar.ValueInt = EditorGUILayout.DelayedIntField(label, gamevar.ValueInt);
                    break;
                case Variable.VariableType.Float:
                    gamevar.ValueFloat = EditorGUILayout.DelayedFloatField(label, gamevar.ValueFloat);
                    break;
                case Variable.VariableType.Bool:
                    gamevar.ValueBool = EditorGUILayout.Toggle(label, gamevar.ValueBool);
                    break;
            }
            return gamevar;
        }


        public static VariableValue DrawSerializedVariable(Rect rect, Variable.VariableType type, VariableValue gamevar)
        {
            switch (type)
            {
                case Variable.VariableType.String:
                    gamevar.Value = EditorGUI.DelayedTextField(rect, gamevar.Value);
                    break;
                case Variable.VariableType.Int:
                    gamevar.ValueInt = EditorGUI.IntField(rect, gamevar.ValueInt);
                    break;
                case Variable.VariableType.Float:
                    gamevar.ValueFloat = EditorGUI.FloatField(rect, gamevar.ValueFloat);
                    break;
                case Variable.VariableType.Bool:
                    gamevar.ValueBool = EditorGUI.Toggle(rect, gamevar.ValueBool);
                    break;
            }
            return gamevar;
        }
    }
}
