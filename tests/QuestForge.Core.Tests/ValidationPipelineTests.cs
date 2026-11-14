using QuestForge.Core;
namespace QuestForge.Core.Tests;
public sealed class ValidationPipelineTests
{
    [Fact] public void Failure_skips_both_callbacks() {
        var calls = 0;
        var result = Result<int>.Failure("bad").Map(x => { calls++; return x + 1; })
            .Bind(x => { calls++; return Result<int>.Success(x); });
        Assert.Equal(0, calls); Assert.Equal("bad", result.Error);
    }
    [Fact] public void Success_transforms_and_bind_can_fail() {
        var result = Result<int>.Success(2).Map(x => x * 3).Bind(x => Result<int>.Failure($"Rejected {x}"));
        Assert.Equal("Rejected 6", result.Error);
    }
    [Fact] public void Validation_returns_clean_copy() {
        var original = new QuestDraft("  Learn  ", "  C#  ", QuestDifficulty.Easy, " Programming ");
        var result = QuestDraftValidation.Validate(original);
        Assert.True(result.IsSuccess); Assert.Equal("Learn", result.Value!.Title);
        Assert.Equal("C#", result.Value.Description); Assert.Equal("Programming", result.Value.Skill);
        Assert.Equal("  Learn  ", original.Title);
    }
    [Theory]
    [InlineData(" ", "Programming", QuestDifficulty.Easy)]
    [InlineData("Test", " ", QuestDifficulty.Easy)]
    [InlineData("Test", "Programming", (QuestDifficulty)999)]
    public void Rejects_bad_input(string title, string skill, QuestDifficulty difficulty) {
        Assert.False(QuestDraftValidation.Validate(new(title, "", difficulty, skill)).IsSuccess);
    }
    [Fact] public void Enforces_storage_limits() {
        Assert.False(QuestDraftValidation.Validate(new(new string('x',101), "", QuestDifficulty.Easy, "Programming")).IsSuccess);
        Assert.False(QuestDraftValidation.Validate(new("Test", "", QuestDifficulty.Easy, new string('x',41))).IsSuccess);
    }
}
