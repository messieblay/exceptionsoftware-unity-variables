using System.Xml.Serialization;
using UnityEngine;
namespace ExceptionSoftware.Variables
{
    [System.Serializable, CreateAssetMenu(fileName = "Variable")]
    public class Variable : ScriptableObject
    {
        public enum VariableType
        {
            String, Int, Float, Bool
        }
        [SerializeField] [XmlAttribute("Type")] VariableType _type = VariableType.String;
        [SerializeField] VariableValue _defaultValue = new VariableValue();
        [SerializeField] VariableValue _inGameValue = new VariableValue();
        [SerializeField] public string description = string.Empty;
        [SerializeField] public string group = string.Empty;

        [XmlAttribute]
        public VariableType Type
        {
            get => _type;
            set => _type = value;
        }

        public VariableValue DefaultValue { get => _defaultValue; set => _defaultValue = value; }
        public VariableValue InGameValue { get => _inGameValue; set => _inGameValue = value; }

        public string Value
        {
            get => _inGameValue.Value;
            set
            {
                this._inGameValue.Value = value;
                ThrowChange();
            }
        }
        public int ValueInt
        {
            get => _inGameValue.ValueInt;
            set
            {
                this._inGameValue.ValueInt = value;
                ThrowChange();
            }
        }
        public float ValueFloat
        {
            get => _inGameValue.ValueFloat;
            set
            {
                this._inGameValue.ValueFloat = value;
                ThrowChange();
            }
        }
        public bool ValueBool
        {
            get => _inGameValue.ValueBool;
            set
            {
                this._inGameValue.ValueBool = value;
                ThrowChange();
            }
        }

        public void SetDefaultValue(object obj) => _defaultValue.Value = obj.ToString();


        System.Action<Variable> _onChange = null;
        public void Subscribe(System.Action<Variable> del)
        {
            _onChange -= del;
            _onChange += del;
        }

        public void Unsubscribe(System.Action<Variable> del)
        {
            _onChange -= del;
        }

        void ThrowChange()
        {
            if (_onChange == null) return;

            System.Delegate[] invocationList = _onChange.GetInvocationList();
            foreach (System.Delegate inv in invocationList)
            {
                if (inv == null)
                {
                    _onChange -= (System.Action<Variable>)inv;
                    continue;
                }

                try
                {
                    inv.DynamicInvoke(this);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[{name}] Error capturado en la invocacion onChange");
                    Debug.LogException(ex);
                }
            }
        }

        public override string ToString() => $"{_inGameValue}";


        #region Serialization
        public void LoadSerializedValue(string value)
        {
            _inGameValue.Value = value;
        }
        public VariableSerialized GetSerialized() => new VariableSerialized(this);
        #endregion
    }
}
