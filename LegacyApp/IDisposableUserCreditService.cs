using LegacyApp;
using System;

/// <summary>
/// This interface binds together the IUserCreditService and IDisposable interfaces.
/// </summary>
public interface IDisposableUserCreditService : IUserCreditService, IDisposable
{

}