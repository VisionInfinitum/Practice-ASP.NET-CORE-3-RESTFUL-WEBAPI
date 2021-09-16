using CourseLibrary.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.ValidationAttributes
{
    public class TitleDescriptionMustBeDifferent : ValidationAttribute
    {
        /// <summary>
        /// object is the object to validate, our course in this example
        /// If we use this at property level, object will not be the property, it will be the containing object so the course and
        /// that can be useful to access other property values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var course = (CourseForManipulationDto)validationContext.ObjectInstance;
            if (course.Title == course.Description)
            {
               return new ValidationResult(ErrorMessage, new[] { nameof(CourseForManipulationDto) });
            }

            return ValidationResult.Success;
        }
    }
}

