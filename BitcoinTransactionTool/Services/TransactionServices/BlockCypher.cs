using BitcoinTransactionTool.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitcoinTransactionTool.Services.TransactionServices
{
    public class BlockCypher : TransactionApi
    {
        public override async Task<Response<List<UTXO>>> GetUTXO(List<SendingAddress> addrList)
        {
            Response<List<UTXO>> resp = new Response<List<UTXO>>();
            string addresses = string.Join(";", addrList.Select(b => b.Address).ToArray());
            string url = $"https://api.blockcypher.com/v1/btc/main/addrs/{addresses}?unspentOnly=true";
            Response<JObject> apiResp = await SendApiRequestAsync(url);
            if (apiResp.Errors.Any())
            {
                resp.Errors.AddRange(apiResp.Errors);
                return resp;
            }

            resp.Result = new List<UTXO>();
            if (apiResp.Result["Result"] != null)
            {
                foreach (var item in apiResp.Result["Result"])
                {
                    if ((int)item["final_n_tx"] != 0)
                    {
                        foreach (var tx in item["txrefs"])
                        {
                            UTXO u = new UTXO()
                            {
                                Address = item["address"].ToString(),
                                AddressHash160 = "",
                                TxHash = tx["tx_hash"].ToString(),
                                Amount = (ulong)tx["value"],
                                Confirmation = (int)tx["confirmations"],
                                OutIndex = (uint)tx["tx_output_n"]
                            };
                            resp.Result.Add(u);
                        }
                    }
                }
            }
            else if ((int)apiResp.Result["final_n_tx"] != 0)
            {
                foreach (var tx in apiResp.Result["txrefs"])
                {
                    UTXO u = new UTXO()
                    {
                        Address = apiResp.Result["address"].ToString(),
                        AddressHash160 = "",
                        TxHash = tx["tx_hash"].ToString(),
                        Amount = (ulong)tx["value"],
                        Confirmation = (int)tx["confirmations"],
                        OutIndex = (uint)tx["tx_output_n"]
                    };
                    resp.Result.Add(u);
                }
            }

            return resp;
        }
    }
}
