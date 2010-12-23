using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace PerformanceTestAgent.Validation
{
    class StringRegularationValidationRule : ValidationRule
    {
        private Regex m_Regex;

        private string m_Regularation;

        public string Regularation
        {
            get { return m_Regularation; }
            set
            {
                m_Regularation = value;
                m_Regex = new Regex(m_Regularation, RegexOptions.Compiled);
            }
        }

        public string ErrorMessage { get; set; }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (m_Regex == null)
                throw new Exception("Regularation cannot be empty");

            var targetString = value.ToString();

            if (m_Regex.IsMatch(targetString))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, string.IsNullOrEmpty(ErrorMessage) ? "Invalid Input!" : ErrorMessage);
        }
    }
}
