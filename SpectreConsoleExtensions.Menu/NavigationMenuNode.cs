namespace SpectreConsoleExtensions.Menu
{
    public sealed class NavigationMenuNode : MenuNode
    {
        public NavigationMenuNode(string title, GroupMenuNode navigateTo, Action<MenuContext>? configureContext = null) : base(title)
        {
            NavigateTo = navigateTo;
            ConfigureContext = configureContext;
        }

        public GroupMenuNode NavigateTo { get; set; }

        public Action<MenuContext>? ConfigureContext { get; set; }
    }
}