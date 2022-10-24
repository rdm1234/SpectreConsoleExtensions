namespace Rdm.Spectre.Console.Extensions.Menu
{
    public record MenuValidationError(string ErrorMessage, MenuNode Node, MenuNode? ParentNode)
    {
    }
}
