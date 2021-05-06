using UnityEngine;

namespace ExceptionSoftware.Variables
{
    [System.Serializable]
    public class VariableValue
    {

        [SerializeField] string _value = string.Empty;
        [SerializeField] int _valueInt = 0;
        [SerializeField] float _valueFloat = 0;
        [SerializeField] bool _valueBool = false;

        public string Value
        {
            get => _value;
            set
            {
                if (value == null) value = string.Empty;
                _value = value.ToString();
                if (!int.TryParse(value, out _valueInt)) { _valueInt = 0; }
                if (!float.TryParse(value, out _valueFloat)) { _valueFloat = 0; }
                if (!bool.TryParse(value, out _valueBool)) { _valueBool = false; }
            }
        }

        public int ValueInt
        {
            get => _valueInt;
            set
            {
                _value = value.ToString();
                _valueFloat = _valueInt = value;
                _valueBool = value >= 1f ? true : false;
            }
        }

        public float ValueFloat
        {
            get => _valueFloat;
            set
            {
                _value = value.ToString();
                _valueInt = Mathf.RoundToInt(value);
                _valueFloat = value;
                _valueBool = value >= 1f ? true : false;
            }
        }

        public bool ValueBool
        {
            get => _valueBool;
            set
            {
                _value = value.ToString();
                _valueFloat = _valueInt = value ? 1 : 0;
                _valueBool = value;
            }
        }


        public static implicit operator string(VariableValue d) => d.Value;
        public static implicit operator int(VariableValue d) => d.ValueInt;
        public static implicit operator float(VariableValue d) => d.ValueInt;
        public static implicit operator bool(VariableValue d) => d.ValueBool;
        public override string ToString() => $"{_value}";
    }
}
