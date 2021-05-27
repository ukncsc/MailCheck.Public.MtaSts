using System;
using System.Collections.Generic;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Keys;

namespace MailCheck.MtaSts.PolicyFetcher.Parsing
{
    public interface IKeyParser
    {
        EvaluationResult<Key> Parse(List<Key> keys, string line, string key, string value);
        string KeyType { get; }
        int MaxOccurrences { get; }
    }
}
