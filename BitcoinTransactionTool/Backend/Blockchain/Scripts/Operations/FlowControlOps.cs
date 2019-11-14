// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    public abstract class IfElseOp : BaseOperation
    {
        protected abstract bool ShouldRunIfTopItemIs { get; }

        protected internal IOperation[] mainOps;
        protected internal IOperation[] elseOps;


        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = "Invalid number of elements in stack.";
                return false;
            }

            // TODO: check what this part is 
            // https://github.com/bitcoin/bitcoin/blob/a822a0e4f6317f98cde6f0d5abe952b4e8992ac9/src/script/interpreter.cpp#L478-L483
            // probably have to include it in whever we run these operations.

            // Remove top stack item and interpret it as bool
            // OP_IF: runs for True
            // OP_NOTIF: runs for false
            bool isRunnable = OpHelper.IsNotZero(opData.Pop()) & ShouldRunIfTopItemIs; // Note: this is a bitwise operation not logical
            if (isRunnable)
            {
                foreach (var op in mainOps)
                {
                    if (!op.Run(opData, out error))
                    {
                        return false;
                    }
                }
            }
            else if (elseOps != null && elseOps.Length != 0)
            {
                foreach (var op in elseOps)
                {
                    if (!op.Run(opData, out error))
                    {
                        return false;
                    }
                }
            }

            error = null;
            return true;
        }


        public byte[] ToByteArray()
        {
            byte[] result = { (byte)OpValue };
            foreach (var op in mainOps)
            {
                if (op is PushDataOp)
                {
                    result = result.ConcatFast(((PushDataOp)op).ToByteArray(false));
                }
                else if (op is IfElseOp)
                {
                    result = result.ConcatFast(((IfElseOp)op).ToByteArray());
                }
                else
                {
                    result = result.AppendToEnd((byte)op.OpValue);
                }
            }

            if (elseOps != null && elseOps.Length != 0)
            {
                result = result.AppendToEnd((byte)OP.ELSE);
                foreach (var op in elseOps)
                {
                    if (op is PushDataOp)
                    {
                        result = result.ConcatFast(((PushDataOp)op).ToByteArray(false));
                    }
                    else if (op is IfElseOp)
                    {
                        result = result.ConcatFast(((IfElseOp)op).ToByteArray());
                    }
                    else
                    {
                        result = result.AppendToEnd((byte)op.OpValue);
                    }
                }
            }

            result = result.AppendToEnd((byte)OP.EndIf);
            return result;
        }


        public override bool Equals(object obj)
        {
            if (obj is IfElseOp)
            {
                if (((IfElseOp)obj).OpValue == OpValue)
                {
                    if (((IfElseOp)obj).mainOps.Length == mainOps.Length)
                    {
                        for (int i = 0; i < mainOps.Length; i++)
                        {
                            if (!((IfElseOp)obj).mainOps[i].Equals(mainOps[i]))
                            {
                                return false;
                            }
                        }

                        if (((IfElseOp)obj).elseOps == null && elseOps != null ||
                            ((IfElseOp)obj).elseOps != null && elseOps == null ||
                            ((IfElseOp)obj).elseOps != null && elseOps != null && ((IfElseOp)obj).elseOps.Length != elseOps.Length)
                        {
                            return false;
                        }
                        if (((IfElseOp)obj).elseOps != null && elseOps != null && ((IfElseOp)obj).elseOps.Length == elseOps.Length)
                        {
                            for (int i = 0; i < elseOps.Length; i++)
                            {
                                if (!((IfElseOp)obj).elseOps[i].Equals(elseOps[i]))
                                {
                                    return false;
                                }
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }


        public override int GetHashCode()
        {
            int result = 17;
            foreach (var item in mainOps)
            {
                result ^= item.GetHashCode();
            }
            if (elseOps != null)
            {
                foreach (var item in elseOps)
                {
                    result ^= item.GetHashCode();
                }
            }

            return result;
        }

    }
    /// <summary>
    /// When running IFOp the code must check the "WasExecuted" bool to know whether IF block was executed or not (if not run else).
    /// The Run() returned bool is showing the success of running the block not the result of the block.
    /// </summary>
    public class IFOp : IfElseOp
    {
        internal IFOp()
        {
        }
        public IFOp(IOperation[] opsBlockIf, IOperation[] opsBlockElse)// TODO: if elseOps were null set it to an empty array (equals will fail if you don't)
        {
            mainOps = opsBlockIf;
            elseOps = opsBlockElse;
        }

        public override OP OpValue => OP.IF;
        protected sealed override bool ShouldRunIfTopItemIs => true;
    }

    public class NotIfOp : IfElseOp
    {
        internal NotIfOp()
        {
        }
        public NotIfOp(IOperation[] opsBlock, IOperation[] opsBlockElse)
        {
            mainOps = opsBlock;
            elseOps = opsBlockElse;
        }

        public override OP OpValue => OP.NotIf;
        protected sealed override bool ShouldRunIfTopItemIs => false;
    }
}
