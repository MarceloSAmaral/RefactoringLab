using Moq;

namespace LegacyApp.UnitTests;

public class UserServiceTests
{

    const string FirstName = "John";
    const string LastName = "Doe";
    const string Email = "john.doe@example.com";
    const int ClientId = 1;
    readonly DateTime userDateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local);
    const int EnoughCreditLimit = 500;


    public UserServiceTests()
    {

    }

    [Fact(DisplayName = "AddUser should return true when user is saved")]
    public void AddUser_ShouldReturnTrue_WhenUserIsSaved()
    {
        // Arrange
        var sut = new UserService(x => { }, id => new Client { Id = id, Name = "VeryImportantClient" }, () => new UserCreditServiceClient());
        // Act
        var result = sut.AddUser(FirstName, LastName, Email, userDateOfBirth, ClientId);
        // Assert
        Assert.True(result);
    }

    [Fact(DisplayName = "AddUser should mark client with HasCreditLimit as false when a VeryImportantClient client's user is saved")]
    public void AddUser_ShouldMarkHasCreditLimitAsFalse_WhenAVeryImportantClientUserIsSaved()
    {
        Mock<Action<User>> mockAddUserMethod = new Mock<Action<User>>(MockBehavior.Strict);
        mockAddUserMethod
            .Setup(addUser => addUser(It.Is<User>(user => user.HasCreditLimit == false)))
            .Verifiable();
        var sut = new UserService(mockAddUserMethod.Object, id => new Client { Id = id, Name = "VeryImportantClient" }, () => new UserCreditServiceClientStub(() => EnoughCreditLimit));

        var result = sut.AddUser(FirstName, LastName, Email, userDateOfBirth, ClientId);

        Assert.True(result);
        mockAddUserMethod.Verify(addUser => addUser(It.Is<User>(user => user.HasCreditLimit == false)), Times.Once);
    }

    [Fact(DisplayName = "AddUser should mark client with HasCreditLimit as true and it should be the double of what the CreditService reports when an ImportantClient client's user is saved")]
    public void AddUser_ShouldMarkHasCreditLimitAsTrueAndItShouldBeTheDoubleOfWhatCreditServiceReports_WhenAnImportantClientUserIsSaved()
    {
        Mock<Action<User>> mockAddUserMethod = new Mock<Action<User>>(MockBehavior.Strict);
        mockAddUserMethod
            .Setup(addUser => addUser(It.Is<User>(user => user.HasCreditLimit == true && user.CreditLimit == EnoughCreditLimit * 2)))
            .Verifiable();
        var sut = new UserService(mockAddUserMethod.Object, id => new Client { Id = id, Name = "ImportantClient" }, () => new UserCreditServiceClientStub(() => EnoughCreditLimit));

        var result = sut.AddUser(FirstName, LastName, Email, userDateOfBirth, ClientId);

        Assert.True(result);
        mockAddUserMethod.Verify(addUser => addUser(It.Is<User>(user => user.HasCreditLimit == true && user.CreditLimit == EnoughCreditLimit * 2)), Times.Once);
    }

    [Fact(DisplayName = "AddUser should mark client with HasCreditLimit as true and it should be what the CreditService reports when a regular client's user is saved")]
    public void AddUser_ShouldMarkHasCreditLimitAsTrueAndItShouldBeTheWhatCreditServiceReports_WhenAVeryImportantClientUserIsSaved()
    {
        Mock<Action<User>> mockAddUserMethod = new Mock<Action<User>>(MockBehavior.Strict);
        mockAddUserMethod
            .Setup(addUser => addUser(It.Is<User>(user => user.HasCreditLimit == true)))
            .Verifiable();
        var sut = new UserService(mockAddUserMethod.Object, id => new Client { Id = id, Name = "Regular Client" }, () => new UserCreditServiceClientStub(() => EnoughCreditLimit));

        var result = sut.AddUser(FirstName, LastName, Email, userDateOfBirth, ClientId);

        Assert.True(result);
        mockAddUserMethod.Verify(addUser => addUser(It.Is<User>(user => user.HasCreditLimit == true && user.CreditLimit == EnoughCreditLimit)), Times.Once);
    }

    [Fact(DisplayName = "AddUser should deny saving a user when they have CreditLimit and it is below 500")]
    public void AddUser_ShouldDenySavingAClientsUser_WhenUserHasCreditidLimitAndItIsBelow500()
    {
        var sut = new UserService(x => { throw new InvalidOperationException("User saving denied"); }, id => new Client { Id = id, Name = "Regular Client" }, () => new UserCreditServiceClientStub(() => 100));

        var result = sut.AddUser(FirstName, LastName, Email, userDateOfBirth, ClientId);

        Assert.False(result);
    }

    [Fact(DisplayName = "AddUser should deny saving a user when the firstname is empty")]
    public void AddUser_ShouldDenySavingAClientsUser_WhenFirstNameIsEmpty()
    {
        var sut = new UserService(x => { throw new InvalidOperationException("User saving denied"); }, id => new Client { Id = id, Name = "Regular Client" }, () => new UserCreditServiceClientStub(() => EnoughCreditLimit));

        var result = sut.AddUser(String.Empty, LastName, Email, userDateOfBirth, ClientId);

        Assert.False(result);
    }

    [Fact(DisplayName = "AddUser should deny saving a user when the firstname is null")]
    public void AddUser_ShouldDenySavingAClientsUser_WhenFirstNameIsNull()
    {
        var sut = new UserService(x => { throw new InvalidOperationException("User saving denied"); }, id => new Client { Id = id, Name = "Regular Client" }, () => new UserCreditServiceClientStub(() => EnoughCreditLimit));

        var result = sut.AddUser(null, LastName, Email, userDateOfBirth, ClientId);

        Assert.False(result);
    }

    [Fact(DisplayName = "AddUser should deny saving a user when the lastname is empty")]
    public void AddUser_ShouldDenySavingAClientsUser_WhenLastNameIsEmpty()
    {
        var sut = new UserService(x => { throw new InvalidOperationException("User saving denied"); }, id => new Client { Id = id, Name = "Regular Client" }, () => new UserCreditServiceClientStub(() => EnoughCreditLimit));

        var result = sut.AddUser(FirstName, String.Empty, Email, userDateOfBirth, ClientId);

        Assert.False(result);
    }

    [Fact(DisplayName = "AddUser should deny saving a user when the lastname is null")]
    public void AddUser_ShouldDenySavingAClientsUser_WhenLastNameIsNull()
    {
        var sut = new UserService(x => { throw new InvalidOperationException("User saving denied"); }, id => new Client { Id = id, Name = "Regular Client" }, () => new UserCreditServiceClientStub(() => EnoughCreditLimit));

        var result = sut.AddUser(FirstName, null, Email, userDateOfBirth, ClientId);

        Assert.False(result);
    }

    [Fact(DisplayName = "AddUser should deny saving a user when the email address has a @ symbol but no dot")]
    public void AddUser_ShouldDenySavingAClientsUser_WhenEmailHasAtSymbolButNoDot()
    {
        var sut = new UserService(x => { throw new InvalidOperationException("User saving denied"); }, id => new Client { Id = id, Name = "Regular Client" }, () => new UserCreditServiceClientStub(() => EnoughCreditLimit));

        var result = sut.AddUser(FirstName, LastName, "invalid@email", userDateOfBirth, ClientId);

        Assert.False(result);
    }

    [Fact(DisplayName = "AddUser should deny saving a user when the age is below 21")]
    public void AddUser_ShouldDenySavingAClientsUser_WhenAgeIsBelow21()
    {
        var sut = new UserService(x => { throw new InvalidOperationException("User saving denied"); }, id => new Client { Id = id, Name = "Regular Client" }, () => new UserCreditServiceClientStub(() => EnoughCreditLimit));

        var result = sut.AddUser(FirstName, LastName, Email, System.DateTime.Now.Date.AddYears(-20), ClientId);

        Assert.False(result);
    }
}
