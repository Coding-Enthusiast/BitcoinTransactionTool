// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operation to check equality of top two stack items.
    /// </summary>
    public class EqualOp : BaseOperation
    {
        public override OP OpValue => OP.EQUAL;

        /// <summary>
        /// Removes top two stack item and pushes (true for equality and false otherwiwe) onto the stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2)
            {
                error = "There was not enough items left in the stack to check.";
                return false;
            }

            byte[] item1 = opData.Pop();
            byte[] item2 = opData.Pop();

            if (item1.IsEqualTo(item2))
            {
                opData.Push(new byte[] { 1 });
            }
            else
            {
                opData.Push(new byte[0]);
            }

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation to check and verify equality of top two stack items.
    /// </summary>
    public class EqualVerifyOp : BaseOperation
    {
        public override OP OpValue => OP.EqualVerify;

        /// <summary>
        /// Removes top two stack item checks their equality, fails if not equal. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2)
            {
                error = "There was not enough items left in the stack to check.";
                return false;
            }

            byte[] item1 = opData.Pop();
            byte[] item2 = opData.Pop();

            if (item1.IsEqualTo(item2))
            {
                error = null;
                return true;
            }
            else
            {
                error = "Items were not equal.";
                return false;
            }

            // ^^ this way we skip unnecessary OP instantiation and Push() and Pop() operations on IOpData

            //EqualOp eq = new EqualOp();
            //VerifyOp ver = new VerifyOp();
            //if (!eq.Run(opData, out error))
            //{
            //    return false;
            //}
            //return ver.Run(opData, out error);
        }
    }
}
