namespace YourNextTravel.Api.Domain.Users;

public sealed class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string? PasswordHash { get; private set; }

    public string? GoogleSubjectId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public Role Role { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private User()
    {
    }

    public static User CreateWithPassword(string email, string passwordHash, string displayName)
    {
        ValidateEmail(email);

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            DisplayName = displayName,
            Role = Role.User,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static User CreateWithGoogle(string email, string googleSubjectId, string displayName)
    {
        ValidateEmail(email);

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            GoogleSubjectId = googleSubjectId,
            DisplayName = displayName,
            Role = Role.User,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void LinkGoogleAccount(string googleSubjectId)
    {
        GoogleSubjectId = googleSubjectId;
    }

    public void PromoteToAdmin()
    {
        Role = Role.Admin;
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }
    }
}
