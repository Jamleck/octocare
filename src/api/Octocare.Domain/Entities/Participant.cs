namespace Octocare.Domain.Entities;

public class Participant
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string NdisNumber { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public string? NomineeName { get; private set; }
    public string? NomineeEmail { get; private set; }
    public string? NomineePhone { get; private set; }
    public string? NomineeRelationship { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    private Participant() { }

    public static Participant Create(Guid tenantId, string ndisNumber, string firstName, string lastName,
        DateOnly dateOfBirth, string? email = null, string? phone = null, string? address = null,
        string? nomineeName = null, string? nomineeEmail = null, string? nomineePhone = null,
        string? nomineeRelationship = null)
    {
        return new Participant
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            NdisNumber = ndisNumber,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Email = email,
            Phone = phone,
            Address = address,
            NomineeName = nomineeName,
            NomineeEmail = nomineeEmail,
            NomineePhone = nomineePhone,
            NomineeRelationship = nomineeRelationship,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string firstName, string lastName, DateOnly dateOfBirth,
        string? email, string? phone, string? address,
        string? nomineeName, string? nomineeEmail, string? nomineePhone,
        string? nomineeRelationship)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Email = email;
        Phone = phone;
        Address = address;
        NomineeName = nomineeName;
        NomineeEmail = nomineeEmail;
        NomineePhone = nomineePhone;
        NomineeRelationship = nomineeRelationship;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
