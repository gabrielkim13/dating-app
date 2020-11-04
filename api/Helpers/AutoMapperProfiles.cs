using System;
using System.Linq;
using api.DTOs;
using api.Entities;
using api.Extensions;
using AutoMapper;

namespace api.Helpers
{
  public class AutoMapperProfiles : Profile
  {
    public AutoMapperProfiles()
    {
      CreateMap<AppUser, MemberDTO>()
        .ForMember(dest => dest.PhotoUrl,
                   opt => opt.MapFrom(src => src.Photos.FirstOrDefault(photo => photo.IsMain).Url))
        .ForMember(dest => dest.Age,
                   opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
      CreateMap<Photo, PhotoDTO>();
      CreateMap<MemberUpdateDTO, AppUser>();
      CreateMap<RegisterDTO, AppUser>();
      CreateMap<Message, MessageDTO>()
        .ForMember(dest => dest.SenderPhotoUrl,
                   opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(photo => photo.IsMain).Url))
        .ForMember(dest => dest.RecipientPhotoUrl,
                   opt => opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(photo => photo.IsMain).Url));
    }
  }
}
