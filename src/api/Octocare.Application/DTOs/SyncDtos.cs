namespace Octocare.Application.DTOs;

public record SyncResult(
    bool InSync,
    List<SyncDiscrepancy> Discrepancies);

public record SyncDiscrepancy(
    string Field,
    string LocalValue,
    string ProdaValue,
    string Severity);
