// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    public class CheckSigOp : BaseOperation
    {
        public override OP OpValue => OP.CheckSig;

        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2)
            {
                error = "Invalid number of elements in stack.";
                return false;
            }

            byte[] pubBa = opData.Pop();
            byte[] sigBa = opData.Pop();

            // "fake" pass!
            opData.Push(new byte[] { 1 });

            error = null;
            return true;
        }
    }

    public class CheckSigVerifyOp : BaseOperation
    {
        public override OP OpValue => OP.CheckSigVerify;

        public override bool Run(IOpData opData, out string error)
        {
            IOperation cs = new CheckSigOp();
            IOperation ver = new VerifyOp();

            if (!cs.Run(opData, out error))
            {
                return false;
            }

            return ver.Run(opData, out error);
        }
    }

    public class CheckMultiSigOp : BaseOperation
    {
        public override OP OpValue => OP.CheckMultiSig;

        public override bool Run(IOpData opData, out string error)
        {
            error = null;
            return true; // "fake" pass
        }
    }

    public class CheckMultiSigVerifyOp : BaseOperation
    {
        public override OP OpValue => OP.CheckMultiSigVerify;

        public override bool Run(IOpData opData, out string error)
        {
            IOperation cms = new CheckMultiSigOp();
            IOperation ver = new VerifyOp();

            if (!cms.Run(opData, out error))
            {
                return false;
            }

            return ver.Run(opData, out error);
        }
    }
}
