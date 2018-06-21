using System.ComponentModel;

namespace CLP.Entities
{
    public enum Correctness
    {
        Correct,
        [Description("Partially Correct")]
        PartiallyCorrect,
        Incorrect,
        Illegible,
        Unanswered,
        Unknown
    }
}
