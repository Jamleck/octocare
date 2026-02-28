using System.Text.Json;
using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Application.Services;

public class PlanTransitionService
{
    private static readonly string[] DefaultChecklistLabels =
    [
        "Review current budget utilisation",
        "Identify active service agreements",
        "Create new plan",
        "Migrate service bookings",
        "Notify providers",
        "Notify participant"
    ];

    private readonly IPlanTransitionRepository _transitionRepo;
    private readonly IPlanRepository _planRepo;
    private readonly ITenantContext _tenantContext;

    public PlanTransitionService(
        IPlanTransitionRepository transitionRepo,
        IPlanRepository planRepo,
        ITenantContext tenantContext)
    {
        _transitionRepo = transitionRepo;
        _planRepo = planRepo;
        _tenantContext = tenantContext;
    }

    public async Task<PlanTransitionDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var transition = await _transitionRepo.GetByIdAsync(id, ct);
        return transition is null ? null : MapToDto(transition);
    }

    public async Task<IReadOnlyList<PlanTransitionDto>> GetByPlanIdAsync(Guid planId, CancellationToken ct)
    {
        var transitions = await _transitionRepo.GetByPlanIdAsync(planId, ct);
        return transitions.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<PlanTransitionDto>> GetAllAsync(CancellationToken ct)
    {
        var transitions = await _transitionRepo.GetAllAsync(ct);
        return transitions.Select(MapToDto).ToList();
    }

    public async Task<PlanTransitionDto> InitiateTransitionAsync(Guid oldPlanId, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(oldPlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        if (plan.Status != PlanStatus.Active && plan.Status != PlanStatus.Expiring)
            throw new InvalidOperationException(
                $"Cannot initiate transition for plan with status '{plan.Status}'. Plan must be Active or Expiring.");

        // Create default checklist
        var checklistItems = DefaultChecklistLabels
            .Select(label => new TransitionChecklistItemDto(label, false))
            .ToList();

        var checklistJson = JsonSerializer.Serialize(checklistItems);
        var transition = PlanTransition.Create(tenantId, oldPlanId, checklistJson);

        await _transitionRepo.AddAsync(transition, ct);

        // Re-fetch with navigation properties
        var saved = await _transitionRepo.GetByIdAsync(transition.Id, ct);
        return MapToDto(saved!);
    }

    public async Task<PlanTransitionDto> UpdateChecklistAsync(Guid id, UpdateTransitionRequest request, CancellationToken ct)
    {
        var transition = await _transitionRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Plan transition not found.");

        if (transition.Status == Domain.Enums.PlanTransitionStatus.Completed)
            throw new InvalidOperationException("Cannot update a completed transition.");

        var checklistJson = JsonSerializer.Serialize(request.ChecklistItems);
        transition.UpdateChecklist(checklistJson);

        if (request.Notes is not null)
            transition.UpdateNotes(request.Notes);

        await _transitionRepo.SaveAsync(ct);

        return MapToDto(transition);
    }

    public async Task<PlanTransitionDto> CompleteTransitionAsync(Guid id, CancellationToken ct)
    {
        var transition = await _transitionRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Plan transition not found.");

        transition.Complete();

        // Mark the old plan as transitioned
        var oldPlan = await _planRepo.GetByIdAsync(transition.OldPlanId, ct);
        if (oldPlan is not null &&
            (oldPlan.Status == PlanStatus.Active || oldPlan.Status == PlanStatus.Expiring))
        {
            oldPlan.Transition();
            await _planRepo.UpdateAsync(oldPlan, ct);
        }

        await _transitionRepo.SaveAsync(ct);

        return MapToDto(transition);
    }

    private static PlanTransitionDto MapToDto(PlanTransition transition)
    {
        var checklistItems = DeserializeChecklist(transition.ChecklistItems);

        return new PlanTransitionDto(
            transition.Id,
            transition.OldPlanId,
            transition.OldPlan?.PlanNumber ?? string.Empty,
            transition.NewPlanId,
            transition.NewPlan?.PlanNumber,
            transition.Status.ToString(),
            checklistItems,
            transition.Notes,
            transition.CreatedAt,
            transition.CompletedAt);
    }

    private static IReadOnlyList<TransitionChecklistItemDto> DeserializeChecklist(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<TransitionChecklistItemDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
