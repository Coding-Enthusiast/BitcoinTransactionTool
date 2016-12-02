using System.Net.Http;
using System.Threading.Tasks;

namespace BitcoinTransactionTool.Services
{
    public interface IPushService
    {
        Task<string> PushTx(string txToPush);
    }
    //public class BlockexplorerApi : IPushService
    //{
    //    public async Task<string> PushTx(string txToPush)
    //    {
    //        using (HttpClient client = new HttpClient())
    //        {
    //            string url = "https://blockexplorer.com/api/tx/send";
    //            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);
    //            req.Headers.Add("rawtx", txToPush);
    //            var result = await client.SendAsync(req);
    //            return await result.Content.ReadAsStringAsync();
    //        }
    //    }
    //}
}
