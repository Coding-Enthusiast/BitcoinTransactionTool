// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using CommonLibrary;
using System;

namespace BitcoinTransactionTool.Models
{
    public class ButtonModel
    {
        public ButtonModel(OP op, bool enabled, Action<object> s)
        {
            OpCode = op;
            string n = op.ToString();
            Name = $"OP__{op.ToString()}";
            RunCommand = new BindableCommand(s);
            Enabled = enabled;
        }

        public string Name { get; set; }
        public bool Enabled { get; set; }
        public OP OpCode { get; set; }
        public BindableCommand RunCommand { get; set; }
    }
}
