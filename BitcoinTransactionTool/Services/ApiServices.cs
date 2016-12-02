using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

using BitcoinTransactionTool.Models;
using CommonLibrary;
using Newtonsoft.Json.Linq;

namespace BitcoinTransactionTool.Services
{
    public enum ApiNames
    {
        BlockchainInfo,
        BlockrIO
    }
    public interface IApiService
    {
        Task<List<UTXO>> GetUTXO(List<SendingAddress> addrList);
    }
    public class BlockchainInfoApi : IApiService
    {
        public async Task<List<UTXO>> GetUTXO(List<SendingAddress> addrList)
        {
            using (var client = new HttpClient())
            {
                // Make sure all balances are 0
                foreach (var ad in addrList)
                {
                    ad.BalanceSatoshi = 0;
                }
                var addresses = string.Join("|", addrList.Select(b => b.Address).ToArray());
                string url = "https://blockchain.info/unspent?active=" + addresses;
                string result = await client.GetStringAsync(url);
                JObject jResult = JObject.Parse(result);
                List<UTXO> utxList = new List<UTXO>();
                foreach (var item in jResult["unspent_outputs"])
                {
                    UTXO u = new UTXO();
                    string script = item["script"].ToString();
                    u.AddressHash160 = script.Substring("76a914".Length, script.Length - "76a91488ac".Length);
                    u.Address = BitcoinConversions.Hash160ToBase58(u.AddressHash160);
                    u.TxHash = item["tx_hash_big_endian"].ToString();
                    u.Amount = (int)item["value"];
                    u.Confirmation = (int)item["confirmations"];
                    u.OutIndex = (UInt32)item["tx_output_n"];
                    utxList.Add(u);
                    // Update address balance
                    foreach (var ad in addrList)
                    {
                        if (ad.Address == u.Address)
                        {
                            ad.BalanceSatoshi += u.Amount;
                        }
                    }
                }

                return utxList;
            }
        }
    }

    public class BlockrApi : IApiService
    {
        public async Task<List<UTXO>> GetUTXO(List<SendingAddress> addrList)
        {
            using (var client = new HttpClient())
            {
                // Make sure all balances are 0
                foreach (var ad in addrList)
                {
                    ad.BalanceSatoshi = 0;
                }

                var addresses = string.Join(",", addrList.Select(b => b.Address).ToArray());
                string url = "https://btc.blockr.io/api/v1/address/unspent/" + addresses;
                string result = await client.GetStringAsync(url);
                JObject jResult = JObject.Parse(result);
                if (jResult["status"].ToString() != "success")
                {
                    string msg = string.Format("Response does not include success!\nCode: {0}\nMessage: {1}", jResult["code"], jResult["message"]);
                    throw new Exception(msg);
                }
                List<UTXO> utxList = new List<UTXO>();
                //Blockr returns array for more than 1 addr and normal for 1!
                if (addrList.Count == 1)
                {
                    utxList.AddRange(DeserU(jResult["data"]));
                }
                else
                {
                    foreach (var item in jResult["data"])
                    {
                        utxList.AddRange(DeserU(item));
                    }
                }

                return utxList;
            }
        }


        public static List<UTXO> DeserU(JToken j)
        {
            List<UTXO> uList = new List<UTXO>();
            foreach (var item in j["unspent"])
            {
                UTXO u = new UTXO();
                u.Address = j["address"].ToString();
                string script = item["script"].ToString();
                u.AddressHash160 = script.Substring(6, script.Length - 10);
                u.TxHash = item["tx"].ToString();
                u.OutIndex = (UInt32)item["n"];
                u.Confirmation = (int)item["confirmations"];
                u.Amount = (int)((decimal)item["amount"] * 100000000);
                uList.Add(u);
            }
            return uList;
        }
    }
}
