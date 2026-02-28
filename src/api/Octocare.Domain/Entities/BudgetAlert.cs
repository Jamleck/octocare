using Octocare.Domain.Enums;

namespace Octocare.Domain.Entities;

public class BudgetAlert
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public Guid? BudgetCategoryId { get; private set; }
    public AlertType AlertType { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public bool IsDismissed { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public string? Data { get; private set; } // JSON for extra context

    public Plan Plan { get; private set; } = null!;
    public BudgetCategory? BudgetCategory { get; private set; }

    private BudgetAlert() { }

    public static BudgetAlert Create(Guid tenantId, Guid planId, Guid? budgetCategoryId,
        AlertType alertType, AlertSeverity severity, string message, string? data = null)
    {
        return new BudgetAlert
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanId = planId,
            BudgetCategoryId = budgetCategoryId,
            AlertType = alertType,
            Severity = severity,
            Message = message,
            IsRead = false,
            IsDismissed = false,
            Data = data,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkRead()
    {
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
    }

    public void Dismiss()
    {
        IsDismissed = true;
    }
}
