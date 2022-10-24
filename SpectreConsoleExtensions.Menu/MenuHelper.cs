using Spectre.Console;
using System.Xml.Linq;

namespace SpectreConsoleExtensions.Menu
{
    public static class MenuHelper
    {
        public static async Task SelectAndRunAsync(GroupMenuNode node)
        {
            MenuValidationHelper.Validate(node);
            node.Context = new(node);
            await RecursiveSelectAndRunAsync(node);
        }

        private static async Task RecursiveSelectAndRunAsync(GroupMenuNode groupNode)
        {
            foreach (var childNode in groupNode.ChildOptions)
            {
                childNode.Context = BuildContext(childNode, groupNode.Context);

                if (childNode is GroupMenuNode childGroup && childGroup.RenderChildrenAsGroupItems) 
                {
                    foreach (var childChildNode in childGroup.ChildOptions)
                    {
                        childChildNode.Context = BuildContext(childChildNode, childGroup.Context);
                    }
                }
            }

            if (!groupNode.IsChildMultiSelect)
            {
                await RecursiveSelectAndRunForSignleSelectAsync(groupNode);
            }
            else
            {
                await RecursiveSelectAndRunForMultiSelectAsync(groupNode);
            }
        }

        private static async Task RecursiveSelectAndRunForMultiSelectAsync(GroupMenuNode groupNode)
        {
            foreach (var childNode in groupNode.ChildOptions)
            {
                childNode.Context = BuildContext(childNode, groupNode.Context);
            }
            
            if (groupNode.DisabledPredicate?.Invoke() ?? false
                || groupNode.ChildOptions.All(x => x.DisabledPredicate?.Invoke() ?? false))
                return;
            
            var prompt = new MultiSelectionPrompt<MenuNode>()
                .Title(groupNode.Title)
                .UseConverter(node => node.Title);

            foreach (var childNode in groupNode.ChildOptions)
            {
                if (childNode.DisabledPredicate?.Invoke() ?? false)
                    continue;

                if (childNode is GroupMenuNode childGroupNode && childGroupNode.RenderChildrenAsGroupItems)
                {
                    prompt.AddChoiceGroup(childNode, childGroupNode.ChildOptions);
                }
                else
                {
                    prompt.AddChoice(childNode);
                }
            }

            var selections = AnsiConsole.Prompt(prompt);

            await ExecuteNodesAsync(selections.Cast<ActionMenuNode>().ToList());
            await groupNode.HandleAllExecutedAsync();
        }

        private static async Task RecursiveSelectAndRunForSignleSelectAsync(GroupMenuNode groupNode)
        {
            if (groupNode.DisabledPredicate?.Invoke() ?? false 
                || groupNode.ChildOptions.All(x => x.DisabledPredicate?.Invoke() ?? false))
                return;
            
            var prompt = new SelectionPrompt<MenuNode>()
                .Title(groupNode.Title)
                .UseConverter(node => node.Title);

            foreach (var childNode in groupNode.ChildOptions)
            {
                if (childNode.DisabledPredicate?.Invoke() ?? false)
                    continue;
                
                if (childNode is GroupMenuNode childGroupNode && childGroupNode.RenderChildrenAsGroupItems)
                {
                    prompt.AddChoiceGroup(childNode, childGroupNode.ChildOptions);
                }
                else
                {
                    prompt.AddChoice(childNode);
                }
            }

            var selection = AnsiConsole.Prompt(prompt);

            await ExecuteNodeAsync(selection);
            await groupNode.HandleAllExecutedAsync();
        }

        private static async Task ExecuteNodeAsync(MenuNode node)
        {
            var context = node.Context;
            
            if (node is GroupMenuNode groupNode)
            {
                await RecursiveSelectAndRunAsync(groupNode);
                return;
            }

            if (node is ReservedMenuNode)
            {
                if (node == MenuNode.Exit)
                {
                    await ExitAsync(node);
                    return;
                }

                if (node == MenuNode.Back)
                {
                    await ReturnBackAsync(node);
                    return;
                }
            }

            if (node is NavigationMenuNode navNode)
            {
                navNode.ConfigureContext?.Invoke(context);

                await RecursiveSelectAndRunAsync(navNode.NavigateTo);
                return;
            }

            var actionNode = (ActionMenuNode)node;

            if (context.RequiersConfirmation)
            {
                if (!AnsiConsole.Confirm($"[yellow]Are your sure you want to execute selected action({actionNode.Title.EscapeMarkup()})?[/]"))
                {
                    await ReturnBackAsync(node);
                    return;
                }
            }

            if (context.ShowActionsProgress)
            {
                await AnsiConsole.Status().StartAsync($"Executing {actionNode.Title}", ctx => ExecuteNodeActionAsync(actionNode));
            }
            else
            {
                await ExecuteNodeActionAsync(actionNode);
            }
        }

        private static async Task ExitAsync(MenuNode node)
        {
            await node.HandleAllExecutedAsync();
        }

        private static MenuContext BuildContext(MenuNode node, MenuContext parentContext)
        {
            return new MenuContext(node, parentContext, node.RequiresConfirmation ?? parentContext.RequiersConfirmation, node.ShowActionsProgress ?? parentContext.ShowActionsProgress);
        }

        private static async Task ReturnBackAsync(MenuNode node)
        {
            var targetContext = node.Context.ParentContext.ParentContext;
            var targetGroup = (GroupMenuNode)targetContext.RelatedNode;

            if (targetGroup.RenderChildrenAsGroupItems)
            {
                targetContext = targetContext.ParentContext;
                targetGroup = (GroupMenuNode)targetContext.RelatedNode;
            }

            await RecursiveSelectAndRunAsync(targetGroup);
            await node.HandleAllExecutedAsync();
        }

        private static async Task ExecuteNodesAsync(List<ActionMenuNode> nodes)
        {
            bool globalConfirmationUsed = nodes.All(x => x.Context.RequiersConfirmation);

            if (globalConfirmationUsed)
            {
                AnsiConsole.MarkupLine("You've selected next actions:");

                foreach (var selection in nodes)
                {
                    AnsiConsole.MarkupLine($"- [cyan]{selection.Title}[/];");
                }

                if (!AnsiConsole.Confirm($"[yellow]Are you sure you want execute them all?[/]"))
                    return;
            }

            if (!nodes.All(x => !x.Context.ShowActionsProgress))
            {
                foreach (var selection in nodes)
                {
                    await ExecuteNodeAsync(selection);
                    await selection.HandleAllExecutedAsync();
                }
            }
            else
            {
                await AnsiConsole.Status()
                    .StartAsync("Preparing to execute selected actions...", async (ctx) =>
                    {
                        foreach (var selection in nodes)
                        {
                            ctx.Status($"Executing '{selection.Title}'");
                            await ExecuteNodeAsync(selection);
                            await selection.HandleAllExecutedAsync();
                        }
                    });
            }
        }

        private static async Task ExecuteNodeActionAsync(ActionMenuNode actionNode)
        {
            actionNode.SyncAction?.Invoke();

            if (actionNode.AsyncAction != null)
            {
                await actionNode.AsyncAction();
            }
            
            await actionNode.HandleAllExecutedAsync();
        }

    }
}
