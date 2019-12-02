// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using BitcoinTransactionTool.Backend.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitcoinTransactionTool.ViewModels
{
    public class ScrArbitraryDataViewModel : ScriptVMBase
    {
        public ScrArbitraryDataViewModel()
        {
            VmName = "Arbitrary data (OP_Return)";
            Description = "Build OP_Return PubkeyScripts. " +
                "These scripts are used to store arbitrary data in bitcoin's blockchain, " +
                "burn coins in proof of burn, for OMNI layer and more.";

            EncodingList = Enum.GetValues(typeof(Encoders)).Cast<Encoders>();
        }



        public enum Encoders
        {
            UTF8,
            Base16
        }

        public IEnumerable<Encoders> EncodingList { get; private set; }

        public Encoders SelectedEncoder { get; set; }
        public string Data { get; set; }


        public override bool SetOperations()
        {
            Errors = null;

            try
            {
                byte[] ba = new byte[0];
                if (!string.IsNullOrEmpty(Data))
                {
                    switch (SelectedEncoder)
                    {
                        case Encoders.UTF8:
                            ba = Encoding.UTF8.GetBytes(Data);
                            break;
                        case Encoders.Base16:
                            if (!Base16.IsValid(Data))
                            {
                                Errors = "Invalid base16 (hexadecimal) string.";
                                return false;
                            }
                            else
                            {
                                ba = Base16.ToByteArray(Data);
                            }
                            break;
                        default:
                            Errors = "Undefined encoder.";
                            return false;
                    }
                }

                ReturnOp op = new ReturnOp(ba, true);
                OpsToAdd = new IOperation[1] { op };
            }
            catch (Exception ex)
            {
                Errors = $"An unexpected error happened. Message: {ex.Message}";
                return false;
            }

            return true;
        }
    }
}
