namespace QKey.Core;

public readonly record struct ReplacementPlan(string Text, int BackspaceCount)
{
    public static ReplacementPlan ForCurrentWord(string rendered, string text, int? backspaceCount = null)
    {
        return new ReplacementPlan(text, Math.Max(0, backspaceCount ?? rendered.Length));
    }
}
