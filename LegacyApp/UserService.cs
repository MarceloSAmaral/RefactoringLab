using System;

namespace LegacyApp
{
    public class UserService
    {
        private const int MinimumAge = 21;
        private const int MinimumCreditLimit = 500;
        private const string VeryImportantClientName = "VeryImportantClient";
        private const string ImportantClientName = "ImportantClient";

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
            if (!IsNameValid(firname) || !IsNameValid(surname)) return false;

            if (!IsEmailValid(email)) return false;

            int age = CalculateAge(_currentLocalDateTime, dateOfBirth);

            if (!IsUserOldEnough(age)) return false;

            var client = _getClientByIdMethod(clientId);

            User user = CreateUser(firname, surname, email, dateOfBirth, client);

            (user.HasCreditLimit, user.CreditLimit) = CalculateUserCredit(client, user, _userCreditServiceFactoryMethod);

            if (!HasUserEnoughCredit(user)) return false;

            _addUserMethod(user);

            return true;
        }

        /// <summary>
        /// Validates if the provided name is valid (not null or empty).
        /// </summary>
        /// <param name="value">Name part to validate.</param>
        /// <returns></returns>
        internal static bool IsNameValid(string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Validates if the provided email is valid (contains "@" and ".").
        /// </summary>
        /// <param name="email">Email address to validate.</param>
        /// <returns></returns>
        internal static bool IsEmailValid(string email)
        {
            return !email.Contains("@") || email.Contains(".");
        }

        /// <summary>
        /// Creates a new user instance with the provided details.
        /// </summary>
        /// <param name="firname">The user's first name.</param>
        /// <param name="surname">The user's surname.</param>
        /// <param name="email">The user's email address.</param>
        /// <param name="dateOfBirth">The user's date of birth.</param>
        /// <param name="client">The client to which the user belongs.</param>
        /// <returns>The created user instance.</returns>
        internal static User CreateUser(string firname, string surname, string email, DateTime dateOfBirth, Client client)
        {
            return new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                Firstname = firname,
                Surname = surname
            };
        }

        /// <summary>
        /// Calculates the age of a user based on the current local date and their date of birth.
        /// </summary>
        /// <param name="currentLocalDateTime">The current local date and time.</param>
        /// <param name="dateOfBirth">The user's date of birth.</param>
        /// <returns>The calculated age.</returns>
        internal static int CalculateAge(DateTime currentLocalDateTime, DateTime dateOfBirth)
        {
            int age = currentLocalDateTime.Year - dateOfBirth.Year;

            if (currentLocalDateTime.Month < dateOfBirth.Month || (currentLocalDateTime.Month == dateOfBirth.Month && currentLocalDateTime.Day < dateOfBirth.Day))
            {
                age--;
            }

            return age;
        }

        /// <summary>
        /// Determines if the user is old enough based on the minimum age requirement.
        /// </summary>
        /// <param name="age">The age of the user.</param>
        /// <returns></returns>
        internal static bool IsUserOldEnough(int age)
        {
            return age >= MinimumAge;
        }

        /// <summary>
        /// Calculates the user's credit limit based on the client type and user information. 
        /// </summary>
        /// <param name="client">The client for which to calculate credit.</param>
        /// <param name="user">The user for whom to calculate credit.</param>
        /// <param name="userCreditServiceFactory">The factory for creating user credit services.</param>
        /// <returns>A tuple indicating whether the user has a credit limit and the limit amount.</returns>
        internal static (bool HasCreditLimit, int CreditLimit) CalculateUserCredit(Client client, User user, Func<IDisposableUserCreditService> userCreditServiceFactory)
        {
            if (client.Name == VeryImportantClientName) return (false, 0);

            bool hasCreditLimit = true;
            int creditLimit = 0;

            using var userCreditService = userCreditServiceFactory();
            creditLimit = userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);

            if (client.Name == ImportantClientName)
            {
                creditLimit *= 2;
            }

            return (hasCreditLimit, creditLimit);
        }

        /// <summary>
        /// Determines if the user has enough credit based on their credit limit and the minimum required credit limit.
        /// </summary>
        /// <param name="user">The user for whom to check credit.</param>
        /// <returns></returns>
        internal static bool HasUserEnoughCredit(User user)
        {
            return (!user.HasCreditLimit) || (user.HasCreditLimit && user.CreditLimit >= MinimumCreditLimit);
        }
    }
}