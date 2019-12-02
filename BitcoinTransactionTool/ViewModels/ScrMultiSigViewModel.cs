// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using BitcoinTransactionTool.Backend.Encoders;
using BitcoinTransactionTool.Models;
using System;
using System.ComponentModel;
using System.Linq;

namespace BitcoinTransactionTool.ViewModels
{
    public class ScrMultiSigViewModel : ScriptVMBase
    {
        public ScrMultiSigViewModel()
        {
            VmName = "Multi-sig redeem script";
            Description = $"Build multi signature redeem script using given public keys.{Environment.NewLine}" +
                $"Validity of public keys are not currently being checked.";

            PubkeyList = new BindingList<PublicKeyModel>();
            PubkeyList.ListChanged += PubkeyList_ListChanged;
        }

        private void PubkeyList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemDeleted)
            {
                N = PubkeyList.Count();
            }
        }

        public BindingList<PublicKeyModel> PubkeyList { get; set; }


        private int _m;
        public int M
        {
            get => _m;
            set
            {
                if (_m != value)
                {
                    if (value > N)
                    {
                        _m = N;
                        RaisePropertyChanged(nameof(M));
                    }
                    else
                    {
                        SetField(ref _m, value);
                    }
                }
            }
        }

        private int _n;
        public int N
        {
            get => _n;
            set => SetField(ref _n, value);
        }


        public override bool SetOperations()
        {
            Errors = null;

            if (PubkeyList.Count() == 0)
            {
                Errors = "At least one public key is needed.";
                return false;
            }
            else if (PubkeyList.Any(x => x.HasErrors))
            {
                Errors = "Invalid public key was found.";
                return false;
            }
            else
            {
                // OP_m | pub1 | pub2 | ... | pub(n) | OP_n | OP_CheckMultiSig
                IOperation[] ops = new IOperation[N + 3];

                ops[0] = new PushDataOp(M);
                ops[N + 1] = new PushDataOp(N);
                ops[N + 2] = new CheckMultiSigOp();

                int i = 1;
                foreach (var item in PubkeyList)
                {
                    ops[i++] = new PushDataOp(Base16.ToByteArray(item.PubKey));
                }

                OpsToAdd = ops;

                return true;
            }
        }
    }
}
