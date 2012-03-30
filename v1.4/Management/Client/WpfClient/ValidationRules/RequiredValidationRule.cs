using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace SuperSocket.Management.Client.ValidationRules
{
    public class RequiredValidationRule : ValidationRule
    {
        public RequiredValidationRule()
        {

        }

        public string ErrorMessage { get; set; }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return new ValidationResult(false, ErrorMessage);

            return ValidationResult.ValidResult;
        }
    }
}
