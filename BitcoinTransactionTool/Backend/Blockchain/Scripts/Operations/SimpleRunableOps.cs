// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Base abstract class for operations that don't do anything with the stack.
    /// Implements a finalized Run() and has overrides for Equals() and GetHashCode() functions from <see cref="BaseOperation"/> class.
    /// </summary>
    public abstract class SimpleRunableOps : BaseOperation
    {
        /// <summary>
        /// Doesn't do anything.
        /// </summary>
        /// <param name="opData">Stack object (won't be used)</param>
        /// <param name="error">Error message (always null)</param>
        /// <returns>True (always successful)</returns>
        public sealed override bool Run(IOpData opData, out string error)
        {
            error = null;
            return true;
        }
    }



    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOPOp : SimpleRunableOps
    {
        public override OP OpValue => OP.NOP;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP1Op : SimpleRunableOps
    {
        public override OP OpValue => OP.NOP1;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP4Op : SimpleRunableOps
    {
        public override OP OpValue => OP.NOP4;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP5Op : SimpleRunableOps
    {
        public override OP OpValue => OP.NOP5;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP6Op : SimpleRunableOps
    {
        public override OP OpValue => OP.NOP6;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP7Op : SimpleRunableOps
    {
        public override OP OpValue => OP.NOP7;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP8Op : SimpleRunableOps
    {
        public override OP OpValue => OP.NOP8;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP9Op : SimpleRunableOps
    {
        public override OP OpValue => OP.NOP9;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP10Op : SimpleRunableOps
    {
        public override OP OpValue => OP.NOP10;
    }

}
