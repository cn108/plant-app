namespace FinalYearProject
{
    // Material Icons codepoints used across the app.
    public static class Icons
    {
        public const string Font = "MaterialIcons";
        public const string Home = "\ue88a";
        public const string Eco = "\uea35";
        public const string Flower = "\ue545";
        public const string Sun = "\ue430";
        public const string Cloud = "\ue2bd";
        public const string Chat = "\ue0b7";
        public const string Tips = "\ue0f0";
        public const string Rotate = "\ue863";
        public const string Forum = "\ue0bf";
        public const string Add = "\ue147";
        public const string Camera = "\ue412";
        public const string List = "\ue896";
        public const string Timeline = "\ue922";
        public const string Logout = "\ue9ba";
        public const string Person = "\ue7fd";
        public const string Water = "\ue798";
        public const string Check = "\ue86c";
        public const string Arrow = "\ue5c8";
        public const string Thermo = "\uf076";
        public const string Spa = "\ueb4c";
        public const string Food = "\ue56c";
        public const string Grass = "\uf205";
        public const string Report = "\ue160";
        public const string Reply = "\ue15e";
        public const string Send = "\ue163";
        public const string Expand = "\ue5cf";
        public const string Close = "\ue5cd";

        public static FontImageSource Source(string glyph, string colorKey = "Green2", double size = 22) =>
            new()
            {
                FontFamily = Font,
                Glyph = glyph,
                Size = size,
                Color = Application.Current?.Resources.TryGetValue(colorKey, out var c) == true && c is Color color
                    ? color
                    : Colors.Black
            };
    }
}
