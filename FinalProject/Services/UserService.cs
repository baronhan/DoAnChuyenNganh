using FinalProject.Data;
using FinalProject.Helpers;
using FinalProject.ViewModels;

namespace FinalProject.Services
{
    public class UserService
    {
        private readonly QlptContext db;

        public UserService (QlptContext db)
        {
            this.db = db;
        }

        public async Task<bool> CreateUserAsync(User user, RegisterVM register)
        {
            user.RandomKey = MyUtil.GenerateRandomKey();
            user.Password = register.password.ToMd5Hash(user.RandomKey);
            user.IsValid = true;
            user.UserTypeId = 3;

            try
            {
                await db.AddAsync(user); 
                await db.SaveChangesAsync(); 
                return true; 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false; 
            }
        }

        public async Task<User> CheckUserAsync(LoginVM model)
        {
            var customer = db.Users.SingleOrDefault(u => u.Username == model.username);
            return customer;
        }
    }
}
