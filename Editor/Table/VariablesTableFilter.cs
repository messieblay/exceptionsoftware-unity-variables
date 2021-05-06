namespace ExceptionSoftware.Variables
{
    public class VariablesTableFilter
    {
        public string textFilter = "";

        public bool HasFilter => textFilter != string.Empty;
    }
}
