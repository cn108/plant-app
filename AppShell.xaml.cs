namespace FinalYearProject
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            var icons = new Dictionary<string, string>
            {
                ["Dashboard"] = Icons.Home, ["Profile"] = Icons.Person, ["Fruits"] = Icons.Food,
                ["Vegetables"] = Icons.Grass, ["Weather"] = Icons.Sun, ["Live Chat Support"] = Icons.Chat,
                ["Plant Care Tips"] = Icons.Tips, ["Crop Rotation Planner"] = Icons.Rotate, ["Forum"] = Icons.Forum,
                ["Add Plants"] = Icons.Add, ["Scan Plants"] = Icons.Camera, ["View Added Plants"] = Icons.List,
                ["Plant Care Timeline"] = Icons.Timeline, ["Logout"] = Icons.Logout
            };

            foreach (var item in Items)
            {
                if (icons.TryGetValue(item.Title ?? string.Empty, out var glyph))
                {
                    item.FlyoutIcon = Icons.Source(glyph, "Green3", 22);
                }
            }
        }
    }
}
