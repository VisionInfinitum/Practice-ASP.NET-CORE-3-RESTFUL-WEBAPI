using CourseLibrary.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Models
{
    /// <summary>
    /// abstract indicates it will only be used as a base class for other classes
    /// </summary>
    [TitleDescriptionMustBeDifferent(ErrorMessage ="Title must be different from description")]
    public abstract class CourseForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a title")]
        [MaxLength(100, ErrorMessage = "No more than 100 characters")]
        public string Title { get; set; }
        [MaxLength(1500, ErrorMessage = "The description should't have more than 1500 characters")]
        public virtual string Description { get; set; }
    }
}

