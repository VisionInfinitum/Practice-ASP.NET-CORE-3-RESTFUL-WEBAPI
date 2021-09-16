using CourseLibrary.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Models
{

    public class CourseForCreationDto : CourseForManipulationDto
    {
    }

    /*[TitleDescriptionMustBeDifferent(ErrorMessage ="Title must be different from description")]
    public class CourseForCreationDto //: IValidatableObject
    {
        [Required(ErrorMessage ="You should fill out a title")]
        [MaxLength(100, ErrorMessage ="No more than 100 characters")]
        public string Title { get; set; }
        [MaxLength(1500)]
        public string Description { get; set; }

        *//*public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(Title == Description)
            {
                yield return new ValidationResult("The provided description should be different from the title.", new[] { "CourseForCreationDto" });
            }
        }*//*

        
    }*/
}
