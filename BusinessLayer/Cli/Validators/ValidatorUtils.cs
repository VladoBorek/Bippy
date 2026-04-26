namespace BusinessLayer.Cli.Validators
{
    public static class ValidatorUtils
    {
        public static bool IsHex(string s)
        {
            return !string.IsNullOrEmpty(s)
                && s.All(c => c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F');
        }

        public static bool IsBinary(string s)
        {
            return !string.IsNullOrEmpty(s) && s.All(c => c == '0' || c == '1');
        }
    }
}
