using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Bot_Application1
{
    public class UnhandledCommandsEntity : TableEntity
    {
        private static string partitionKey = "ARandomkeyAlwaysUsedForThisPartition";
        public string QueryResultJson { get; set; }

        private LuisResult QueryResult
        {
            get
            {
                return string.IsNullOrEmpty(this.QueryResultJson)
                    ? null
                    : JsonConvert.DeserializeObject<LuisResult>(this.QueryResultJson);
            }
        }

        public UnhandledCommandsEntity(LuisResult result) : base(partitionKey, result.Query)
        {
            this.QueryResultJson = JsonConvert.SerializeObject(result);
        }
    }
}