using ContactsAPI.Application.AuditLogs.Queries.GetAdminActionsSummary;
using Xunit;

namespace ContactsAPI.Test.Validators.AdminValidators;

public class GetAdminActionsSummaryFilterValidatorTests
{
    private readonly GetAdminActionsSummaryFilterValidator _validator = new();

    // ── Future dates ──────────────────────────────────────────────────────────

    [Fact]
    public void Validate_FutureFromDate_Fails()
    {
        var filter = new AdminActionSummaryFilter
        {
            FromDate = DateTime.UtcNow.AddDays(5),
            Page = 1,
            PageSize = 10
        };

        var result = _validator.Validate(filter);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "From Date cannot be in the future");
    }

    [Fact]
    public void Validate_FutureToDate_Fails()
    {
        var filter = new AdminActionSummaryFilter
        {
            ToDate = DateTime.UtcNow.AddDays(5),
            Page = 1,
            PageSize = 10
        };

        var result = _validator.Validate(filter);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "To Date cannot be in the future");
    }

    // ── ToDate before FromDate ────────────────────────────────────────────────

    [Fact]
    public void Validate_ToDateBeforeFromDate_Fails()
    {
        var from = DateTime.UtcNow.AddDays(-5);
        var to = from.AddDays(-2);

        var filter = new AdminActionSummaryFilter
        {
            FromDate = from,
            ToDate = to,
            Page = 1,
            PageSize = 10
        };

        var result = _validator.Validate(filter);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "To Date must be greater than or equal to From Date");
    }

    // ── Pagination ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PageLessThanOne_Fails(int page)
    {
        var result = _validator.Validate(
            new AdminActionSummaryFilter { Page = page, PageSize = 10 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "Page must be greater than 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_PageSizeOutOfRange_Fails(int size)
    {
        var result = _validator.Validate(
            new AdminActionSummaryFilter { Page = 1, PageSize = size });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "PageSize must be between 1 and 100");
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidFilter_Passes()
    {
        var result = _validator.Validate(new AdminActionSummaryFilter
        {
            FromDate = DateTime.UtcNow.AddDays(-7),
            ToDate = DateTime.UtcNow.AddDays(-1),
            Page = 1,
            PageSize = 20
        });

        Assert.True(result.IsValid);
    }
}