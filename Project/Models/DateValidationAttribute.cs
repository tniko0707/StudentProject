using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    /// <summary>
    /// Проверка корректности даты окончания
    /// </summary>
    public class DateValidationAttribute: ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is CreateEventDto)
            {
                CreateEventDto ev = value as CreateEventDto;
                if (ev.StartAt < ev.EndAt)
                {
                    return true;
                }

            }
            if (value is UpdateEventDto)
            {
                UpdateEventDto ev = value as UpdateEventDto;
                if (ev.StartAt < ev.EndAt)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
