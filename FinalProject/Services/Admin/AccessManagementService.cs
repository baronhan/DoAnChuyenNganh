using FinalProject.Data;
using FinalProject.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Services.Admin
{
    public class AccessManagementService
    {
        private readonly QlptContext db;

        public AccessManagementService(QlptContext db)
        {
            this.db = db;
        }

        public List<UserTypeVM> GetAllUserType()
        {
            try
            {
                var userTypeList = (from ut in db.UserTypes
                                    select new UserTypeVM
                                    {
                                        UserTypeId = ut.UserTypeId,
                                        UserTypeName = ut.TypeName
                                    }).ToList();
                return userTypeList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public UserTypeVM GetUserTypeById(int userTypeId)
        {
            try
            {
                var userType = db.UserTypes.FirstOrDefault(ut => ut.UserTypeId == userTypeId);

                if (userType != null)
                {
                    UserTypeVM userTypeVM = new UserTypeVM
                    {
                        UserTypeId = userType.UserTypeId,
                        UserTypeName = userType.TypeName
                    };

                    return userTypeVM;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public bool UpdateUserType(UserTypeVM userTypeVM)
        {
            try
            {
                var existingUserType = db.UserTypes.FirstOrDefault(ut => ut.UserTypeId == userTypeVM.UserTypeId);

                if (existingUserType != null)
                {
                    existingUserType.TypeName = userTypeVM.UserTypeName;

                    db.SaveChanges();

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool CanDeleteUserType(int userTypeId)
        {
            return !db.Users.Any(u => u.UserTypeId == userTypeId);
        }

        public bool DeleteUserType(int userTypeId)
        {
            var userType = db.UserTypes.FirstOrDefault(ut => ut.UserTypeId == userTypeId);
            if (userType == null)
            {
                return false; 
            }

            db.UserTypes.Remove(userType);
            db.SaveChanges();
            return true;
        }

        public bool AddUserType(UserTypeVM userTypeVM)
        {
            try
            {
                var userType = new UserType
                {
                    TypeName = userTypeVM.UserTypeName
                };

                db.UserTypes.Add(userType);

                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public List<UserTypeVM> SearchUserTypes(string searchQuery)
        {
            try
            {
                return db.UserTypes
                    .Where(u => u.TypeName.Contains(searchQuery))
                    .Select(u => new UserTypeVM
                    {
                        UserTypeId = u.UserTypeId,
                        UserTypeName = u.TypeName
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public List<PageAddressVM> GetPageAddresses(int pageNumber, int pageSize)
        {
            return db.PageAddresses
                .OrderBy(p => p.PageName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PageAddressVM
                {
                    PageAddressId = p.PageAddressId,
                    PageName = p.PageName
                })
                .ToList();
        }

        public int GetPageAddressCount()
        {
            return db.PageAddresses.Count();
        }

        public bool AddPageAddress(PageAddressVM pageAddressVM)
        {
            try
            {
                var pageAddress = new PageAddress
                {
                    PageName = pageAddressVM.PageName,
                    Url = pageAddressVM.Url
                };

                db.PageAddresses.Add(pageAddress);

                db.SaveChanges();

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool UpdatePageAddress(PageAddressVM pageAddressVM)
        {
            try
            {
                var existingPageAddress = db.PageAddresses.FirstOrDefault(p => p.PageAddressId == pageAddressVM.PageAddressId);

                if (existingPageAddress != null)
                {
                    existingPageAddress.Url = pageAddressVM.Url;
                    existingPageAddress.PageName = pageAddressVM.PageName;

                    db.SaveChanges();

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public PageAddressVM GetPageAddressById(int pageAddressId)
        {
            try
            {
                var pageAddress = db.PageAddresses.FirstOrDefault(p => p.PageAddressId == pageAddressId);

                if (pageAddress != null)
                {
                    PageAddressVM pageAddressVM = new PageAddressVM
                    {
                        PageAddressId = pageAddress.PageAddressId,
                        PageName = pageAddress.PageName,
                        Url = pageAddress.Url
                    };

                    return pageAddressVM;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public bool CanDeletePageAddress(int pageAddressId)
        {
            return !db.Privileges.Any(u => u.PageAddressId == pageAddressId);
        }

        public bool DeletePageAdress(int pageAddressId)
        {
            var pageAddress = db.PageAddresses.FirstOrDefault(p => p.PageAddressId == pageAddressId);
            if (pageAddress == null)
            {
                return false;
            }

            db.PageAddresses.Remove(pageAddress);
            db.SaveChanges();
            return true;
        }

        public List<PageAddressVM>? SearchPageAddress(string searchQuery)
        {
            try
            {
                return db.PageAddresses
                    .Where(u => u.PageName.Contains(searchQuery))
                    .Select(u => new PageAddressVM
                    {
                        PageAddressId = u.PageAddressId,
                        PageName = u.PageName,
                        Url = u.Url
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public int GetUserTypeCount()
        {
            return db.UserTypes.Count();
        }

        public List<UserTypeVM> GetUserType(int pageNumber, int pageSize)
        {
            return db.UserTypes
                .OrderBy(p => p.TypeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new UserTypeVM
                {
                    UserTypeName = p.TypeName,
                    UserTypeId = p.UserTypeId
                })
                .ToList();
        }

        public List<PrivilegeVM> GetPrivilegesByUserType(int userTypeId)
        {
            try
            {
                var privilegeList = (from p in db.Privileges
                                     join page in db.PageAddresses on p.PageAddressId equals page.PageAddressId
                                     where p.UserTypeId == userTypeId
                                     select new PrivilegeVM
                                     {
                                         PageAddressId = (int)p.PageAddressId,
                                         PageName = page.PageName,
                                         IsPrivileged = (bool)p.IsPrivileged,
                                     }).ToList();

                return privilegeList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public bool GrantPermission(PrivilegeVM userPrivilege)
        {
            try
            {
                var newPrivilege = new Privilege
                {
                    UserTypeId = userPrivilege.UserTypeId,
                    PageAddressId = userPrivilege.PageAddressId,
                    IsPrivileged = (bool)userPrivilege.IsPrivileged,
                };

                db.Privileges.Add(newPrivilege);

                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);  
                return false;
            }
        }

        public PrivilegeVM GetPrivilegeByUserTypeAndPageAddress(int userTypeId, int pageAddressId)
        {
            var privilege = db.Privileges
                                    .Where(p => p.UserTypeId == userTypeId && p.PageAddressId == pageAddressId)
                                    .FirstOrDefault();

            if (privilege != null)
            {
                var privilegeVM = new PrivilegeVM
                {
                    PrivilegeId = privilege.PrivilegeId,
                    UserTypeId = (int)privilege.UserTypeId,
                    PageAddressId = (int)privilege.PageAddressId,
                    IsPrivileged = (bool)privilege.IsPrivileged
                };

                return privilegeVM;
            }

            return null;
        }

        public bool UpdatePrivilege(PrivilegeVM privilege)
        {
            var existingPrivilege = db.Privileges
                                             .Where(p => p.PrivilegeId == privilege.PrivilegeId)
                                             .FirstOrDefault();

            if (existingPrivilege != null)
            {
                existingPrivilege.IsPrivileged = privilege.IsPrivileged;

                db.SaveChanges();

                return true; 
            }

            return false; 
        }

    }
}
