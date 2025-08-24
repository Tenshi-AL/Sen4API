using AutoMapper;
using Domain.Models;
using Infrastructure.DTO;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Operation = Domain.Models.Operation;
using TaskStatus = Domain.Models.TaskStatus;

namespace Infrastructure;

public class MapperProfile: Profile
{
    public MapperProfile()
    {
        //User
        CreateMap<UserRegistrationDTO, User>()
            .ForMember(p=>p.UserName, dest=>dest.MapFrom(p=>p.Email));
        CreateMap<User, UserReadDTO>();
        CreateMap<UserUpdateDTO, User>();
        CreateMap<User, UserUpdateDTO>();
        
        //user patch document
        CreateMap(typeof(JsonPatchDocument<UserUpdateDTO>), typeof(JsonPatchDocument<User>));
        CreateMap(typeof(Operation<UserUpdateDTO>), typeof(Operation<User>));
        
        //User post
        CreateMap<Post, PostReadDTO>();
        
        //Project
        CreateMap<Project, ProjectReadDTO>();

        CreateMap<ProjectWriteDTO, Project>()
            .ForMember(p => p.CreatedDateTime, opt => opt.MapFrom(p => DateTime.UtcNow));
        
        //Project task
        CreateMap<ProjectTaskWriteDTO, ProjectTask>()
            .ForMember(p => p.CreatedDate, opt => opt.MapFrom(p => p.CreatedDate.ToUniversalTime()))
            .ForMember(p => p.DeadlineDate, opt => opt.MapFrom(p => p.DeadlineDate.ToUniversalTime()));

        CreateMap<ProjectTask, ProjectTaskReadDTO>()
            .ForMember(p=>p.Priority, opt=> opt.MapFrom(p=>p.Priority.Name))
            .ForMember(p => p.Status, opt => opt.MapFrom(p => p.TaskStatus.Name))
            .ForMember(p => p.UserCreated, opt => opt.MapFrom(p => $"{p.UserCreated.Name} {p.UserCreated.Surname}"))
            .ForMember(p => p.UserExecutor, opt => opt.MapFrom(p => $"{p.UserExecutor.Name} {p.UserExecutor.Surname}"))
            .ForMember(p=>p.UserExecutorEmail, opt=>opt.MapFrom(p=>p.UserExecutor.Email))
            .ForMember(p=>p.UserCreatorEmail, opt=>opt.MapFrom(p=>p.UserCreated.Email));
        
        //project task status
        CreateMap<TaskStatus, TaskStatusReadDTO>();
        
        //project task priority
        CreateMap<Priority, PriorityReadDTO>();

        //User rules
        CreateMap<Rule, RuleDTO>()
            .ForMember(p => p.Controller, opt => opt.MapFrom(p => p.Operation.Controller))
            .ForMember(p => p.Action, opt => opt.MapFrom(p => p.Operation.Action))
            .ForMember(p => p.Description, opt => opt.MapFrom(p => p.Operation.Description));
        CreateMap<RuleDTO, Rule>();

        
        //project patch document
        CreateMap(typeof(JsonPatchDocument<ProjectWriteDTO>), typeof(JsonPatchDocument<Project>));
        CreateMap(typeof(Operation<ProjectWriteDTO>), typeof(Operation<Project>));
        
        //project task patch document
        CreateMap(typeof(JsonPatchDocument<ProjectTaskWriteDTO>), typeof(JsonPatchDocument<ProjectTask>));
        CreateMap(typeof(Operation<ProjectTaskWriteDTO>), typeof(Operation<ProjectTask>));
        
        //operation
        CreateMap<Operation, OperationReadDTO>();
        
    }
}