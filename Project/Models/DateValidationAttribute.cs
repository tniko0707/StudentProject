using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class DateValidationAttribute: ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            Event ev = value as Event;
            if (ev != null)
            {
                if (ev.StartAt < ev.EndAt)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
