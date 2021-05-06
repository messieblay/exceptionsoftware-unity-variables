using System.Xml.Serialization;

namespace ExceptionSoftware.Variables
{
    [System.Serializable]
    public class VariableSerialized
    {
        [XmlAttribute] public string name;
        [XmlAttribute] public string value;
        public VariableSerialized()
        {
        }

        public VariableSerialized(Variable var)
        {
            this.name = var.name;
            this.value = var.InGameValue.Value;
        }
    }
}
