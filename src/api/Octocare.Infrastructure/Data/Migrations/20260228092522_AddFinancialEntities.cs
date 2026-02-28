using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Octocare.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    total_amount = table.Column<long>(type: "bigint", nullable: false),
                    ndia_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    submission_date = table.Column<DateOnly>(type: "date", nullable: true),
                    response_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claims", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "communication_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    template_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    related_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    related_entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_communication_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "email_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stream_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stream_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    total_amount = table.Column<long>(type: "bigint", nullable: false),
                    aba_file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_batches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_plans_participants_participant_id",
                        column: x => x.participant_id,
                        principalTable: "participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "price_guide_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_guide_versions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    abn = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    bsb = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    account_number = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    account_name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_providers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_provider_relationships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_provider_relationships", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "budget_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    support_category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    support_purpose = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    allocated_amount = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_budget_categories_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "participant_statements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    generated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    pdf_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participant_statements", x => x.id);
                    table.ForeignKey(
                        name: "FK_participant_statements_participants_participant_id",
                        column: x => x.participant_id,
                        principalTable: "participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_participant_statements_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plan_transitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    new_plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    checklist_items = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_transitions", x => x.id);
                    table.ForeignKey(
                        name: "FK_plan_transitions_plans_new_plan_id",
                        column: x => x.new_plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_plan_transitions_plans_old_plan_id",
                        column: x => x.old_plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "support_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    support_category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    support_purpose = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    price_limit_national = table.Column<long>(type: "bigint", nullable: false),
                    price_limit_remote = table.Column<long>(type: "bigint", nullable: false),
                    price_limit_very_remote = table.Column<long>(type: "bigint", nullable: false),
                    is_ttp_eligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    cancellation_rule = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    claim_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_support_items_price_guide_versions_version_id",
                        column: x => x.version_id,
                        principalTable: "price_guide_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    service_period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    service_period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    total_amount = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoices_participants_participant_id",
                        column: x => x.participant_id,
                        principalTable: "participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    invoice_ids = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    remittance_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_items_payment_batches_payment_batch_id",
                        column: x => x.payment_batch_id,
                        principalTable: "payment_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payment_items_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "service_agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    signed_document_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_agreements", x => x.id);
                    table.ForeignKey(
                        name: "FK_service_agreements_participants_participant_id",
                        column: x => x.participant_id,
                        principalTable: "participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_service_agreements_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_service_agreements_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "budget_alerts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    budget_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alert_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_dismissed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    data = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_alerts", x => x.id);
                    table.ForeignKey(
                        name: "FK_budget_alerts_budget_categories_budget_category_id",
                        column: x => x.budget_category_id,
                        principalTable: "budget_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_budget_alerts_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "budget_projections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    budget_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    allocated_amount = table.Column<long>(type: "bigint", nullable: false),
                    committed_amount = table.Column<long>(type: "bigint", nullable: false),
                    spent_amount = table.Column<long>(type: "bigint", nullable: false),
                    pending_amount = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_projections", x => x.id);
                    table.ForeignKey(
                        name: "FK_budget_projections_budget_categories_budget_category_id",
                        column: x => x.budget_category_id,
                        principalTable: "budget_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_line_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    support_item_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    service_date = table.Column<DateOnly>(type: "date", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    rate = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    budget_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    validation_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    validation_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_line_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_line_items_budget_categories_budget_category_id",
                        column: x => x.budget_category_id,
                        principalTable: "budget_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_line_items_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_agreement_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    support_item_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    agreed_rate = table.Column<long>(type: "bigint", nullable: false),
                    frequency = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_agreement_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_service_agreement_items_service_agreements_service_agreemen~",
                        column: x => x.service_agreement_id,
                        principalTable: "service_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    budget_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    allocated_amount = table.Column<long>(type: "bigint", nullable: false),
                    used_amount = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_bookings", x => x.id);
                    table.ForeignKey(
                        name: "FK_service_bookings_budget_categories_budget_category_id",
                        column: x => x.budget_category_id,
                        principalTable: "budget_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_service_bookings_service_agreements_service_agreement_id",
                        column: x => x.service_agreement_id,
                        principalTable: "service_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "claim_line_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_line_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_line_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_line_items_claims_claim_id",
                        column: x => x.claim_id,
                        principalTable: "claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_claim_line_items_invoice_line_items_invoice_line_item_id",
                        column: x => x.invoice_line_item_id,
                        principalTable: "invoice_line_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_budget_alerts_budget_category_id",
                table: "budget_alerts",
                column: "budget_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_budget_alerts_plan_id",
                table: "budget_alerts",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_budget_alerts_tenant_id_is_read_is_dismissed",
                table: "budget_alerts",
                columns: new[] { "tenant_id", "is_read", "is_dismissed" });

            migrationBuilder.CreateIndex(
                name: "IX_budget_alerts_tenant_id_plan_id",
                table: "budget_alerts",
                columns: new[] { "tenant_id", "plan_id" });

            migrationBuilder.CreateIndex(
                name: "IX_budget_categories_plan_id",
                table: "budget_categories",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_budget_projections_budget_category_id",
                table: "budget_projections",
                column: "budget_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_claim_line_items_claim_id",
                table: "claim_line_items",
                column: "claim_id");

            migrationBuilder.CreateIndex(
                name: "IX_claim_line_items_invoice_line_item_id",
                table: "claim_line_items",
                column: "invoice_line_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_claims_tenant_id_batch_number",
                table: "claims",
                columns: new[] { "tenant_id", "batch_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_communication_logs_recipient_email",
                table: "communication_logs",
                column: "recipient_email");

            migrationBuilder.CreateIndex(
                name: "IX_communication_logs_tenant_id_sent_at",
                table: "communication_logs",
                columns: new[] { "tenant_id", "sent_at" });

            migrationBuilder.CreateIndex(
                name: "IX_email_templates_tenant_id_name",
                table: "email_templates",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_event_type",
                table: "events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_events_stream_id",
                table: "events",
                column: "stream_id");

            migrationBuilder.CreateIndex(
                name: "IX_events_stream_id_version",
                table: "events",
                columns: new[] { "stream_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_line_items_budget_category_id",
                table: "invoice_line_items",
                column: "budget_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_line_items_invoice_id",
                table: "invoice_line_items",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_participant_id",
                table: "invoices",
                column: "participant_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_plan_id",
                table: "invoices",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_provider_id",
                table: "invoices",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_tenant_id_invoice_number",
                table: "invoices",
                columns: new[] { "tenant_id", "invoice_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_tenant_id_participant_id",
                table: "invoices",
                columns: new[] { "tenant_id", "participant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_invoices_tenant_id_provider_id",
                table: "invoices",
                columns: new[] { "tenant_id", "provider_id" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_tenant_id_user_id_is_read",
                table: "notifications",
                columns: new[] { "tenant_id", "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id_created_at",
                table: "notifications",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_participant_statements_participant_id",
                table: "participant_statements",
                column: "participant_id");

            migrationBuilder.CreateIndex(
                name: "IX_participant_statements_plan_id",
                table: "participant_statements",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_participant_statements_tenant_id_participant_id",
                table: "participant_statements",
                columns: new[] { "tenant_id", "participant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_payment_batches_tenant_id_batch_number",
                table: "payment_batches",
                columns: new[] { "tenant_id", "batch_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_items_payment_batch_id",
                table: "payment_items",
                column: "payment_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_items_provider_id",
                table: "payment_items",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_transitions_new_plan_id",
                table: "plan_transitions",
                column: "new_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_transitions_old_plan_id",
                table: "plan_transitions",
                column: "old_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_transitions_tenant_id_old_plan_id",
                table: "plan_transitions",
                columns: new[] { "tenant_id", "old_plan_id" });

            migrationBuilder.CreateIndex(
                name: "IX_plans_participant_id",
                table: "plans",
                column: "participant_id");

            migrationBuilder.CreateIndex(
                name: "IX_plans_tenant_id_participant_id",
                table: "plans",
                columns: new[] { "tenant_id", "participant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_plans_tenant_id_plan_number",
                table: "plans",
                columns: new[] { "tenant_id", "plan_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_price_guide_versions_name",
                table: "price_guide_versions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_agreement_items_service_agreement_id",
                table: "service_agreement_items",
                column: "service_agreement_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_agreements_participant_id",
                table: "service_agreements",
                column: "participant_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_agreements_plan_id",
                table: "service_agreements",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_agreements_provider_id",
                table: "service_agreements",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_agreements_tenant_id_participant_id",
                table: "service_agreements",
                columns: new[] { "tenant_id", "participant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_service_bookings_budget_category_id",
                table: "service_bookings",
                column: "budget_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_bookings_service_agreement_id",
                table: "service_bookings",
                column: "service_agreement_id");

            migrationBuilder.CreateIndex(
                name: "IX_support_items_version_id",
                table: "support_items",
                column: "version_id");

            migrationBuilder.CreateIndex(
                name: "IX_support_items_version_id_item_number",
                table: "support_items",
                columns: new[] { "version_id", "item_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_provider_relationships_tenant_id",
                table: "tenant_provider_relationships",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_provider_relationships_tenant_id_provider_id",
                table: "tenant_provider_relationships",
                columns: new[] { "tenant_id", "provider_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "budget_alerts");

            migrationBuilder.DropTable(
                name: "budget_projections");

            migrationBuilder.DropTable(
                name: "claim_line_items");

            migrationBuilder.DropTable(
                name: "communication_logs");

            migrationBuilder.DropTable(
                name: "email_templates");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "participant_statements");

            migrationBuilder.DropTable(
                name: "payment_items");

            migrationBuilder.DropTable(
                name: "plan_transitions");

            migrationBuilder.DropTable(
                name: "service_agreement_items");

            migrationBuilder.DropTable(
                name: "service_bookings");

            migrationBuilder.DropTable(
                name: "support_items");

            migrationBuilder.DropTable(
                name: "tenant_provider_relationships");

            migrationBuilder.DropTable(
                name: "claims");

            migrationBuilder.DropTable(
                name: "invoice_line_items");

            migrationBuilder.DropTable(
                name: "payment_batches");

            migrationBuilder.DropTable(
                name: "service_agreements");

            migrationBuilder.DropTable(
                name: "price_guide_versions");

            migrationBuilder.DropTable(
                name: "budget_categories");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropTable(
                name: "providers");
        }
    }
}
