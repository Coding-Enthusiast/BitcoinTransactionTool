// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using BitcoinTransactionTool.Backend.MVVM;
using System;
using System.ComponentModel;

namespace BitcoinTransactionTool.Models
{
    public class ButtonModel
    {
        public ButtonModel(OP op, bool enabled, Action<object> methodToRun)
        {
            Name = $"OP__{op.ToString()}";
            OpCode = op;

            DescriptionAttribute[] desc = GetDescriptions(op);
            Description = (desc == null || desc.Length == 0) ? Name : desc[0].Description;

            RunCommand = new BindableCommand(methodToRun);
            Enabled = enabled;
        }

        private DescriptionAttribute[] GetDescriptions(OP op)
        {
            return op.GetType()
                     .GetField(op.ToString())
                     .GetCustomAttributes(typeof(DescriptionAttribute), false)
                     as DescriptionAttribute[];
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public OP OpCode { get; set; }
        public BindableCommand RunCommand { get; set; }
    }
}
