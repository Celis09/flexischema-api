using ContactsAPI.Application.Users.Queries.GetAllUsers;
using Xunit;

namespace ContactsAPI.Test.Validators.UsersValidators;

public class GetAllUsersQueryValidatorTests
{
    private readonly GetAllUsersQueryValidator _validator = new();

    // ── Page ──────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PageLessThanOne_Fails(int page)
    {
        var result = _validator.Validate(
            new GetAllUsersQuery { Page = page, PageSize = 10 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Page must be at least 1.");
    }

    // ── PageSize ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_PageSizeOutOfRange_Fails(int size)
    {
        var result = _validator.Validate(
            new GetAllUsersQuery { Page = 1, PageSize = size });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "Page Size must be between 1 and 100.");
    }

    // ── SortOrder ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("ascending")]
    [InlineData("random")]
    public void Validate_InvalidSortOrder_Fails(string sortOrder)
    {
        var result = _validator.Validate(
            new GetAllUsersQuery { Page = 1, PageSize = 10, SortOrder = sortOrder });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "Sort Order must be 'asc' or 'desc'.");
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    [InlineData("ASC")]
    [InlineData(null)]
    public void Validate_ValidOrNullSortOrder_Passes(string? sortOrder)
    {
        var result = _validator.Validate(
            new GetAllUsersQuery { Page = 1, PageSize = 10, SortOrder = sortOrder });

        Assert.True(result.IsValid);
    }

    // ── Date range ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ToDateBeforeFromDate_Fails()
    {
        var from = DateTime.UtcNow.AddDays(-1).Date;
        var to = from.AddDays(-2);

        var result = _validator.Validate(new GetAllUsersQuery
        {
            Page = 1,
            PageSize = 10,
            FromDate = from,
            ToDate = to
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "To Date must be greater than or equal to From Date.");
    }

    [Fact]
    public void Validate_FutureFromDate_Fails()
    {
        var result = _validator.Validate(new GetAllUsersQuery
        {
            Page = 1,
            PageSize = 10,
            FromDate = DateTime.UtcNow.AddDays(5)
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "From Date cannot be in the future.");
    }

    [Fact]
    public void Validate_ValidQuery_Passes()
    {
        var result = _validator.Validate(new GetAllUsersQuery
        {
            Page = 1,
            PageSize = 20,
            SortOrder = "asc"
        });

        Assert.True(result.IsValid);
    }
}