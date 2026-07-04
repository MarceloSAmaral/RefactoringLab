using System;

namespace LegacyApp
{
    public class UserService
    {
        /// <summary>
        /// Method for saving a user to the database.
        /// </summary>
        private readonly Action<User> _addUserMethod;

        /// <summary>
        /// Method for retrieving a client by their ID.
        /// </summary>
        private readonly Func<int, Client> _getClientByIdMethod;

        /// <summary>
        /// Factory method for creating instances of the user credit service.
        /// </summary>
        private readonly Func<IDisposableUserCreditService> _userCreditServiceFactoryMethod;

        /// <summary>
        /// The current local date and time used for age calculation.
        /// </summary>
        private readonly DateTime _currentLocalDateTime;

        /// <summary>
        /// Default constructor for the UserService class. Initializes a new instance of the UserService class.
        /// </summary>
        public UserService()
        {
            _addUserMethod = UserDataAccess.AddUser;
            _getClientByIdMethod = new ClientRepository().GetById;
            _userCreditServiceFactoryMethod = () => new UserCreditServiceClient();
            _currentLocalDateTime = System.DateTime.Now;
        }

        /// <summary>
        /// Parameterized constructor for the UserService class that accepts a method to add a user. This allows for dependency injection of the user addition logic.
        /// </summary>
        /// <param name="addUserMethod">Method for saving a user to the database.</param>
        /// <param name="getClientByIdMethod">Method for retrieving a client by their ID.</param>
        /// <param name="userCreditServiceFactoryMethod">Factory method for creating instances of the user credit service.</param>
        /// <param name="currentLocalDateTime">The current date.</param>
        internal UserService(Action<User> addUserMethod, Func<int, Client> getClientByIdMethod, Func<IDisposableUserCreditService> userCreditServiceFactoryMethod, DateTime currentLocalDateTime)
        {
            _addUserMethod = addUserMethod;
            _getClientByIdMethod = getClientByIdMethod;
            _userCreditServiceFactoryMethod = userCreditServiceFactoryMethod;
            _currentLocalDateTime = currentLocalDateTime.Kind == DateTimeKind.Local ? currentLocalDateTime : throw new ArgumentException("Provide the current local datetime with the correct kind.", nameof(currentLocalDateTime));
        }

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

            var now = _currentLocalDateTime;
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