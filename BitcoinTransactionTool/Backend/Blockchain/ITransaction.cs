// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts;

namespace BitcoinTransactionTool.Backend.Blockchain
{
    public interface ITransaction : IDeserializable
    {
        TxIn[] TxInList { get; set; }
        TxOut[] TxOutList { get; set; }
        IWitnessScript[] WitnessList { get; set; }


        byte[] GetTransactionHash();
        //byte[] GetBytesToSign(ITransaction prevTx, int index, SigHashType sigHashType);
        //void WriteScriptSig(Signature sig, PublicKey pubKey, ITransaction prevTx, int index);
    }
}
