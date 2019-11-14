// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend
{
    public interface IDeserializable
    {
        /// <summary>
        /// Converts this instance into its byte array representation.
        /// </summary>
        /// <returns>An array of bytes</returns>
        byte[] Serialize();

        /// <summary>
        /// Deserializes the given byte array starting from the specified offset. The return value indicates success.
        /// </summary>
        /// <param name="data">Byte array to use.</param>
        /// <param name="offset">The offset inside the <paramref name="data"/> to start from</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if deserialization was successful, false if otherwise</returns>
        bool TryDeserialize(byte[] data, ref int offset, out string error);
    }
}
