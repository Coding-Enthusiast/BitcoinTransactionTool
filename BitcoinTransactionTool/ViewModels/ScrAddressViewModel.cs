// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend;
using BitcoinTransactionTool.Backend.Blockchain.Scripts;

namespace BitcoinTransactionTool.ViewModels
{
    public class ScrAddressViewModel : ScriptVMBase
    {
        public ScrAddressViewModel()
        {
            VmName = "Addresses";
            Description = "Build PubkeyScript based on different addresses.";
        }



        private readonly Address addrManager = new Address();
        public string Address { get; set; }



        public override bool SetOperations()
        {
            Errors = null;

            if (string.IsNullOrEmpty(Address))
            {
                Errors = "Address can not be empty.";
                return false;
            }
            else if (addrManager.TryGetType(Address, out PubkeyScriptType scrType, out byte[] hash))
            {
                PubkeyScript scr = new PubkeyScript();
                switch (scrType)
                {
                    case PubkeyScriptType.P2PKH:
                        scr.SetToP2PKH(hash);
                        break;
                    case PubkeyScriptType.P2SH:
                        scr.SetToP2SH(hash);
                        break;
                    case PubkeyScriptType.P2WPKH:
                        scr.SetToP2WPKH(hash);
                        break;
                    case PubkeyScriptType.P2WSH:
                        scr.SetToP2WSH(hash);
                        break;
                    default:
                        Errors = "Undefined script type."; // This should never happen.
                        return false;
                }

                OpsToAdd = scr.OperationList;
                return true;
            }
            else
            {
                Errors = "Invalid address type/format.";
                return false;
            }
        }

    }
}
