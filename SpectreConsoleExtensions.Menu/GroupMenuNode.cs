namespace Rdm.Spectre.Console.Extensions.Menu
{
    public sealed class GroupMenuNode : MenuNode
    {
        public GroupMenuNode(string title) : base(title)
        {
        }

        public GroupMenuNode(string title, bool isGroup, params MenuNode[] childOptions) : base(title)
        {
            ChildOptions = childOptions.ToList();
            RenderChildrenAsGroupItems = isGroup;
        }

        public List<MenuNode> ChildOptions { get; set; }
        public bool RenderChildrenAsGroupItems { get; set; }
        public bool IsChildMultiSelect { get; set; }
    }
}