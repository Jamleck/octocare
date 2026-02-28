using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

public class PlanService
{
    private readonly IPlanRepository _planRepo;
    private readonly IParticipantRepository _participantRepo;
    private readonly ITenantContext _tenantContext;
    private readonly IEventStore _eventStore;

    public PlanService(IPlanRepository planRepo, IParticipantRepository participantRepo,
        ITenantContext tenantContext, IEventStore eventStore)
    {
        _planRepo = planRepo;
        _participantRepo = participantRepo;
        _tenantContext = tenantContext;
        _eventStore = eventStore;
    }

    public async Task<PlanDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(id, ct);
        return plan is null ? null : MapToDto(plan);
    }

    public async Task<IReadOnlyList<PlanDto>> GetByParticipantIdAsync(Guid participantId, CancellationToken ct)
    {
        var plans = await _planRepo.GetByParticipantIdAsync(participantId, ct);
        return plans.Select(MapToDto).ToList();
    }

    public async Task<PlanDto> CreateAsync(Guid participantId, CreatePlanRequest request, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var participant = await _participantRepo.GetByIdAsync(participantId, ct)
            ?? throw new KeyNotFoundException("Participant not found.");

        if (await _planRepo.ExistsByPlanNumberAsync(request.PlanNumber, ct: ct))
            throw new InvalidOperationException("A plan with this plan number already exists.");

        var plan = Plan.Create(tenantId, participantId, request.PlanNumber,
            request.StartDate, request.EndDate);

        await _planRepo.AddAsync(plan, ct);

        await _eventStore.AppendAsync(
            plan.Id,
            "Plan",
            "PlanCreated",
            new { plan.PlanNumber, plan.StartDate, plan.EndDate, plan.ParticipantId },
            0,
            null,
            ct);

        // Re-fetch with participant navigation to populate ParticipantName
        var saved = await _planRepo.GetByIdWithBudgetCategoriesAsync(plan.Id, ct);
        return MapToDto(saved!);
    }

    public async Task<PlanDto> UpdateAsync(Guid id, UpdatePlanRequest request, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(id, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        if (request.PlanNumber != plan.PlanNumber &&
            await _planRepo.ExistsByPlanNumberAsync(request.PlanNumber, id, ct))
            throw new InvalidOperationException("A plan with this plan number already exists.");

        plan.Update(request.PlanNumber, request.StartDate, request.EndDate);
        await _planRepo.UpdateAsync(plan, ct);

        return MapToDto(plan);
    }

    public async Task<PlanDto> ActivateAsync(Guid id, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(id, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        plan.Activate();
        await _planRepo.UpdateAsync(plan, ct);

        var events = await _eventStore.GetStreamAsync(plan.Id, ct);
        await _eventStore.AppendAsync(
            plan.Id,
            "Plan",
            "PlanActivated",
            new { plan.PlanNumber, plan.Status },
            events.Count,
            null,
            ct);

        return MapToDto(plan);
    }

    public async Task<BudgetCategoryDto> AddBudgetCategoryAsync(Guid planId, CreateBudgetCategoryRequest request, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdAsync(planId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        var amountCents = Money.FromDollars(request.AllocatedAmount).Cents;
        var category = BudgetCategory.Create(planId, request.SupportCategory,
            request.SupportPurpose, amountCents);

        await _planRepo.AddBudgetCategoryAsync(category, ct);

        var events = await _eventStore.GetStreamAsync(plan.Id, ct);
        await _eventStore.AppendAsync(
            plan.Id,
            "Plan",
            "BudgetAllocated",
            new { category.SupportCategory, category.SupportPurpose, AllocatedAmountCents = amountCents },
            events.Count,
            null,
            ct);

        return MapBudgetCategoryToDto(category);
    }

    public async Task<BudgetCategoryDto> UpdateBudgetCategoryAsync(Guid planId, Guid categoryId,
        UpdateBudgetCategoryRequest request, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(planId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        var category = plan.BudgetCategories.FirstOrDefault(bc => bc.Id == categoryId)
            ?? throw new KeyNotFoundException("Budget category not found.");

        var amountCents = Money.FromDollars(request.AllocatedAmount).Cents;
        category.UpdateAllocation(amountCents);
        await _planRepo.SaveAsync(ct);

        return MapBudgetCategoryToDto(category);
    }

    private static PlanDto MapToDto(Plan plan)
    {
        return new PlanDto(
            plan.Id,
            plan.ParticipantId,
            plan.Participant?.FullName ?? string.Empty,
            plan.PlanNumber,
            plan.StartDate,
            plan.EndDate,
            plan.Status,
            plan.BudgetCategories.Select(MapBudgetCategoryToDto).ToList(),
            plan.CreatedAt);
    }

    private static BudgetCategoryDto MapBudgetCategoryToDto(BudgetCategory bc)
    {
        return new BudgetCategoryDto(
            bc.Id,
            bc.SupportCategory,
            bc.SupportPurpose,
            new Money(bc.AllocatedAmount).ToDollars());
    }
}
