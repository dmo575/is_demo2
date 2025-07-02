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

        SanitizeUser_NEW(createdUser);

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
        SanitizeUser_NEW(fullyUpdatedUser);

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
        SanitizeUser_NEW(partiallyUPdatedUser);

        return GetResponseUserDTO(await _userRepository.UpdateUserById(partiallyUPdatedUser, id));
    }

    public async Task<bool> DeleteUserById(int id)
    {
        return await _userRepository.DeleteUserById(id);
    }

    private void CheckRequiredPropsNotNull_NEW(User user)
    {
        string message = "You cannot set the following properties to NULL:";
        List<string> props = new List<string>();

        List<string> requiredPropsNames = typeof(CreateUserDTO)
        .GetProperties()
        .Where(p => Attribute.IsDefined(p, typeof(RequiredAttribute)))
        .Select(p => p.Name)
        .ToList();

        PropertyInfo[] col_cls_UpdateUserDTO_properties = typeof(User).GetProperties();

        foreach (PropertyInfo cls_UpdateUserDTO_curProp in col_cls_UpdateUserDTO_properties)
        {
            if (requiredPropsNames.Contains(cls_UpdateUserDTO_curProp.Name))
            {
                object? mem_user_curProp = cls_UpdateUserDTO_curProp.GetValue(user);

                // if the field in mem is null
                if (mem_user_curProp == null)
                    props.Add(cls_UpdateUserDTO_curProp.Name);
            }
        }

        if (props.Count == 0) return;

        for (int i = 0; i < props.Count - 1; i++)
        {
            message += $" {props[i]},";
        }
        message += $" {props.Last()}.";

        if (props.Count > 0) throw new NullReqFieldException(message);
    }

    // Required fields list is taken from the CreateUserDTO class via reflection.
    private void CheckRequiredPropsNotNull(UpdateUserDTO user, bool checkOptionalNotNull)
    {
        string message = "You cannot set the following properties to NULL:";
        List<string> props = new List<string>();

        List<string> requiredPropsNames = typeof(CreateUserDTO)
        .GetProperties()
        .Where(p => Attribute.IsDefined(p, typeof(RequiredAttribute)))
        .Select(p => p.Name)
        .ToList();

        PropertyInfo[] col_cls_UpdateUserDTO_properties = typeof(UpdateUserDTO).GetProperties();

        foreach (PropertyInfo cls_UpdateUserDTO_curProp in col_cls_UpdateUserDTO_properties)
        {
            if (requiredPropsNames.Contains(cls_UpdateUserDTO_curProp.Name))
            {
                object? mem_user_curProp = cls_UpdateUserDTO_curProp.GetValue(user);

                // if the Optional in mem is NULL:
                if (mem_user_curProp == null)
                {
                    // add it to the list of nulls if we are indeed checking for that too
                    if (checkOptionalNotNull)
                        props.Add(cls_UpdateUserDTO_curProp.Name);

                    continue;
                }

                PropertyInfo? cls_UpdateUserDTO_curProp_ValueProp = mem_user_curProp.GetType().GetProperty("value");

                var mem_user_curProp_ValueProp = cls_UpdateUserDTO_curProp_ValueProp?.GetValue(mem_user_curProp);

                // check if the value of 'value' is set to null
                if (mem_user_curProp_ValueProp == null)
                {
                    // since its null, it means we are explicitly setting the value of a required column
                    // to null, which is not allowed, so we add the name of this property to the list
                    props.Add(cls_UpdateUserDTO_curProp.Name);
                }
            }
        }

        if (props.Count == 0) return;

        for (int i = 0; i < props.Count - 1; i++)
        {
            message += $" {props[i]},";
        }
        message += $" {props.Last()}.";

        if (props.Count > 0) throw new NullReqFieldException(message);
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

    private void SanitizeUser_NEW(User u)
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
            throw new Exception(JsonSerializer.Serialize(erList));
    }

    // server-side sanitation for the User. Ideally we would read from the User type to get
    // the constraints and check on those here.
    private void SanitizeUser(User u)
    {
        // makes sure that any required values are not set to null. Throws "NullReqFieldException" if so.
        CheckRequiredPropsNotNull_NEW(u);

        // TODO: For each prop, like we are doing with the reflection method, check for all of the constraints, not just the
        // required constraint.
        // Use the User model class via the Context.
        // Create some sort of JSON to display all the errors per field, like:
        /*
        [
            {
                "Token": "FirstName",
                "Errors": [
                    "Required field cannot be set to null",
                    "Must be no longer than 20 characters long"
                ]
            },
            {...}
        ]
        */

        List<string> errors = new List<string>();
        string errorMessage = "";

        if (u.PhoneNumber != null && u.PhoneNumber.Length != 10)
            errors.Add("Property PhoneNumber must be a 10 character string.");
        if (u.FirstName.Length > 25)
            errors.Add("Property FirstName must not exceed 25 characters.");
        if (u.LastName.Length > 25)
            errors.Add("Property LastName must not exceed 25 characters.");
        if (u.Email.Length > 25)
            errors.Add("Property Email must not exceed 30 characters.");

        if (errors.Count() == 0) return;

        foreach (String error in errors)
        {
            errorMessage += error + "\n";
        }

        throw new Exception(errorMessage);
    }
}