namespace Chemistry
{
    public enum Element : byte
    { H = 1, C = 6, N, O, F, S = 16, Cl, Br = 35, I = 53 }

    public static class Elements
    {
        public static int Valence(int electrons)
        {
            if (electrons > 2) electrons -= 2;
            if (electrons > 8) electrons -= 8;
            if (electrons > 8) electrons -= 8;
            if (electrons > 8) electrons -= 18;
            if (electrons > 8) electrons -= 18;
            return 8 - electrons;
        }

        public static int Valence(Element e)
        {
            return Valence((int)e);
        }

        public static Element FromSymbol(string s)
        {
            return (Element)System.Enum.Parse(typeof(Element), s);
        }
    }
}