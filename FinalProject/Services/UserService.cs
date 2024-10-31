using FinalProject.Data;
using FinalProject.Helpers;
using FinalProject.ViewModels;

namespace FinalProject.Services
{
    public class UserService
    {
        private readonly QlptContext db;

        public UserService(QlptContext db)
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

        public async Task<User> getUserById(int? userId)
        {
            if (userId == null)
            {
                return null;
            }

            var user = db.Users.SingleOrDefault(u => u.UserId == userId);

            return user;
        }

        public async Task<bool> UpdateUserInformationAsync(User user, IFormFile userimage)
        {
            try
            {
                var existingUser = await db.Users.FindAsync(user.UserId);

                if (existingUser == null)
                {
                    return false;
                }

                existingUser.Fullname = user.Fullname;
                existingUser.Dob = user.Dob;
                existingUser.Address = user.Address;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.Gender = user.Gender;
                if(userimage != null)
                {
                    existingUser.UserImage = user.UserImage;
                }    

                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<string> UpdateUserImageAsync(User existingUser, IFormFile imageFile)
        {
            try
            {
                if (!string.IsNullOrEmpty(existingUser.UserImage))
                {
                    var oldImagePath = Path.Combine("wwwroot", existingUser.UserImage.TrimStart('/'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                var filePath = Path.Combine("wwwroot/img/User", fileName);

                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }    

                string img = "/img/User/" + fileName;

                return img;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
