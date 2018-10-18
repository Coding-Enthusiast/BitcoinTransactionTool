using BitcoinTransactionTool.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BitcoinTransactionTool.Services
{
    public enum TxApiNames
    {
        BlockCypher,
        BlockchainInfo,
    }

    public abstract class Api
    {
        protected async Task<Response<JObject>> SendApiRequestAsync(string url)
        {
            Response<JObject> resp = new Response<JObject>();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string result = await client.GetStringAsync(url);
                    if (result.StartsWith("["))
                    {
                        resp.Result = new JObject
                        {
                            { "Result", JArray.Parse(result) }
                        };
                    }
                    else
                    {
                        resp.Result = JObject.Parse(result);
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = (ex.InnerException == null) ?
                        ex.Message :
                        ex.Message + Environment.NewLine + ex.InnerException.Message;
                    resp.Errors.Add(errMsg);
                }
            }
            return resp;
        }
    }

    public abstract class TransactionApi : Api
    {
        public abstract Task<Response<List<UTXO>>> GetUTXO(List<SendingAddress> addrList);
    }
}
