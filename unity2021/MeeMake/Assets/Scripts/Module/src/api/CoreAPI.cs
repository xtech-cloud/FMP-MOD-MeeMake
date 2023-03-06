namespace MeeX.MeeMake
{
    public class CoreAPI
    {
        public string GetCurrentLanguage()
        {
            return "en_US";
        }

        public bool StringToBool(string _string, bool _default)
        {
            bool value = false;
            if (bool.TryParse(_string, out value))
                value = _default;
            return value;
        }

        public int StringToInt(string _string, int _default)
        {
            int value = 0;
            if (int.TryParse(_string, out value))
                value = _default;
            return value;
        }

        public float StringToFloat(string _string, float _default)
        {
            float value = 0;
            if (float.TryParse(_string, out value))
                value = _default;
            return value;
        }
    }
}