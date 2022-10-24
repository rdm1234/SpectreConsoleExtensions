namespace Rdm.Spectre.Console.Extensions.Menu
{
    public abstract class MenuNode
    {
        internal MenuNode(string title)
        {
            Title = title;
        }
        
        public string Title { get; set; }

        public Func<bool>? DisabledPredicate { get; set; }
        
        public bool? RequiresConfirmation { get; set; }
        public bool? ShowActionsProgress { get; set; }

        public static ReservedMenuNode Back { get; } = new ReservedMenuNode("Back");
        public static ReservedMenuNode Exit { get; } = new ReservedMenuNode("Exit");

        internal async Task HandleAllExecutedAsync()
        {
            OnAfterExecuted?.Invoke();

            if (OnAfterExecutedAsync != null)
                await OnAfterExecutedAsync();
        }
        
        public event Action OnAfterExecuted;
        public event Func<Task> OnAfterExecutedAsync;
        
        internal MenuContext Context { get; set; }
    }
}