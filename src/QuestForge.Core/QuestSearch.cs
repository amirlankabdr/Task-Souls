namespace QuestForge.Core;

public static class QuestSearch
{
    // PostgreSQL LIKE treats %, _ and \ as special characters. Search input is literal text.
    public static string ToLikePattern(string text) =>
        $"%{text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%";
}
