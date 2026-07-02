namespace LegacyApp.UnitTests;

class UserCreditServiceClientStub : IDisposableUserCreditService
{
    private readonly Func<int> _GetCreditLimitImplementation;
    public UserCreditServiceClientStub(Func<int> getCreditLimitImplementation)
    {
        _GetCreditLimitImplementation = getCreditLimitImplementation;
    }

    public int GetCreditLimit(string firstname, string surname, DateTime dateOfBirth)
    {
        return _GetCreditLimitImplementation();
    }

    public void Dispose() { }

}