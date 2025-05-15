using AutoMapper;
using qlockAPI.Core.DTOs.GroupDTOs;
using qlockAPI.Core.DTOs.KeyDTOs;
using qlockAPI.Core.DTOs.LockDTOs;
using qlockAPI.Core.DTOs.LogDTOs;
using qlockAPI.Core.DTOs.UserDTOs;
using qlockAPI.Core.Entities;
using System.Net.Sockets;

namespace qlockAPI.Core.AutoMapperConfig
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //User
            CreateMap<CreateUserDTO, User>();
            CreateMap<User,UserViewDTO>();
            CreateMap<Lock,LockDTO>();
            CreateMap<UpdateUserDTO, User>();
            CreateMap<User, FriendDTO>()
            .ForMember(dest => dest.FriendId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FriendName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.FriendEmail, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin));
            //CreateMap<Relationship, FriendDTO>();

            //Lock
            CreateMap<CreateLockDTO, Lock>();
            CreateMap<UpdateLockDTO, Lock>();

            //Key
            CreateMap<UpdateKeyDTO,Key>();
            CreateMap<Key, KeyDTO>()
            .ForMember(dest => dest.LockId, opt => opt.MapFrom(src => src.Lock.Id))
            .ForMember(dest => dest.LockName, opt => opt.MapFrom(src => src.Lock.Name))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name));
            CreateMap<CreateKeyDTO, Key>();

            //Log
            CreateMap<Log, LockLogDTO>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.LockName, opt => opt.MapFrom(src => src.Lock.Name))
            .ForMember(dest => dest.LockId, opt => opt.MapFrom(src => src.Lock.Id))
            .ForMember(dest => dest.KeyName, opt => opt.MapFrom(src => src.Key.Name))
            .ForMember(dest => dest.KeyId, opt => opt.MapFrom(src => src.Key.Id));

            //Group
            CreateMap<CreateGroupDTO, Group>();
        }
    }
}
