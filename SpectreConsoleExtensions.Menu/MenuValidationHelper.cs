namespace SpectreConsoleExtensions.Menu
{
    public static class MenuValidationHelper
    {
        public static void Validate(GroupMenuNode node)
        {
            var errors = new List<MenuValidationError>();

            AddMenuNodeValidationErrors(node, errors, null, null);

            if (errors.Count > 0)
            {
                throw new MenuValidationException("Menu validation failed", errors);
            }
        }

        private static void AddMenuNodeValidationErrors(MenuNode node, List<MenuValidationError> errors, GroupMenuNode? rootGroup, GroupMenuNode? rootRootGroup)
        {
            if (node is GroupMenuNode groupNode)
            {
                AddGroupMenuNodeValidationErrors(groupNode, errors, rootGroup);
                return;
            }

            if (node is ActionMenuNode actionNode)
            {
                AddActionNodeValidationErrors(actionNode, errors, rootGroup);
                return;
            }

            if (node is NavigationMenuNode navigationNode)
            {
                AddNavigationNodeValidationErrors(navigationNode, errors, rootGroup);
                return;
            }

            var reservedNode = (ReservedMenuNode)node;
            AddReservedNodeValidationError(reservedNode, errors, rootGroup, rootRootGroup);
        }

        private static void AddReservedNodeValidationError(ReservedMenuNode reservedNode, List<MenuValidationError> errors, GroupMenuNode rootGroup, GroupMenuNode? rootRootGroup)
        {
            if (reservedNode == MenuNode.Back)
            {
                if (rootRootGroup == null)
                {
                    errors.Add(new("Back node can be used only inside a group child to other group", reservedNode, rootGroup));
                }
                else if (rootGroup.RenderChildrenAsGroupItems && rootRootGroup.RenderChildrenAsGroupItems)
                {
                    errors.Add(new("Back node can be used only inside a group child to other group with RenderChildrenAsGroupItems = false", reservedNode, rootGroup));
                }
            }
        }

        private static void AddNavigationNodeValidationErrors(NavigationMenuNode navigationNode, List<MenuValidationError> errors, GroupMenuNode? rootGroup)
        {
            if (navigationNode.NavigateTo == null)
                errors.Add(new("Navigation node must have a target", navigationNode, rootGroup));
        }

        private static void AddActionNodeValidationErrors(ActionMenuNode node, List<MenuValidationError> errors, GroupMenuNode? rootGroup)
        {
            if (node.AsyncAction == null && node.SyncAction == null)
            {
                errors.Add(new("Action cannot be null for a menu node without child options", node, rootGroup));
            }
        }

        private static void AddGroupMenuNodeValidationErrors(GroupMenuNode node, List<MenuValidationError> errors, GroupMenuNode? rootGroup)
        {
            if (node.ChildOptions == null || node.ChildOptions.Count == 0)
            {
                errors.Add(new("Child options cannot be null or empty for the group menu node", node, rootGroup));
            }

            if (node.IsChildMultiSelect && rootGroup?.IsChildMultiSelect == true)
            {
                errors.Add(new("Cannot use multi select for a child node of a single select node", node, rootGroup));
            }

            if (node.IsChildMultiSelect)
            {
                if (node.ChildOptions?.Any(child => child is GroupMenuNode groupChildNode) ?? false)
                {
                    errors.Add(new("Multi selection group node children cannot have children", node, rootGroup));
                }
            }

            if (node.ChildOptions != null)
            {
                foreach (var childNode in node.ChildOptions)
                {
                    AddMenuNodeValidationErrors(childNode, errors, node, rootGroup);
                }
            }
        }
    }
}
