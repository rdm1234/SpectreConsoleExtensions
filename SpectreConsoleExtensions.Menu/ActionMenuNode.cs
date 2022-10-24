namespace Rdm.Spectre.Console.Extensions.Menu
{
    public sealed class ActionMenuNode : MenuNode
    {
        public ActionMenuNode(string title, Func<Task> action) : base(title)
        {
            AsyncAction = action;
        }

        public ActionMenuNode(string title, Action action) : base(title)
        {
            SyncAction = action;
        }

        public Func<Task>? AsyncAction { get; }
        public Action? SyncAction { get; }
    }
}