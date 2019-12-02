// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using BitcoinTransactionTool.Backend.MVVM;
using System.Collections.Generic;

namespace BitcoinTransactionTool.ViewModels
{
    public abstract class ScriptVMBase : ViewModelBase
    {
        public string VmName { get; protected set; }
        public string Description { get; protected set; }


        private IEnumerable<IOperation> _ops;
        public IEnumerable<IOperation> OpsToAdd
        {
            get => _ops;
            set => SetField(ref _ops, value);
        }

        public abstract bool SetOperations();
    }
}
