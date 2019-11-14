// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace BitcoinTransactionTool.Backend.Encoders
{
    /// <summary>
    /// Base-43 is a special encoding based on <see cref="Base58"/> encoding that Electrum uses to encode transactions 
    /// before turning them into QR code.
    /// </summary>
    public class Base43 : Base58
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Base43"/> using parameters of the given <see cref="ICoin"/>
        /// and using default Base-43 characters.
        /// </summary>
        public Base43() : base()
        {
            // https://github.com/spesmilo/electrum/blob/b39c51adf7ef9d56bd45b1c30a86d4d415ef7940/electrum/bitcoin.py#L428
            b58Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ$*+-./:";

            baseValue = 43;
            logBaseValue = 679;
        }
    }
}
