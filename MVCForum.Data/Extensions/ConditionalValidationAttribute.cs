using System;
using System.ComponentModel.DataAnnotations;

namespace SnitzCore.Data.Extensions
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredIfTrueAttribute : RequiredAttribute
    {
        private string PropertyName { get; set; }

        public RequiredIfTrueAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            object instance = context.ObjectInstance;
            Type type = instance.GetType();

            bool.TryParse(type?.GetProperty(PropertyName)?.GetValue(instance)?.ToString(), out bool propertyValue);

            if (propertyValue && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }


    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredIfAttribute : RequiredAttribute
    {
        private string PropertyName { get; set; }
        private object Value { get; set; }

        public RequiredIfAttribute(string propertyName, object value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            object instance = context.ObjectInstance;
            Type type = instance.GetType();
            var propertyValue = type?.GetProperty(PropertyName)?.GetValue(instance);

            //bool.TryParse(type?.GetProperty(PropertyName)?.GetValue(instance)?.ToString(), out bool propertyValue);
            Type valType = Value.GetType();
            if (propertyValue != Value)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }


    }
}