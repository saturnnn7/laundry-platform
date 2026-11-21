using Laundry.BuildingBlocks.Domain;

namespace Laundry.Tests.BuildingBlocks;

public class ResultTests
{
    private static readonly Error SampleError = Error.NotFound("thing.not-found", "Thing was not found.");

    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_CarriesItsError()
    {
        var result = Result.Failure(SampleError);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void Failure_WithNoneError_Throws()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
    }

    [Fact]
    public void GenericSuccess_HasValueAndNoError()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void GenericFailure_CarriesItsError()
    {
        var result = Result.Failure<int>(SampleError);

        Assert.True(result.IsFailure);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void GenericFailure_WithNoneError_Throws()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure<int>(Error.None));
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        var result = Result.Failure<string>(SampleError);

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void GenericResult_WithValueAndError_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Result<int>(1, true, SampleError));
    }

    [Fact]
    public void ImplicitConversion_FromValue_GivesSuccess()
    {
        Result<string> result = "ok";

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_GivesFailure()
    {
        Result<string> result = SampleError;

        Assert.True(result.IsFailure);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void ErrorFactories_SetTheType()
    {
        Assert.Equal(ErrorType.Failure, Error.Failure("c", "d").Type);
        Assert.Equal(ErrorType.Validation, Error.Validation("c", "d").Type);
        Assert.Equal(ErrorType.NotFound, Error.NotFound("c", "d").Type);
        Assert.Equal(ErrorType.Conflict, Error.Conflict("c", "d").Type);
        Assert.Equal(ErrorType.Unauthorized, Error.Unauthorized("c", "d").Type);
        Assert.Equal(ErrorType.Forbidden, Error.Forbidden("c", "d").Type);
    }
}
