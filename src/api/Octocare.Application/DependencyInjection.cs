using Microsoft.Extensions.DependencyInjection;
using Octocare.Application.Services;

namespace Octocare.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<OrganisationService>();
        services.AddScoped<MemberService>();
        services.AddScoped<ParticipantService>();
        services.AddScoped<ProviderService>();
        services.AddScoped<PriceGuideService>();
        services.AddScoped<PlanService>();
        services.AddScoped<ServiceAgreementService>();
        services.AddScoped<InvoiceService>();
        services.AddScoped<InvoiceValidationService>();
        services.AddScoped<BudgetProjectionService>();
        services.AddScoped<PmFeeService>();
        services.AddScoped<ClaimService>();
        services.AddScoped<NdiaCsvExporter>();
        services.AddScoped<PaymentService>();
        services.AddScoped<AbaFileGenerator>();
        services.AddScoped<AlertService>();
        services.AddScoped<PlanTransitionService>();
        services.AddScoped<ProdaSyncService>();

        return services;
    }
}
