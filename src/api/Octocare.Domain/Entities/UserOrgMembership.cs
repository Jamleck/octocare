using Octocare.Domain.Enums;

namespace Octocare.Domain.Entities;

public class UserOrgMembership
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrganisationId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User User { get; private set; } = null!;
    public Organisation Organisation { get; private set; } = null!;

    private UserOrgMembership() { }

    public static UserOrgMembership Create(Guid userId, Guid organisationId, string role)
    {
        if (!OrgRole.IsValid(role))
            throw new ArgumentException($"Invalid role: {role}", nameof(role));

        return new UserOrgMembership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganisationId = organisationId,
            TenantId = organisationId,
            Role = role,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateRole(string role)
    {
        if (!OrgRole.IsValid(role))
            throw new ArgumentException($"Invalid role: {role}", nameof(role));

        Role = role;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
