using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.MtaSts.Api.Domain;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.MtaSts.Api.Dao
{
    public interface IMtaStsApiDao
    {
        Task<List<MtaStsInfoResponse>> GetMtaStsForDomains(List<string> domains);
        Task<MtaStsInfoResponse> GetMtaStsForDomain(string domain);
        Task<string> GetMtaStsHistory(string domain);
    }

    public class MtaStsApiDao : IMtaStsApiDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public MtaStsApiDao(IConnectionInfoAsync connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<List<MtaStsInfoResponse>> GetMtaStsForDomains(List<string> domain)
        {
            string query = string.Format(MtaStsApiDaoResources.SelectMtaStsStates,
                string.Join(',', domain.Select((_, i) => $"@domain{i}")));

            MySqlParameter[] parameters = domain
                .Select((_, i) => new MySqlParameter($"domain{i}", _))
                .ToArray();

            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(connectionString, query, parameters))
            {
                List<MtaStsInfoResponse> states = new List<MtaStsInfoResponse>();

                while (await reader.ReadAsync())
                {
                    if (!reader.IsDbNull("state"))
                    {
                        states.Add(JsonConvert.DeserializeObject<MtaStsInfoResponse>(reader.GetString("state"),
                            _serializerSettings));
                    }
                }

                return states;
            }
        }

        public async Task<MtaStsInfoResponse> GetMtaStsForDomain(string domain)
        {
            List<MtaStsInfoResponse> responses = await GetMtaStsForDomains(new List<string>{domain});
            return responses.FirstOrDefault();
        }

        public async Task<string> GetMtaStsHistory(string domain)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            return (string) await MySqlHelper.ExecuteScalarAsync(connectionString,
                MtaStsApiDaoResources.SelectMtaStsHistoryStates, new MySqlParameter("domain", domain));
        }
    }
}