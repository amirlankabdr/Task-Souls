namespace QuestForge.Core;

public static class QuestDraftValidation
{
    public static Result<QuestDraft> Validate(QuestDraft draft) =>
        QuestValidation.Title(draft.Title)
            .Map(title => draft with { Title = title, Description = (draft.Description ?? "").Trim(), Skill = (draft.Skill ?? "").Trim() })
            .Bind(d => string.IsNullOrWhiteSpace(d.Skill)
                ? Result<QuestDraft>.Failure("Skill is required.") : Result<QuestDraft>.Success(d))
            .Bind(d => d.Skill.Length > 40
                ? Result<QuestDraft>.Failure("Skill must be 40 characters or fewer.") : Result<QuestDraft>.Success(d))
            .Bind(d => d.Description.Length > 1000
                ? Result<QuestDraft>.Failure("Description must be 1000 characters or fewer.") : Result<QuestDraft>.Success(d))
            .Bind(d => !Enum.IsDefined(d.Difficulty)
                ? Result<QuestDraft>.Failure("Choose a valid difficulty.") : Result<QuestDraft>.Success(d));
}
