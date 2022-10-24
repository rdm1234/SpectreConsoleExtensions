namespace SpectreConsoleExtensions.Menu
{
    public record MenuContext
    {
        internal MenuContext(MenuNode relatedNode, MenuContext parentContext = null, bool? requiresConfirmation = null, bool? showActionsProgress = null)
        {
            RelatedNode = relatedNode;
            ParentContext = parentContext;
            RequiersConfirmation = requiresConfirmation ?? parentContext?.RequiersConfirmation ?? false;
            ShowActionsProgress = showActionsProgress ?? parentContext?.ShowActionsProgress ?? false;
        }

        public MenuNode RelatedNode { get; }
        public MenuContext ParentContext { get; }

        public bool RequiersConfirmation { get; set; }
        public bool ShowActionsProgress { get; set; }
    }
}