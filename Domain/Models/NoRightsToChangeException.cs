namespace Domain.Models
{
    public class NoRightsToChangeException : Exception
    {
        public NoRightsToChangeException() : base("Out of rights") { }
        public NoRightsToChangeException(string? message) : base(message)
        {
        }
        public NoRightsToChangeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
