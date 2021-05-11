using ExceptionSoftware.ExEditor;
using UnityEditor;
using UnityEngine;

namespace ExceptionSoftware.Variables
{
    public class VariableFactoryWindow : ExWindow<VariableFactoryWindow>
    {
        [MenuItem(VariablesUtilityEditor.VARIABLES_MENU_ITEM + "Factory", false, 3000)]
        static void OpenWindow()
        {
            VariableFactoryWindow w = VariableFactoryWindow.GetWindow<VariableFactoryWindow>();
            w.position = new Rect(new Vector2(200, 200), new Vector2(800, 800));
            w.Open();
        }


        public override string GetTitle() { return "Variable factory"; }
        protected override void DoEnable() { }
        protected override void DoRecompile() { DoEnable(); }
        protected override void DoResize() { DoEnable(); }

        #region Layout

        Rect _rtoolbar, _rhelp, _rtable, _rtableFilter;

        protected override void DoLayout()
        {
            Rect[] lv = base.position.CopyToZero().Split(SplitMode.Vertical, 20, 60, -1);
            Rect[] lh = lv[2].Split(SplitMode.Horizontal, .5f, -1);

            _rtoolbar = lv[0];
            _rhelp = lv[1];
            _rtable = lh[0];
            _rtableFilter = lh[1];
        }

        #endregion
        public override void DoGUI()
        {
            DoToolbar(_rtoolbar);

            DoHelp(_rhelp);

            DoBody(_rtable, _rtableFilter);
        }

        void DoToolbar(Rect rect)
        {
            GUILayout.BeginArea(rect, EditorStyles.toolbar);
            {
                GUILayout.BeginHorizontal();
                {

                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                    {
                        _textLeft = _textRight = string.Empty;
                    }

                    GUILayout.FlexibleSpace();

                    GUI.enabled = _textRight.Trim() != string.Empty;
                    if (GUILayout.Button("Create", EditorStyles.toolbarButton))
                    {
                        VariablesUtilityEditor.CreateNewVariables(_textRight);
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        string _textLeft;
        string _textRight;
        void DoHelp(Rect rect)
        {
            GUILayout.BeginArea(rect);
            {
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                EditorGUILayout.HelpBox("En la columna de la izquierda hay que meter la lista de nombres de variables separador por INTRO. A la derecha apareceran los nombres procesados", MessageType.Info, true);
                GUILayout.FlexibleSpace();

                GUILayout.EndVertical();
            }
            GUILayout.EndArea();
            //EditorGUI.LabelField(rect, "Wololo\ndsada", EditorStyles.helpBox);
        }
        void DoBody(Rect rectLeft, Rect rectRight)
        {
            EditorGUI.BeginChangeCheck();
            _textLeft = EditorGUI.TextArea(rectLeft, _textLeft);
            if (EditorGUI.EndChangeCheck())
            {
                _textRight = VariablesUtilityEditor.ProcessVariableFactoryInputVariables(_textLeft);
            }

            GUI.enabled = false;
            EditorGUI.TextArea(rectRight, _textRight);
            GUI.enabled = true;
        }

        void CreateVariables()
        {

        }
    }
}
