using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UserService.AsyncDataServices;
using UserService.Data;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController: ControllerBase
{
    private readonly IUserRepo _repo;
    private readonly IMapper _mapper;
    private readonly IMessageBusClient _messageBusClient;

    public UsersController(IUserRepo repo, IMapper mapper, IMessageBusClient messageBusClient)
    {
        _repo = repo;
        _mapper = mapper;
        _messageBusClient = messageBusClient;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserReadDto>> GetUsers()
    {
        // Get all users from database
        var userItems = _repo.GetAllUsers();
        return Ok(_mapper.Map<IEnumerable<UserReadDto>>(userItems));
    }

    [HttpGet("{id}", Name = "GetUserById")]
    public ActionResult<UserReadDto> GetUserById(int id)
    {
        // Get user by id from database
        var userItem = _repo.GetUserById(id);

        // If no result, 404
        if (userItem == null)
        {
            return NotFound();
        }

        // If found, return user
        return Ok(_mapper.Map<UserReadDto>(userItem));
    }

    [HttpPost]
    public ActionResult<UserReadDto> CreateUser(UserCreateDto userCreateDto)
    {
        // Map UserCreateDto to model
        var userModel = _mapper.Map<User>(userCreateDto);

        // Create user in database
        _repo.CreateUser(userModel);
        _repo.SaveChanges();

        // Map model to UserReadDto
        var userReadDto = _mapper.Map<UserReadDto>(userModel);

        // Map UserReadDto to UserPublishedDto and publish to message bus
        try
        {
            var userPublishedDto = _mapper.Map<UserPublishedDto>(userReadDto);
            userPublishedDto.Event = "User_Published";
            _messageBusClient.PublishNewUser(userPublishedDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
        }

        return CreatedAtRoute(nameof(GetUserById), new { Id = userReadDto.Id }, userReadDto);
    } 
}
