using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using demo2;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

public class UserService
{
    private readonly UsersRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly IEntityType _userEntity;


    public UserService(UsersRepository userRepository, ILogger<UserService> logger, DbnameContext context)
    {
        _userRepository = userRepository;
        _logger = logger;
        _userEntity = context.Model.GetEntityTypes().FirstOrDefault(ent => ent.ClrType == typeof(User))!;
    }

    public async Task<List<ResponseUserDTO>> GetAllUsers()
    {
        List<ResponseUserDTO> responseUsers = new List<ResponseUserDTO>();

        List<User> users = await _userRepository.GetUsers();

        foreach (User u in users)
        {
            responseUsers.Add(GetResponseUserDTO(u)!);
        }

        return responseUsers;
    }

    public async Task<ResponseUserDTO?> GetUserById(int id)
    {
        return GetResponseUserDTO(await _userRepository.GetUserById(id));
    }

    public async Task<ResponseUserDTO> AddNewUser(CreateUserDTO newUserDTO)
    {
        // if an Optional is NULL               SET TO NULL
        // if an Optional's value is NULL       SET TO NULL
        User createdUser = new User
        {
            FirstName = newUserDTO.FirstName,
            LastName = newUserDTO.LastName,
            Email = newUserDTO.Email,
            PhoneNumber = newUserDTO.PhoneNumber != null ? newUserDTO.PhoneNumber.value : null,
            Dob = newUserDTO.Dob != null ? newUserDTO.Dob.value : null
        };

        SanitizeUser(createdUser);

        return GetResponseUserDTO(await _userRepository.AddUser(createdUser))!;
    }

    public async Task<ResponseUserDTO?> FullyUpdateUserById(UpdateUserDTO userDTO, int id)
    {
        User? originalUser = await _userRepository.GetUserById(id);

        if (originalUser == null) return null;

        // if an Optional is NULL               SET TO NULL
        // if an Optional's value is NULL       SET TO NULL
        User fullyUpdatedUser = new User
        {
            FirstName = userDTO.FirstName != null ? userDTO.FirstName.value! : null!,
            LastName = userDTO.LastName != null ? userDTO.LastName.value! : null!,
            Email = userDTO.Email != null ? userDTO.Email.value! : null!,
            PhoneNumber = userDTO.PhoneNumber != null ? userDTO.PhoneNumber.value : null!,
            Dob = userDTO.Dob != null ? userDTO.Dob.value : null!
        };

        // checks for invalid values assigned to the user's properties
        SanitizeUser(fullyUpdatedUser);

        return GetResponseUserDTO(await _userRepository.UpdateUserById(fullyUpdatedUser, id));
    }

    public async Task<ResponseUserDTO?> PartiallyUpdateUserById(UpdateUserDTO userDTO, int id)
    {
        User? targetUser = await _userRepository.GetUserById(id);

        if (targetUser == null) return null;

        // if an Optional is NULL               SKIP IT
        // if an Optional's value is NULL       SET TO NULL
        User partiallyUPdatedUser = new User {
            FirstName = userDTO.FirstName != null ? userDTO.FirstName.value! : targetUser.FirstName,
            LastName = userDTO.LastName != null ? userDTO.LastName.value! : targetUser.LastName,
            Email = userDTO.Email != null ? userDTO.Email.value! : targetUser.Email,
            PhoneNumber = userDTO.PhoneNumber != null ? userDTO.PhoneNumber.value: targetUser.PhoneNumber,
            Dob = userDTO.Dob != null ? userDTO.Dob.value : targetUser.Dob
        };

        // checks for invalid values assigned to the user's properties
        SanitizeUser(partiallyUPdatedUser);

        return GetResponseUserDTO(await _userRepository.UpdateUserById(partiallyUPdatedUser, id));
    }

    public async Task<bool> DeleteUserById(int id)
    {
        return await _userRepository.DeleteUserById(id);
    }

    private ResponseUserDTO? GetResponseUserDTO(User? modelUser)
    {
        if (modelUser == null)
            return null;

        return new ResponseUserDTO
        {
            Id = modelUser.Id,
            FirstName = modelUser.FirstName,
            LastName = modelUser.LastName,
            Email = modelUser.Email,
            PhoneNumber = modelUser.PhoneNumber,
            Dob = modelUser.Dob
        };
    }

    // server-side sanitation. Takes some workload off the database server.
    private void SanitizeUser(User u)
    {
        List<TokenError> erList = new List<TokenError>();

        foreach (var modelProp in _userEntity.GetProperties())
        {
            foreach (var userProp in u.GetType().GetProperties())
            {
                if (userProp.Name == modelProp.Name)
                {
                    var mem_user_prop = userProp.GetValue(u);
                    List<string> erMessages = new List<string>();

                    if (mem_user_prop == null)
                    {
                        // if set to null, and is nullable, continue
                        // else if set to null while not being nullable, add error
                        if (modelProp.IsNullable)
                            continue;
                        else
                            erMessages.Add("Cannot be null");
                    }
                    else
                    {
                        // here we write down check on string fields
                        if (modelProp.ClrType == typeof(string))
                        {
                            // if max length exceeded, add that error to the list
                            if (modelProp.GetMaxLength() < ((string)mem_user_prop).Length)
                                erMessages.Add($"Max length is {modelProp.GetMaxLength()}");

                            // ... other string related checks
                        }

                        // ... other filed types we might want to check
                    }

                    if (erMessages.Count == 0) continue;

                    // fill the TokenError object with all of the errors for the current property
                    TokenError erToken = new() { TokenName = userProp.Name };
                    foreach (var errorMessage in erMessages)
                    {
                        erToken.Errors.Add(errorMessage);
                    }

                    // add the token to the list
                    erList.Add(erToken);
                }
            }
        }

        if (erList.Count > 0)
            // throw new SanitationException(JsonSerializer.Serialize(erList));
            throw new SanitationException(erList);

    }
}