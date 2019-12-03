// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using BitcoinTransactionTool.Backend.Encoders;
using BitcoinTransactionTool.Backend.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BitcoinTransactionTool.ViewModels
{
    public class ScrHashCollisionViewModel : ScriptVMBase
    {
        public ScrHashCollisionViewModel()
        {
            VmName = "Hash collision puzzles";
            Description = "Build complete signature script for claiming the hash collision puzzles. " +
                "More information could be found at https://bitcointalk.org/index.php?topic=293382.0 ";

            PuzzleList = Enum.GetValues(typeof(PuzzleTypes)).Cast<PuzzleTypes>();
        }



        public enum PuzzleTypes
        {
            SHA1,
            SHA256,
            RIPEMD160,
            RIPEMD160_SHA256,
            SHA256_double,
            Absolute
        }

        public IEnumerable<PuzzleTypes> PuzzleList { get; private set; }

        private PuzzleTypes _selPuz;
        public PuzzleTypes SelectedPuzzle
        {
            get => _selPuz;
            set => SetField(ref _selPuz, value);
        }

        private bool _isRun;
        public bool MakeRunnable
        {
            get => _isRun;
            set => SetField(ref _isRun, value);
        }

        public string MakeRunnableToolTip =>
            "If checked, the redeem script part will be pulled out of the signature script " +
            "so that it could be run here." + Environment.NewLine +
            "When unchecked, the result is a valid signature script.";


        private string _d1;
        public string Data1
        {
            get => _d1;
            set => SetField(ref _d1, value);
        }

        private string _d2;
        public string Data2
        {
            get => _d2;
            set => SetField(ref _d2, value);
        }


        public string Example1ToolTip =>
            "SHA1 hash collision example found in txid=8d31992805518fd62daa3bdd2a5c4fd2cd3054c9b3dca1d78055e9528cff6adc";
        public string Example2ToolTip => "Absolute valueS of unequal numbers i and -i are equal!";


        public RelayCommand Example1Command => new RelayCommand(Example1);
        private void Example1()
        {
            SelectedPuzzle = PuzzleTypes.SHA1;
            Data1 = "255044462d312e330a25e2e3cfd30a0a0a312030206f626a0a3c3c2f57696474682032203020522f4865696768742033203020522f547970652034203020522f537562747970652035203020522f46696c7465722036203020522f436f6c6f7253706163652037203020522f4c656e6774682038203020522f42697473506572436f6d706f6e656e7420383e3e0a73747265616d0affd8fffe00245348412d3120697320646561642121212121852fec092339759c39b1a1c63c4c97e1fffe017f46dc93a6b67e013b029aaa1db2560b45ca67d688c7f84b8c4c791fe02b3df614f86db1690901c56b45c1530afedfb76038e972722fe7ad728f0e4904e046c230570fe9d41398abe12ef5bc942be33542a4802d98b5d70f2a332ec37fac3514e74ddc0f2cc1a874cd0c78305a21566461309789606bd0bf3f98cda8044629a1";
            Data2 = "255044462d312e330a25e2e3cfd30a0a0a312030206f626a0a3c3c2f57696474682032203020522f4865696768742033203020522f547970652034203020522f537562747970652035203020522f46696c7465722036203020522f436f6c6f7253706163652037203020522f4c656e6774682038203020522f42697473506572436f6d706f6e656e7420383e3e0a73747265616d0affd8fffe00245348412d3120697320646561642121212121852fec092339759c39b1a1c63c4c97e1fffe017346dc9166b67e118f029ab621b2560ff9ca67cca8c7f85ba84c79030c2b3de218f86db3a90901d5df45c14f26fedfb3dc38e96ac22fe7bd728f0e45bce046d23c570feb141398bb552ef5a0a82be331fea48037b8b5d71f0e332edf93ac3500eb4ddc0decc1a864790c782c76215660dd309791d06bd0af3f98cda4bc4629b1";
        }

        public RelayCommand Example2Command => new RelayCommand(Example2);
        private void Example2()
        {
            SelectedPuzzle = PuzzleTypes.Absolute;
            Random rand = new Random();
            int i = rand.Next();
            Data1 = i.ToString();
            Data2 = (-i).ToString();
        }

        public override bool SetOperations()
        {
            Errors = null;

            PushDataOp push1, push2;
            IOperation op = null;
            if (SelectedPuzzle == PuzzleTypes.Absolute)
            {
                if (!int.TryParse(Data1, out int a) || !int.TryParse(Data2, out int b))
                {
                    Errors = "Data inputs for absolute puzzle must be a number.";
                    return false;
                }
                else
                {
                    push1 = new PushDataOp(a);
                    push2 = new PushDataOp(b);
                    op = new ABSOp();
                }
            }
            else if (!Base16.IsValid(Data1) || !Base16.IsValid(Data2))
            {
                Errors = "Data inputs for hash puzzles must be using base-16 encoding.";
                return false;
            }
            else
            {
                byte[] ba1 = Base16.ToByteArray(Data1);
                byte[] ba2 = Base16.ToByteArray(Data2);

                push1 = new PushDataOp(ba1);
                push2 = new PushDataOp(ba2);

                switch (SelectedPuzzle)
                {
                    case PuzzleTypes.SHA1:
                        op = new Sha1Op();
                        break;
                    case PuzzleTypes.SHA256:
                        op = new Sha256Op();
                        break;
                    case PuzzleTypes.RIPEMD160:
                        op = new RipeMd160Op();
                        break;
                    case PuzzleTypes.RIPEMD160_SHA256:
                        new Hash160Op();
                        break;
                    case PuzzleTypes.SHA256_double:
                        op = new Hash256Op();
                        break;
                    default:
                        Errors = "Puzzle type was not defined.";
                        return false;
                }
            }

            if (MakeRunnable)
            {
                OpsToAdd = new IOperation[]
                {
                    push1,
                    push2,
                    new DUP2Op(),
                    new EqualOp(),
                    new NOTOp(),
                    new VerifyOp(),
                    op,
                    new SWAPOp(),
                    op,
                    new EqualOp()
                };
            }
            else
            {
                RedeemScript rdm = new RedeemScript()
                {
                    OperationList = new IOperation[]
                    {
                        new DUP2Op(),
                        new EqualOp(),
                        new NOTOp(),
                        new VerifyOp(),
                        op,
                        new SWAPOp(),
                        op,
                        new EqualOp()
                    }
                };

                OpsToAdd = new IOperation[]
                {
                    push1,
                    push2,
                    new PushDataOp(rdm)
                };
            }


            return true;
        }
    }
}
