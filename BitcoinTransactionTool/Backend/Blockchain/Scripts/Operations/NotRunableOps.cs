// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Base abstract class for operations that can not be run 
    /// (should fail when <see cref="IOperation.Run(IOpData, out string)"/> is called).
    /// Implements a finalized Run() and has overrides for Equals() and GetHashCode() functions from <see cref="BaseOperation"/> class.
    /// </summary>
    public abstract class NotRunableOps : BaseOperation
    {
        /// <summary>
        /// Fails when called.
        /// </summary>
        /// <param name="opData">Stack object (won't be used)</param>
        /// <param name="error">Error message (contains name of the operation that caused the failure)</param>
        /// <returns>False (always failing)</returns>
        public sealed override bool Run(IOpData opData, out string error)
        {
            error = $"Can not run an OP_{OpValue.ToString()} operation.";
            return false;
        }
    }



    // We have an IOperation for OP.Reserved,... because they can exist in a transaction but can not be run.
    // We don't have any IOperation for OP.VerIf,... because they can neither exist nor be run.


    /// <summary>
    /// Reserved operation, will fail on running.
    /// </summary>
    public class ReservedOp : NotRunableOps
    {
        public override OP OpValue => OP.Reserved;
    }

    /// <summary>
    /// Removed operation, will fail on running.
    /// </summary>
    public class VEROp : NotRunableOps
    {
        public override OP OpValue => OP.VER;
    }

    /// <summary>
    /// Reserved operation, will fail on running.
    /// </summary>
    public class Reserved1Op : NotRunableOps
    {
        public override OP OpValue => OP.Reserved1;
    }

    /// <summary>
    /// Reserved operation, will fail on running.
    /// </summary>
    public class Reserved2Op : NotRunableOps
    {
        public override OP OpValue => OP.Reserved2;
    }

}
