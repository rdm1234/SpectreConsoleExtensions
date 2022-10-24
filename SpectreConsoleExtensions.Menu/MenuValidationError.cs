namespace SpectreConsoleExtensions.Menu
{
    public record MenuValidationError(string ErrorMessage, MenuNode Node, MenuNode? ParentNode)
    {
    }
}
