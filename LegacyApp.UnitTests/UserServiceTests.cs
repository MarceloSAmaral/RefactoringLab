namespace LegacyApp.UnitTests;

public class UserServiceTests
{
    public UserServiceTests()
    {

    }

    [Fact]
    public void AddUser_ShouldReturnTrue_WhenUserIsSaved()
    {
        // Arrange
        var sut = new UserService(x => { }, id => new Client { Id = id, Name = "VeryImportantClient" });
        // Act
        var result = sut.AddUser("John", "Doe", "john.doe@example.com", new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local), 1);
        // Assert
        Assert.True(result);
    }



}
