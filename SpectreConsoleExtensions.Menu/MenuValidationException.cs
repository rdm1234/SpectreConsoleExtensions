namespace Rdm.Spectre.Console.Extensions.Menu
{
    public class MenuValidationException : Exception
    {
        public MenuValidationException(List<MenuValidationError> errors)
        {
            Errors = errors;
        }

        public MenuValidationException(string? message, List<MenuValidationError> errors) : base(message)
        {
            Errors = errors;
        }

        public List<MenuValidationError> Errors { get; }
    }
}
