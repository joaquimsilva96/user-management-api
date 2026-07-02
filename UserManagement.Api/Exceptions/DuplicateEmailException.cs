namespace UserManagement.Api.Exceptions;

public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException()
        : base("A user with this email already exists.")
    {
    }
}
