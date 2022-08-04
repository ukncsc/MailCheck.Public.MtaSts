using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Contracts.PolicyFetcher;
using MailCheck.MtaSts.PolicyFetcher.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace MailCheck.MtaSts.PolicyFetcher.Parsing
{
    public interface IMtaStsPolicyParser
    {
        MtaStsPolicyResult Parse(string domain, string responseBody, List<AdvisoryMessage> errors);
    }

    public class MtaStsPolicyParser : IMtaStsPolicyParser
    {
        private readonly Dictionary<string, IKeyParser> _parsers;
        private readonly ILogger<PolicyFetcher> _log;

        public MtaStsPolicyParser(IEnumerable<IKeyParser> parsers, ILogger<PolicyFetcher> log)
        {
            _parsers = parsers.ToDictionary(_ => _.KeyType, _ => _);
            _log = log;
        }

        public MtaStsPolicyResult Parse(string domain, string responseBody, List<AdvisoryMessage> errors)
        {
            errors ??= new List<AdvisoryMessage>();

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                if (!errors.Any())
                {
                    errors.Add(new NoPolicyError());
                }
                
                return new MtaStsPolicyResult(responseBody, new List<Key>(), errors.OfType<MtaStsAdvisoryMessage>().ToList());
            }

            List<Key> keys = new List<Key>();

            try
            {
                using (StringReader sr = new StringReader(responseBody))
                {
                    string line;
                    while (!string.IsNullOrWhiteSpace(line = sr.ReadLine()))
                    {
                        List<string> keyParts = (line.Split(":")).ToList();

                        if (keyParts.Count == 2)
                        {
                            string keyKey = keyParts[0];
                            string keyValue = keyParts[1].Trim();
                            if (_parsers.TryGetValue(keyKey, out IKeyParser keyParser))
                            {
                                EvaluationResult<Key> key = keyParser.Parse(keys, line, keyKey, keyValue);

                                int tagInstanceCount = keys.Count(_ => _.GetType() == key.Item.GetType());

                                if (tagInstanceCount == keyParser.MaxOccurrences)
                                {
                                    errors.Add(new MaxOccurrencesExceededError(keyKey, keyParser.MaxOccurrences, tagInstanceCount));
                                }

                                keys.Add(key.Item);
                                errors.AddRange(key.AdvisoryMessages);
                            }
                            else
                            {
                                UnknownKey key = new UnknownKey(keyValue, line);
                                key.Explanation = "Invalid key";
                                keys.Add(key);
                                errors.Add(new InvalidKeyError(keyKey));
                            }
                        }
                        else
                        {
                            errors.Add(new KeyValueNotFound(line));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Failed parsing policy file for domain {domain}. " +
                    $"Error: {e?.Message} InnerException: {e?.InnerException?.Message}");
                errors.Add(new FailedToParse());
            }

            return new MtaStsPolicyResult(responseBody, keys, errors.OfType<MtaStsAdvisoryMessage>().ToList());
        }
    }
}
