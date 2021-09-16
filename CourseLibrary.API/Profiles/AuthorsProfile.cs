using AutoMapper;
using CourseLibrary.API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Profiles
{
    /// <summary>
    /// It derives from the Profile class from AutoMapper to be used as a profile
    /// </summary>
    public class AuthorsProfile: Profile
    {
        public AuthorsProfile()
        {
            /*
             * AutoMapper is convention based, it will map the properties from source to destination object with the same names
             * It will ignore null reference exceptions, if a property does not exists it will be ignored
             * For our case we have some additional requirements, and for that we need projection
             */ 
            CreateMap<Entities.Author, Models.AuthorDto>()
                .ForMember(destinationMember => destinationMember.Name, memberOptions => memberOptions.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(destinationMember => destinationMember.Age, memberOptions => memberOptions.MapFrom(src => src.DateOfBirth.GetCurrentAge()));
            CreateMap<Models.AuthorForCreationDto, Entities.Author>();
        }
    }
}
