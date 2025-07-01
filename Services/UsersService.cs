using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using demo2;
using Microsoft.EntityFrameworkCore.Metadata;

public class UserService
{
    private readonly UsersRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly IEntityType _userEntType;


    public UserService(UsersRepository userRepository, ILogger<UserService> logger, DbnameContext context)
    {
        _userRepository = userRepository;
        _logger = logger;
        _userEntType = context.Model.FindEntityType(typeof(User))!;
    }

    public async Task<List<ResponseUserDTO>> GetAllUsers()
    {
        List<ResponseUserDTO> responseUsers = new List<ResponseUserDTO>();

        List<User> users = await _userRepository.GetUsers();

        foreach (User u in users)
        {
            responseUsers.Add(GetResponseUser(u)!);
        }

        return responseUsers;
    }

    public async Task<ResponseUserDTO?> GetUserById(int id)
    {
        return GetResponseUser(await _userRepository.GetUserById(id));
    }

    public async Task<ResponseUserDTO> AddNewUser(CreateUserDTO newUserDTO)
    {
        // If an Optional is NULL               SET TO NULL
        // If an Optional's value is NULL       SET TO NULL
        User createdUser = await _userRepository.AddUser(new User
        {
            FirstName = newUserDTO.FirstName,
            LastName = newUserDTO.LastName,
            Email = newUserDTO.Email,
            PhoneNumber = newUserDTO.PhoneNumber != null ? newUserDTO.PhoneNumber.value : null,
            Dob = newUserDTO.Dob != null ? newUserDTO.Dob.value : null
        });

        // here we convert
        return GetResponseUser(createdUser)!;
    }

    public async Task<ResponseUserDTO?> FullyUpdateUserById(UpdateUserDTO userDTO, int id)
    {
        // Makes sure the userDTO's REQUIRED properties are not set to null
        // Throws NullReqFieldException exception if so.
        CheckRequiredPropsNotNull(userDTO);

        User? originalUser = await _userRepository.GetUserById(id);

        if (originalUser == null) return null;

        // If an Optional is NULL               SET TO NULL
        // If an Optional's value is NULL       SET TO NULL
        // Optional's value for a NON NULL column will throw an Exception
        User updatedUser = new User
        {
            FirstName = userDTO.FirstName != null ? userDTO.FirstName.value! : originalUser.FirstName,
            LastName = userDTO.LastName != null ? userDTO.LastName.value! : originalUser.LastName,
            Email = userDTO.Email != null ? userDTO.Email.value! : originalUser.Email,
            PhoneNumber = userDTO.PhoneNumber != null ? userDTO.PhoneNumber.value : null,
            Dob = userDTO.Dob != null ? userDTO.Dob.value : null
        };

        // checks for invalid values assigned to the properties
        // throws SanitationException
        SanitizeUser(updatedUser);

        return GetResponseUser(await _userRepository.UpdateUserById(updatedUser, id));
    }

    public async Task<ResponseUserDTO?> PartiallyUpdateUserById(UpdateUserDTO userDTO, int id)
    {
        // Makes sure the userDTO's REQUIRED properties are not set to null
        // Throws NullReqFieldException exception if so.
        CheckRequiredPropsNotNull(userDTO);

        User? targetUser = await _userRepository.GetUserById(id);

        if (targetUser == null) return null;

        // If an Optional is NULL               SKIP IT
        // If an Optional's value is NULL       SET TO NULL
        // Optional's value for a 'NOT NULL' column will throw an Exception
        return GetResponseUser(await _userRepository.UpdateUserById(new User
        {
            FirstName = userDTO.FirstName != null ? userDTO.FirstName.value! : targetUser.FirstName,
            LastName = userDTO.LastName != null ? userDTO.LastName.value! : targetUser.LastName,
            Email = userDTO.Email != null ? userDTO.Email.value! : targetUser.Email,
            PhoneNumber = userDTO.PhoneNumber == null ? targetUser.PhoneNumber : userDTO.PhoneNumber.value,
            Dob = userDTO.Dob == null ? targetUser.Dob : userDTO.Dob.value
        }, id));
    }

    public async Task<bool> DeleteUserById(int id)
    {
        return await _userRepository.DeleteUserById(id);
    }

    private void Old(UpdateUserDTO user)
    {
        string message = "You cannot set the following properties to NULL: ";
        List<string> props = new List<string>();

        if (user.FirstName != null && user.FirstName.value == null)
        {
            props.Add("FirstName");
        }
        if (user.LastName != null && user.LastName.value == null)
        {
            props.Add("LastName");
        }
        if (user.Email != null && user.Email.value == null)
        {
            props.Add("Email");
        }

        for (int i = 0; i < props.Count - 1; i++)
        {
            message += $"{props[i]}, ";
        }
        message += $"{props.Last()}.";

        if (props.Count > 0)
            throw new Exception(message);
    }

    // Required fields list is taken from the CreateUserDTO class via reflection.
    private void CheckRequiredPropsNotNull(UpdateUserDTO user)
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

                // we continue if the Optional in mem is null, because that means the request
                // just doesn't want to update the field.
                if (mem_user_curProp == null) continue;

                PropertyInfo? cls_UpdateUserDTO_curProp_ValueProp = mem_user_curProp.GetType().GetProperty("value");

                var mem_user_curProp_ValueProp = cls_UpdateUserDTO_curProp_ValueProp?.GetValue(mem_user_curProp);

                // check if the value of 'Value' is set to null
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

    private ResponseUserDTO? GetResponseUser(User? modelUser)
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

    // server-side sanitation for the User. Ideally we would read from the User type to get
    // the constraints and check on those here.
    private void SanitizeUser(User u)
    {
        List<string> errors = new List<string>();
        string errorMessage = "";



        if (u.PhoneNumber?.Length != 10)
            errors.Add("Property PhoneNumber must be a 10 digits string");
        if (u.FirstName.Length > 25)
            errors.Add("Property FirstName must not exceed 25 digits");
        if (u.LastName.Length > 25)
            errors.Add("Property LastName must not exceed 25 digits");
        if (u.Email.Length > 25)
            errors.Add("Property Email must not exceed 30 digits");

        if (errors.Count() == 0) return;

        foreach (String error in errors)
        {
            errorMessage += error + "\n";
        }

        throw new Exception(errorMessage);
    }
}