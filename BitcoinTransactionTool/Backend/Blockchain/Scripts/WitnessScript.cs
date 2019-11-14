// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts
{
    /// <summary>
    /// Witness is not actually a script but items inside of the script.
    /// </summary>
    public class WitnessScript : Script, IWitnessScript
    {
        public WitnessScript()
        {
            IsWitness = true;
            OperationList = new IOperation[0];
            ScriptType = ScriptType.ScriptWitness;
            maxLenOrCount = 100; // TODO: set this to a real value and from ICoin
        }



        //public void SetToP2WPKH(Signature sig, PublicKey pubKey)
        //{
        //    byte[] sigBa = sig.EncodeWithSigHashType();
        //    byte[] pubkBa = pubKey.ToByteArray(true); // only compressed pubkeys are used in witness

        //    OperationList = new IOperation[]
        //    {
        //        new PushDataOp(sigBa),
        //        new PushDataOp(pubkBa)
        //    };
        //}

        //public void SetToP2WSH_MultiSig(Signature[] sigs, RedeemScript redeem)
        //{
        //    OperationList = new IOperation[sigs.Length + 2]; // OP_0 | Sig1 | sig2 | .... | sig(n) | redeemScript

        //    OperationList[0] = new PushDataOp(OP._0);

        //    for (int i = 1; i <= sigs.Length; i++)
        //    {
        //        OperationList[i] = new PushDataOp(sigs[i].EncodeWithSigHashType());
        //    }

        //    OperationList[OperationList.Length - 1] = new PushDataOp(new PushDataOp(redeem.ToByteArray()).ToByteArray(false));
        //}


    }
}
