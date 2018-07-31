using MyOSBB.DAL;
using MyOSBB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Security;

namespace MyOSBB.Providers
{
    public class CustomMembershipProvider : MembershipProvider
    {
        public override bool ValidateUser(string username, string password)
        {
            using (MyOSBBContext _db = new MyOSBBContext())
            {
                User user = _db.Users.FirstOrDefault(u => u.Login == username);

                if (user != null && Crypto.VerifyHashedPassword(user.Password, password))
                {
                    return true;
                }
                return false;
            }
        }
        public MembershipUser CreateUser(string login, string password, string firstName,
                        string lastName, string middleName, string email, string phone)
        {
            MembershipUser membershipUser = GetUser(login, false);
            if (membershipUser == null)
            {
                try
                {
                    using (MyOSBBContext _db = new MyOSBBContext())
                    {
                        User user = new User();
                        user.Login = login;
                        user.Password = Crypto.HashPassword(password);
                        user.FirstName = firstName;
                        user.LastName = lastName;
                        user.MiddleName = middleName;
                        user.Email = email;
                        user.Phone = phone;
						user.CodeForResetPassword = null;
						if (_db.Roles.Find(3) != null)
                        {
                            user.RoleId = 3;
                        }

                        _db.Users.Add(user);
                        _db.SaveChanges();
                        membershipUser = GetUser(login, false);
                        return membershipUser;
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public MembershipUser UpdateUser(int id, string login, string password, string firstName,
                        string lastName, string middleName, string email, string phone, string codeForResetPassword)
        {
            MembershipUser membershipUser = GetUser(login, false);
            if (membershipUser != null)
            {
                try
                {
                    using (MyOSBBContext _db = new MyOSBBContext())
                    {
                        User user = new User();
                        user.Id = id;
                        user.Login = login;
                        user.Password = password;
                        user.FirstName = firstName;
                        user.LastName = lastName;
                        user.MiddleName = middleName;
                        user.Email = email;
                        user.Phone = phone;
						user.CodeForResetPassword = codeForResetPassword;
						if (_db.Roles.Find(3) != null)
                        {
                            user.RoleId = 3;
                        }
                        user.Role = _db.Roles
                          .Where(r => r.Id == user.RoleId).FirstOrDefault();
                        User userToUpdate = _db.Users
                          .Where(u => u.Login == login).FirstOrDefault();
                        _db.Entry(userToUpdate).CurrentValues.SetValues(user);
                        _db.SaveChanges();
                        return membershipUser;
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            try
            {
                using (MyOSBBContext _db = new MyOSBBContext())
                {
                    var users = from u in _db.Users
                               where u.Login == username
                               select u;
                    if(users.Count() > 0)
                    {
                        User user = users.First();
                        MembershipUser memberUser = new MembershipUser("CustomMembershipProvider", user.Login, null, null, null, null,
                            false, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
                        return memberUser;
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;

        }

        public override bool EnablePasswordRetrieval => throw new NotImplementedException();

        public override bool EnablePasswordReset => throw new NotImplementedException();

        public override bool RequiresQuestionAndAnswer => throw new NotImplementedException();

        public override string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override int MaxInvalidPasswordAttempts => throw new NotImplementedException();

        public override int PasswordAttemptWindow => throw new NotImplementedException();

        public override bool RequiresUniqueEmail => throw new NotImplementedException();

        public override MembershipPasswordFormat PasswordFormat => throw new NotImplementedException();

        public override int MinRequiredPasswordLength => throw new NotImplementedException();

        public override int MinRequiredNonAlphanumericCharacters => throw new NotImplementedException();

        public override string PasswordStrengthRegularExpression => throw new NotImplementedException();

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }
    }
}