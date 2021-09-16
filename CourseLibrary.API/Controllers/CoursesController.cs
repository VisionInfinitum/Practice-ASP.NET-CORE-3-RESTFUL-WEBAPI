using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController: ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;
        public CoursesController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if(!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var coursesFromAuthorRepo = _courseLibraryRepository.GetCourses(authorId);
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(coursesFromAuthorRepo));
        }

        [HttpGet("{courseId}", Name ="GetCourseForAuthor")]
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId)
        {

            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseFromAuthorRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if(courseFromAuthorRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CourseDto>(courseFromAuthorRepo));
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor(Guid authorId, CourseForCreationDto course)
        {
            if(!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            // map source CourseForCreationDto to destination courseEntity
            var courseEntity = _mapper.Map<Course>(course);
            _courseLibraryRepository.AddCourse(authorId, courseEntity);
            _courseLibraryRepository.Save();

            // map source courseEntity to destination CourseDto
            var courseToReturn = _mapper.Map<CourseDto>(courseEntity);

            return CreatedAtRoute("GetCourseForAuthor", new {
                authorId,
                courseId = courseToReturn.Id
            }, courseToReturn);
        }

        [HttpPut("{courseId}")]
        public IActionResult UpdateCourseForAuthor (Guid authorId, Guid courseId, CourseForUpdateDto course)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            // Implementing the concept of upserting
            // client can create a resource by generating a new guid
            if(courseForAuthorFromRepo == null)
            {
                var courseToAdd = _mapper.Map<Course>(course);
                courseToAdd.Id = courseId;

                _courseLibraryRepository.AddCourse(authorId,courseToAdd);
                _courseLibraryRepository.Save();

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute("GetCourseForAuthor", new { authorId, courseId = courseToReturn.Id }, courseToReturn);
            }

            // Auto mapper taking care of these steps
            // map the entity to a CourseForUpdateDto
            // apply the updated field values to that dto
            // map the CourseForUpdateDto back to an entity
            _mapper.Map(course, courseForAuthorFromRepo);

            // This underlying implementation of this method is empty because
            // In Entity Framework core, these entities are tracked by the context, So by executing the
            // mapper . Map statement, the entity has changed to a modifies state and executing to Save
            // will write changes to our database. SO all we have to do is to call the Save method into the
            // repository.
            _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);
            _courseLibraryRepository.Save();

            return NoContent();
        }

        [HttpPatch("{courseId}")]
        public ActionResult PartiallyUpdateCourseForAuthor(Guid authorId,Guid courseId,JsonPatchDocument<CourseForUpdateDto> patchDocument)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var courseFromAuthorRepo = _courseLibraryRepository.GetCourse(authorId, courseId);
            /// Applying Upserting
            if (courseFromAuthorRepo == null)
            {
                var courseDto = new CourseForUpdateDto();
                patchDocument.ApplyTo(courseDto, ModelState);

                if (!TryValidateModel(courseDto))
                {
                    return ValidationProblem(ModelState);
                }

                var courseToAdd = _mapper.Map<Entities.Course>(courseDto);
                courseToAdd.Id = courseId;

                _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                _courseLibraryRepository.Save();

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);

                return CreatedAtRoute("GetCourseForAuthor",
                    new { authorId, courseId = courseToReturn.Id },
                    courseToReturn);
            }
            // Since patch is being applied to CourseForUpdateDto we need to map it from the entity
            // and apply the patch
            CourseForUpdateDto courseToPatch = _mapper.Map<CourseForUpdateDto>(courseFromAuthorRepo);

            /*
             * We need to pass the ModelState, so that if patchDocument result in error the model state
             * becomes invalid and TryValidateModel will catch the error
             * [
                    {
                      "op": "remove",
                      "path": "/thisdoesnotexist"
                    }
                ]
             */
            patchDocument.ApplyTo(courseToPatch, ModelState);
            // We need to validate the model, since it is a patch document that is applied
            // hence the validation required by us for example description can not be empty for
            // update did not occur.
            // By applying the TryValidateModel we make sure our rules are applied if not
            // we return the ValidationProblem
            // So we use the following patch action it will return in error
            /*
             * [
                    {
                      "op": "remove",
                      "path": "/description"
                    }
                  ]
             */
            if (!TryValidateModel(courseToPatch))
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(courseToPatch, courseFromAuthorRepo);
            _courseLibraryRepository.UpdateCourse(courseFromAuthorRepo);
            _courseLibraryRepository.Save();
            return NoContent();

        }

        /// <summary>
        /// We need to override this for ValidationProblem because the code inside ValidationProblem
        /// does not take in to account our custom implementation. So
        /// in order to return our custom response, which we configured in the startup file
        /// with 422 error code and a complete error message description.
        /// 
        /// </summary>
        /// <returns></returns>
        public override ActionResult ValidationProblem( [ActionResultObjectValue] ModelStateDictionary modelStateDitionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }

        [HttpDelete("{courseId}")]
        public ActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteCourse(courseForAuthorFromRepo);
            _courseLibraryRepository.Save();

            return NoContent();
        }
    }
}
