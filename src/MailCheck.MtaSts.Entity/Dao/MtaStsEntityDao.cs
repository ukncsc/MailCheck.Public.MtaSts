using System;
using System.Threading.Tasks;
using Dapper;
using MailCheck.Common.Data;
using MailCheck.MtaSts.Entity.Entity;
using Newtonsoft.Json;

namespace MailCheck.MtaSts.Entity.Dao
{
    public interface IMtaStsEntityDao
    {
        Task<MtaStsEntityState> Get(string domainName);
        Task Upsert(MtaStsEntityState state);
        Task Delete(string domainName);
    }

    public class MtaStsEntityDao : IMtaStsEntityDao
    {


        private readonly IDatabase _database;

        public MtaStsEntityDao(IDatabase database)
        {
            _database = database;
        }

        public async Task Delete(string domain)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                await connection.ExecuteAsync(MtaStsEntityDaoResources.DeleteMtaStsEntity, new { domain });
            }
        }

        public async Task<MtaStsEntityState> Get(string domain)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                string state = await connection.QueryFirstOrDefaultAsync<string>(MtaStsEntityDaoResources.SelectMtaStsEntity, new { domain });
                return state == null ? null : JsonConvert.DeserializeObject<MtaStsEntityState>(state);
            }
        }

        public async Task Upsert(MtaStsEntityState state)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                await connection.ExecuteAsync(MtaStsEntityDaoResources.UpsertMtaStsEntity, new { domain = state.Id.ToLower(), version = state.Version, state = JsonConvert.SerializeObject(state) });
            }
        }
    }
}
