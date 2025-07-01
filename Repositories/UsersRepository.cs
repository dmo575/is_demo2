using System.Threading.Tasks;
using demo2;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class UsersRepository
{
    private readonly DbnameContext _context;

    public UsersRepository(DbnameContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetUserById(int id)
    {
        return await _context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User> AddUser(User newUser)
    {
        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        return newUser;
    }
    public async Task<User?> UpdateUserById(User update, int id)
    {
        User? user = await _context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();

        if (user != null)
        {
            user.FirstName = update.FirstName;
            user.LastName = update.LastName;
            user.Email = update.Email;
            user.Dob = update.Dob;
            user.PhoneNumber = update.PhoneNumber;

            await _context.SaveChangesAsync();

            return user;
        }

        return null;
    }
    public async Task<bool> DeleteUserById(int id)
    {
        User? user = await _context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();

        if (user != null)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}