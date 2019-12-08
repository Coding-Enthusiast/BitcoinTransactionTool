// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    public abstract class LockTimeOp : BaseOperation
    {
        /// <summary>
        /// The locktime value converted from the top stack item (without popping it)
        /// </summary>
        protected long lt;

        protected bool TrySetLockTime(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = "Not enough items left in the stack.";
                return false;
            }

            // The two locktime OPs (CheckLocktimeVerify and CheckSequenceVerify) used to be NOPs. NOPs don't do anything.
            // For backward compatibility of the softfork, Run Peeks at the top item of the stack instead of Poping it.
            byte[] data = opData.Peek();

            // TODO: move this check to OpHelper
            // (for locktimes max length is 5 for others it is 4)
            if (data.Length > 5)
            {
                error = "Data length for locktimes can not be bigger than 5.";
                return false;
            }

            if (!OpHelper.TryConvertByteArrayToInt(data, out lt, true))
            {
                error = "Invalid number format.";
                return false;
            }

            if (lt < 0)
            {
                error = "Locktime can not be negative.";
                return false;
            }

            error = null;
            return true;
        }
    }



    public class CheckLocktimeVerifyOp : LockTimeOp
    {
        public override OP OpValue => OP.CheckLocktimeVerify;

        public override bool Run(IOpData opData, out string error)
        {
            if (!TrySetLockTime(opData, out error))
            {
                return false;
            }

            // Compare to tx.locktime (we assume it is valid and skip this!)
            // TODO: change this for this tool if transactions were set inside IOpdata one day...

            error = null;
            return true;
        }
    }



    public class CheckSequenceVerifyOp : LockTimeOp
    {
        public override OP OpValue => OP.CheckSequenceVerify;

        public override bool Run(IOpData opData, out string error)
        {
            if (!TrySetLockTime(opData, out error))
            {
                return false;
            }

            // Compare to tx.locktime, as relative locktime (we assume it is valid and skip this!)
            // TODO: change this for this tool if transactions were set inside IOpdata one day...

            error = null;
            return true;
        }
    }
}
