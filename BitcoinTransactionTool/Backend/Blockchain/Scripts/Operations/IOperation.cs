// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Defines any kind of operation that is placed inside <see cref="Script"/>s and could be run.
    /// </summary>
    public interface IOperation
    {
        /// <summary>
        /// A single byte inticating type of the opeartion
        /// </summary>
        OP OpValue { get; }

        /// <summary>
        /// Performs the action defined by the operation on the stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data used by the <see cref="IOperation"/>s (an advanced form of <see cref="System.Collections.Stack"/>)</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        bool Run(IOpData opData, out string error);
    }
}
