using BitcoinTransactionTool.Models;
using CommonLibrary;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitcoinTransactionTool.Services.TransactionServices
{
    public class BlockchainInfo : TransactionApi
    {
        public override async Task<Response<List<UTXO>>> GetUTXO(List<SendingAddress> addrList)
        {
            Response<List<UTXO>> resp = new Response<List<UTXO>>();
            string addresses = string.Join("|", addrList.Select(b => b.Address).ToArray());
            string url = "https://blockchain.info/unspent?active=" + addresses;
            Response<JObject> apiResp = await SendApiRequestAsync(url);
            if (apiResp.Errors.Any())
            {
                resp.Errors.AddRange(apiResp.Errors);
                return resp;
            }

            resp.Result = new List<UTXO>();
            foreach (var item in apiResp.Result["unspent_outputs"])
            {
                UTXO u = new UTXO();
                string script = item["script"].ToString();
                u.AddressHash160 = script.Substring("76a914".Length, script.Length - "76a91488ac".Length);
                u.Address = BitcoinConversions.Hash160ToBase58(u.AddressHash160);
                u.TxHash = item["tx_hash_big_endian"].ToString();
                u.Amount = (ulong)item["value"];
                u.Confirmation = (int)item["confirmations"];
                u.OutIndex = (uint)item["tx_output_n"];
                resp.Result.Add(u);
            }

            return resp;
        }

    }
}
