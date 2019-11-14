// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    public class CheckLocktimeVerifyOp : BaseOperation
    {
        public override OP OpValue => OP.CheckLocktimeVerify;

        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = "Not enough items left on the stack.";
                return false;
            }

            // OP_CheckLocktimeVerify was a NOP that was activated through a soft fork. NOPs don't do anything, 
            // for backward compatibility of the softfork, Run Peeks at the top item of the stack instead of Poping it
            byte[] ltBa = opData.Peek();
            // TODO: iterpret this as LockTime
            // compare to tx.locktime



            throw new NotImplementedException();
        }
    }
}
