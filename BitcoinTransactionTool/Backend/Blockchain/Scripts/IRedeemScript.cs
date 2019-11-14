// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts
{
    public interface IRedeemScript : IScript
    {
        /// <summary>
        /// Returns type of this redeem script instance.
        /// </summary>
        /// <returns><see cref="RedeemScriptType"/> enum</returns>
        RedeemScriptType GetRedeemScriptType();
    }
}
