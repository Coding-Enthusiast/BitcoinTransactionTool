// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend;
using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using BitcoinTransactionTool.Backend.Encoders;

namespace BitcoinTransactionTool.Services
{
    public class ScriptReader
    {
        private class Helper : Script
        {
            public Helper()
            {
                IsWitness = false;
            }
        }

        public Response<IOperation[]> Read(string hex)
        {
            Response<IOperation[]> resp = new Response<IOperation[]>();

            if (!Base16.IsValid(hex))
            {
                resp.Errors.Add("Invalid base-16 string.");
            }
            else
            {
                byte[] data = Base16.ToByteArray(hex);
                int offset = 0;
                CompactInt len = new CompactInt(data.Length);
                data = len.ToByteArray().ConcatFast(data);
                
                Script scr = new Helper();
                if (scr.TryDeserialize(data, ref offset, out string error))
                {
                    resp.Result = scr.OperationList;
                }
                else
                {
                    resp.Errors.Add(error);
                }
            }            

            return resp;
        }
    }
}
