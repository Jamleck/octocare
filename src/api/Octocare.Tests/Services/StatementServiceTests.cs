using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Application.Services;
using Octocare.Domain.Entities;

namespace Octocare.Tests.Services;

public class StatementServiceTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task GenerateStatementAsync_ValidInput_ReturnsStatement()
    {
        // Arrange
        var participant = Participant.Create(_tenantId, "431234567", "Sarah", "Johnson",
            new DateOnly(1985, 3, 15), email: "sarah@test.com");

        var plan = Plan.Create(_tenantId, participant.Id, "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
        plan.Activate();

        var statementRepo = new FakeStatementRepository();
        var participantRepo = new FakeParticipantRepository(participant);
        var planRepo = new FakePlanRepository(new List<Plan> { plan });
        var orgRepo = new FakeOrganisationRepository(null);
        var invoiceRepo = new FakeInvoiceRepository();
        var projectionRepo = new FakeBudgetProjectionRepository();
        var currentUser = new FakeCurrentUserService(_tenantId);
        var emailSender = new FakeEmailSender();
        var pdfGenerator = new StatementPdfGenerator();

        var service = new StatementService(statementRepo, participantRepo, planRepo,
            orgRepo, invoiceRepo, projectionRepo, currentUser, emailSender, pdfGenerator);

        var request = new GenerateStatementRequest(plan.Id,
            new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 31));

        // Act
        var result = await service.GenerateStatementAsync(participant.Id, request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(participant.Id, result.ParticipantId);
        Assert.Equal(plan.Id, result.PlanId);
        Assert.Equal(new DateOnly(2025, 10, 1), result.PeriodStart);
        Assert.Equal(new DateOnly(2025, 10, 31), result.PeriodEnd);
        Assert.Null(result.SentAt);
    }

    [Fact]
    public async Task GenerateStatementAsync_ParticipantNotFound_ThrowsKeyNotFound()
    {
        var plan = Plan.Create(_tenantId, Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));

        var statementRepo = new FakeStatementRepository();
        var participantRepo = new FakeParticipantRepository(null);
        var planRepo = new FakePlanRepository(new List<Plan> { plan });
        var orgRepo = new FakeOrganisationRepository(null);
        var invoiceRepo = new FakeInvoiceRepository();
        var projectionRepo = new FakeBudgetProjectionRepository();
        var currentUser = new FakeCurrentUserService(_tenantId);
        var emailSender = new FakeEmailSender();
        var pdfGenerator = new StatementPdfGenerator();

        var service = new StatementService(statementRepo, participantRepo, planRepo,
            orgRepo, invoiceRepo, projectionRepo, currentUser, emailSender, pdfGenerator);

        var request = new GenerateStatementRequest(plan.Id,
            new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 31));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GenerateStatementAsync(Guid.NewGuid(), request, CancellationToken.None));
    }

    [Fact]
    public async Task GenerateStatementAsync_PlanNotFound_ThrowsKeyNotFound()
    {
        var participant = Participant.Create(_tenantId, "431234567", "Sarah", "Johnson",
            new DateOnly(1985, 3, 15));

        var statementRepo = new FakeStatementRepository();
        var participantRepo = new FakeParticipantRepository(participant);
        var planRepo = new FakePlanRepository(new List<Plan>());
        var orgRepo = new FakeOrganisationRepository(null);
        var invoiceRepo = new FakeInvoiceRepository();
        var projectionRepo = new FakeBudgetProjectionRepository();
        var currentUser = new FakeCurrentUserService(_tenantId);
        var emailSender = new FakeEmailSender();
        var pdfGenerator = new StatementPdfGenerator();

        var service = new StatementService(statementRepo, participantRepo, planRepo,
            orgRepo, invoiceRepo, projectionRepo, currentUser, emailSender, pdfGenerator);

        var request = new GenerateStatementRequest(Guid.NewGuid(),
            new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 31));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GenerateStatementAsync(participant.Id, request, CancellationToken.None));
    }

    [Fact]
    public async Task GetStatementsAsync_ReturnsStatements()
    {
        var participant = Participant.Create(_tenantId, "431234567", "Sarah", "Johnson",
            new DateOnly(1985, 3, 15));

        var plan = Plan.Create(_tenantId, participant.Id, "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));

        var statement = ParticipantStatement.Create(_tenantId, participant.Id, plan.Id,
            new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 31));

        var statementRepo = new FakeStatementRepository();
        statementRepo.AddStatement(statement);
        var participantRepo = new FakeParticipantRepository(participant);
        var planRepo = new FakePlanRepository(new List<Plan> { plan });
        var orgRepo = new FakeOrganisationRepository(null);
        var invoiceRepo = new FakeInvoiceRepository();
        var projectionRepo = new FakeBudgetProjectionRepository();
        var currentUser = new FakeCurrentUserService(_tenantId);
        var emailSender = new FakeEmailSender();
        var pdfGenerator = new StatementPdfGenerator();

        var service = new StatementService(statementRepo, participantRepo, planRepo,
            orgRepo, invoiceRepo, projectionRepo, currentUser, emailSender, pdfGenerator);

        var results = await service.GetStatementsAsync(participant.Id, CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(statement.Id, results[0].Id);
    }

    [Fact]
    public async Task GenerateBatchAsync_GeneratesForAllActivePlans()
    {
        var participant1 = Participant.Create(_tenantId, "431234567", "Sarah", "Johnson",
            new DateOnly(1985, 3, 15));
        var participant2 = Participant.Create(_tenantId, "432345678", "Michael", "Chen",
            new DateOnly(1992, 7, 22));

        var plan1 = Plan.Create(_tenantId, participant1.Id, "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
        plan1.Activate();

        var plan2 = Plan.Create(_tenantId, participant2.Id, "NDIS-2025-002",
            new DateOnly(2025, 4, 1), new DateOnly(2026, 3, 31));
        plan2.Activate();

        var statementRepo = new FakeStatementRepository();
        var participantRepo = new FakeParticipantRepository(participant1);
        var planRepo = new FakePlanRepository(new List<Plan> { plan1, plan2 });
        var orgRepo = new FakeOrganisationRepository(null);
        var invoiceRepo = new FakeInvoiceRepository();
        var projectionRepo = new FakeBudgetProjectionRepository();
        var currentUser = new FakeCurrentUserService(_tenantId);
        var emailSender = new FakeEmailSender();
        var pdfGenerator = new StatementPdfGenerator();

        var service = new StatementService(statementRepo, participantRepo, planRepo,
            orgRepo, invoiceRepo, projectionRepo, currentUser, emailSender, pdfGenerator);

        var results = await service.GenerateBatchAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
    }

    #region Test Doubles

    private class FakeStatementRepository : IStatementRepository
    {
        private readonly List<ParticipantStatement> _statements = new();

        public void AddStatement(ParticipantStatement statement) => _statements.Add(statement);

        public Task<ParticipantStatement?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_statements.FirstOrDefault(s => s.Id == id));

        public Task<IReadOnlyList<ParticipantStatement>> GetByParticipantIdAsync(Guid participantId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<ParticipantStatement>>(
                _statements.Where(s => s.ParticipantId == participantId).ToList());

        public Task AddAsync(ParticipantStatement statement, CancellationToken ct = default)
        {
            _statements.Add(statement);
            return Task.CompletedTask;
        }

        public Task SaveAsync(CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private class FakeParticipantRepository : IParticipantRepository
    {
        private readonly Participant? _participant;

        public FakeParticipantRepository(Participant? participant) => _participant = participant;

        public Task<Participant?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_participant);

        public Task<(IReadOnlyList<Participant> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? search = null, CancellationToken ct = default)
            => Task.FromResult<(IReadOnlyList<Participant>, int)>(
                (_participant is not null ? new List<Participant> { _participant } : new List<Participant>(), _participant is not null ? 1 : 0));

        public Task<bool> ExistsByNdisNumberAsync(string ndisNumber, Guid? excludeId = null, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<IReadOnlyList<Participant>> GetAllActiveAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Participant>>(
                _participant is not null ? new List<Participant> { _participant } : new List<Participant>());

        public Task<Participant> AddAsync(Participant participant, CancellationToken ct = default)
            => Task.FromResult(participant);

        public Task UpdateAsync(Participant participant, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private class FakePlanRepository : IPlanRepository
    {
        private readonly List<Plan> _plans;

        public FakePlanRepository(List<Plan> plans) => _plans = plans;

        public Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_plans.FirstOrDefault(p => p.Id == id));

        public Task<Plan?> GetByIdWithBudgetCategoriesAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_plans.FirstOrDefault(p => p.Id == id));

        public Task<IReadOnlyList<Plan>> GetByParticipantIdAsync(Guid participantId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Plan>>(_plans.Where(p => p.ParticipantId == participantId).ToList());

        public Task<Plan?> GetActivePlanForParticipantAsync(Guid participantId, CancellationToken ct = default)
            => Task.FromResult(_plans.FirstOrDefault(p => p.ParticipantId == participantId && p.Status == "active"));

        public Task<bool> ExistsByPlanNumberAsync(string planNumber, Guid? excludeId = null, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<Plan> AddAsync(Plan plan, CancellationToken ct = default)
            => Task.FromResult(plan);

        public Task UpdateAsync(Plan plan, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task SaveAsync(CancellationToken ct = default)
            => Task.CompletedTask;

        public Task AddBudgetCategoryAsync(BudgetCategory category, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<Plan>> GetActivePlansAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Plan>>(_plans.Where(p => p.Status == "active").ToList());
    }

    private class FakeOrganisationRepository : IOrganisationRepository
    {
        private readonly Organisation? _org;

        public FakeOrganisationRepository(Organisation? org) => _org = org;

        public Task<Organisation?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_org);

        public Task<Organisation> AddAsync(Organisation organisation, CancellationToken ct = default)
            => Task.FromResult(organisation);

        public Task UpdateAsync(Organisation organisation, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private class FakeInvoiceRepository : IInvoiceRepository
    {
        public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult<Invoice?>(null);

        public Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? status = null, Guid? participantId = null, Guid? providerId = null, CancellationToken ct = default)
            => Task.FromResult<(IReadOnlyList<Invoice>, int)>((new List<Invoice>(), 0));

        public Task<bool> ExistsByInvoiceNumberAsync(string invoiceNumber, Guid? excludeId = null, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<Invoice> AddAsync(Invoice invoice, CancellationToken ct = default)
            => Task.FromResult(invoice);

        public Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task SaveAsync(CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private class FakeBudgetProjectionRepository : IBudgetProjectionRepository
    {
        public Task<IReadOnlyList<BudgetProjection>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<BudgetProjection>>(new List<BudgetProjection>());

        public Task<BudgetProjection?> GetByCategoryIdAsync(Guid budgetCategoryId, CancellationToken ct = default)
            => Task.FromResult<BudgetProjection?>(null);

        public Task<BudgetProjection> AddAsync(BudgetProjection projection, CancellationToken ct = default)
            => Task.FromResult(projection);

        public Task SaveAsync(CancellationToken ct = default)
            => Task.CompletedTask;

        public Task<Dictionary<Guid, long>> GetCommittedAmountsByPlanAsync(Guid planId, CancellationToken ct = default)
            => Task.FromResult(new Dictionary<Guid, long>());

        public Task<Dictionary<Guid, long>> GetSpentAmountsByPlanAsync(Guid planId, CancellationToken ct = default)
            => Task.FromResult(new Dictionary<Guid, long>());

        public Task<Dictionary<Guid, long>> GetPendingAmountsByPlanAsync(Guid planId, CancellationToken ct = default)
            => Task.FromResult(new Dictionary<Guid, long>());
    }

    private class FakeCurrentUserService : ICurrentUserService
    {
        public FakeCurrentUserService(Guid tenantId) => TenantId = tenantId;

        public string? ExternalUserId => "test-user";
        public Guid? TenantId { get; }

        public Task<User?> GetUserAsync(CancellationToken ct = default)
            => Task.FromResult<User?>(null);

        public Task<string?> GetRoleAsync(CancellationToken ct = default)
            => Task.FromResult<string?>("org_admin");

        public Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default)
            => Task.FromResult(true);
    }

    private class FakeEmailSender : IEmailSender
    {
        public List<(string To, string Subject, string Body, string? AttachmentName)> SentEmails { get; } = new();

        public Task SendAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null, CancellationToken ct = default)
        {
            SentEmails.Add((to, subject, body, attachmentName));
            return Task.CompletedTask;
        }
    }

    #endregion
}
