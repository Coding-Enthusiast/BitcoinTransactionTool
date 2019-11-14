// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Base abstract class for all operations. Implements overrides for Equals() and GetHashCode() functions.
    /// </summary>
    public abstract class BaseOperation : IOperation
    {
        /// <summary>
        /// A single byte inticating type of the opeartion
        /// </summary>
        public abstract OP OpValue { get; }

        /// <summary>
        /// When overriden, performs the specifc operation on the given stack object <see cref="IOpData"/>.
        /// </summary>
        /// <param name="opData">Stack to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public abstract bool Run(IOpData opData, out string error);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object, flase if otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is IOperation)
            {
                return ((IOperation)obj).OpValue == OpValue;
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code</returns>
        public override int GetHashCode()
        {
            return OpValue.GetHashCode();
        }
    }
}
