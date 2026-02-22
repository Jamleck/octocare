namespace Octocare.Domain.Entities;

public class Organisation
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Abn { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public ICollection<UserOrgMembership> Memberships { get; private set; } = [];

    private Organisation() { }

    public static Organisation Create(string name, string? abn = null, string? contactEmail = null,
        string? contactPhone = null, string? address = null)
    {
        var id = Guid.NewGuid();
        return new Organisation
        {
            Id = id,
            TenantId = id, // org IS the tenant
            Name = name,
            Abn = abn,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            Address = address,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string name, string? abn, string? contactEmail, string? contactPhone, string? address)
    {
        Name = name;
        Abn = abn;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        Address = address;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
