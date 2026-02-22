namespace Octocare.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public ICollection<UserOrgMembership> Memberships { get; private set; } = [];

    public string FullName => $"{FirstName} {LastName}";

    private User() { }

    public static User Create(string externalId, string email, string firstName, string lastName)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            ExternalId = externalId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateEmail(string email)
    {
        Email = email;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
