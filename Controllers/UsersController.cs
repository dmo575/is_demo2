using Microsoft.AspNetCore.Mvc;
using demo2;
using Microsoft.Identity.Client;
using Microsoft.Data.SqlClient;


[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{

    private readonly UserService _userService;
    private readonly ILogger<UsersController> _Logger;


    public UsersController(UserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _Logger = logger;
    }

    [HttpGet]
    public async Task<List<ResponseUserDTO>> GetUsers()
    {
        // return all users on db
        return await _userService.GetAllUsers();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseUserDTO?>> GetUsersById([FromRoute] int id)
    {
        // return user by id or nothing if none
        ResponseUserDTO? user = await _userService.GetUserById(id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] CreateUserDTO userDTO)
    {
        // add user (required properties are automatically checked by [ApiController])
        ResponseUserDTO createdUser = await _userService.AddNewUser(userDTO);

        return Created($"Users/{createdUser.Id}", createdUser);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> FullyUpdateUser([FromBody] UpdateUserDTO userDTO, [FromRoute] int id)
    {
        try
        {
            ResponseUserDTO? updatedUser = await _userService.FullyUpdateUserById(userDTO, id);

            if (updatedUser == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(updatedUser);
        }
        catch (NullReqFieldException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PartiallyUpdateUser([FromBody] UpdateUserDTO userDTO, [FromRoute] int id)
    {
        try
        {
            ResponseUserDTO? updatedUser = await _userService.PartiallyUpdateUserById(userDTO, id);

            if (updatedUser == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(updatedUser);
        }
        catch (NullReqFieldException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser([FromRoute] int id)
    {
        bool success = await _userService.DeleteUserById(id);

        if (!success) return NoContent();

        return Ok("User deleted.");
    }
}