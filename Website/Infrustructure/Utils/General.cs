using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Website.Infrustructure.Utils
{
    public class General
    {
        public static List<ValidationResult> ValidateObject(object obj)
        {
            ValidationContext context = new ValidationContext(obj, null, null);
            List<ValidationResult> results = new List<ValidationResult>();
            Validator.TryValidateObject(obj, context, results);

            return results;
        }
    }
}