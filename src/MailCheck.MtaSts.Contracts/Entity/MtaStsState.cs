using System;
namespace MailCheck.MtaSts.Contracts.Entity
{
    public enum MtaStsState
    {
        Created,
        PollPending,
        EvaluationPending,
        Unchanged,
        Evaluated
    }
}
