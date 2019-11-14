// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operation to push the size of the top stack item to the stack.
    /// </summary>
    public class SizeOp : IOperation
    {
        public OP OpValue => OP.SIZE;

        /// <summary>
        /// Pushes the size of the top stack item to the stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = "There was not enough items left in the stack.";
                return false;
            }

            byte[] temp = opData.Peek();
            opData.Push(OpHelper.IntToByteArray(temp.Length));

            error = null;
            return true;
        }
    }

}
