// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operation to verify if the top stack item is true.
    /// </summary>
    public class VerifyOp : BaseOperation
    {
        public override OP OpValue => OP.VERIFY;

        /// <summary>
        /// Removes top stack item only passes if it is true. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = "There was not enough items left in the stack.";
                return false;
            }
            // Check the top stack value, only fail if False
            bool b = OpHelper.IsNotZero(opData.Pop());
            if (!b)
            {
                error = "Top stack item value was 'false'.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
