namespace Octocare.Domain.Entities;

/// <summary>
/// An NDIS service provider. Providers are shared across tenants —
/// they do NOT have a TenantId. Tenant-specific linkage is via TenantProviderRelationship.
/// </summary>
public class Provider
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Abn { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? Address { get; private set; }
    public string? Bsb { get; private set; }
    public string? AccountNumber { get; private set; }
    public string? AccountName { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Provider() { }

    public static Provider Create(string name, string? abn = null, string? contactEmail = null,
        string? contactPhone = null, string? address = null)
    {
        return new Provider
        {
            Id = Guid.NewGuid(),
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

    public void UpdateBankDetails(string? bsb, string? accountNumber, string? accountName)
    {
        Bsb = bsb;
        AccountNumber = accountNumber;
        AccountName = accountName;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
