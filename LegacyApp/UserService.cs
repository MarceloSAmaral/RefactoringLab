using System;

namespace LegacyApp
{
    public class UserService
    {

        /// <summary>
        /// Default constructor for the UserService class. Initializes a new instance of the UserService class.
        /// </summary>
        public UserService() { }

        /// <summary>
        /// Parameterized constructor for the UserService class that accepts a method to add a user. This allows for dependency injection of the user addition logic.
        /// </summary>
        /// <param name="addUserMethod">Method for saving a user to the database.</param>
        /// <param name="getClientByIdMethod">Method for retrieving a client by their ID.</param>
        /// <param name="userCreditServiceFactoryMethod">Factory method for creating instances of the user credit service.</param>
        internal UserService(Action<User> addUserMethod, Func<int, Client> getClientByIdMethod, Func<IDisposableUserCreditService> userCreditServiceFactoryMethod)
        {
            _addUserMethod = addUserMethod;
            _getClientByIdMethod = getClientByIdMethod;
            _userCreditServiceFactoryMethod = userCreditServiceFactoryMethod;
        }


        private readonly Action<User> _addUserMethod = UserDataAccess.AddUser;
        private readonly Func<int,Client> _getClientByIdMethod = new ClientRepository().GetById;
        private readonly Func<IDisposableUserCreditService> _userCreditServiceFactoryMethod = () => new UserCreditServiceClient();


        public bool AddUser(string firname, string surname, string email, DateTime dateOfBirth, int clientId)
        {
            if (string.IsNullOrEmpty(firname) || string.IsNullOrEmpty(surname))
            {
                return false;
            }

            if (email.Contains("@") && !email.Contains("."))
            {
                return false;
            }

            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;

            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }

            if (age < 21)
            {
                return false;
            }

            var client = _getClientByIdMethod(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                Firstname = firname,
                Surname = surname
            };

            if (client.Name == "VeryImportantClient")
            {
                // Skip credit chek
                user.HasCreditLimit = false;
            }
            else if (client.Name == "ImportantClient")
            {
                // Do credit check and double credit limit
                user.HasCreditLimit = true;
                using (var userCreditService = GetUserCreditService())
                {
                    var creditLimit = userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
                    creditLimit = creditLimit * 2;
                    user.CreditLimit = creditLimit;
                }
            }
            else
            {
                // Do credit check
                user.HasCreditLimit = true;
                using (var userCreditService = GetUserCreditService())
                {
                    var creditLimit = userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                }
            }

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            _addUserMethod(user);

            return true;
        }

        private IDisposableUserCreditService GetUserCreditService()
        {
            return _userCreditServiceFactoryMethod();
        }
    }
}