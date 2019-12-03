// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend;
using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using BitcoinTransactionTool.Backend.Encoders;
using BitcoinTransactionTool.Backend.MVVM;
using BitcoinTransactionTool.Models;
using BitcoinTransactionTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace BitcoinTransactionTool.ViewModels
{
    public class ScriptViewModel : ViewModelBase
    {
        public ScriptViewModel()
        {
            OpGroups = new ObservableCollection<OpGroupNames>(Enum.GetValues(typeof(OpGroupNames)).Cast<OpGroupNames>());
            SetGroups(OpGroupNames.All);

            ScriptTemplateList = new ScriptVMBase[]
            {
                new ScrAddressViewModel(),
                new ScrArbitraryDataViewModel(),
                new ScrMultiSigViewModel(),
                new ScrHashCollisionViewModel()
            };
            SelectedCustomScript = ScriptTemplateList.First();

            OperationList = new ObservableCollection<IOperation>();
            OperationList.CollectionChanged += OperationList_CollectionChanged;

            SetOperationsCommand = new RelayCommand(SetOperations);
            PushDataCommand = new RelayCommand(Push, () => !string.IsNullOrWhiteSpace(DataToPush));
            CopyCommand = new RelayCommand(() => Clipboard.SetText(ScriptString), () => !string.IsNullOrEmpty(ScriptString));
            CopyHexCommand = new RelayCommand(() => Clipboard.SetText(ScriptHex), () => !string.IsNullOrEmpty(ScriptHex));
            ClearCommand = new RelayCommand(Clear, () => !string.IsNullOrEmpty(ScriptString) || !string.IsNullOrEmpty(ScriptHex));

            RunCommand = new RelayCommand(Run, () => OperationList.Count != 0);
        }



        public OperationConverter OpConverter { get; set; } = new OperationConverter();
        public ScriptToStringConverter ScriptConverter { get; set; } = new ScriptToStringConverter();
        public ScriptReader ScrReader { get; set; } = new ScriptReader();

        public ObservableCollection<IOperation> OperationList { get; private set; }
        private void OperationList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                IEnumerable<IOperation> newItems = e.NewItems.Cast<IOperation>();

                ScriptString += ScriptConverter.GetString(newItems);
                ScriptHex += ScriptConverter.GetHex(newItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ScriptString = string.Empty;
                ScriptHex = string.Empty;
            }

            // Raise for all actions: add, remove, change,...
            RunCommand.RaiseCanExecuteChanged();
        }

        public IEnumerable<ScriptVMBase> ScriptTemplateList { get; set; }
        public ObservableCollection<ButtonModel> AvailableOps { get; set; }

        private ObservableCollection<string> _stackItems = new ObservableCollection<string>();
        public ObservableCollection<string> StackItems
        {
            get => _stackItems;
            set => SetField(ref _stackItems, value);
        }


        public ObservableCollection<OpGroupNames> OpGroups { get; set; }

        public enum OpGroupNames
        {
            All,
            Numbers,
            FlowControl,
            Stack,
            Splice,
            BitwiseLogic,
            Arithmetic,
            Cryptography,
            NewOps
        }

        private void SetGroups(OpGroupNames n)
        {
            IEnumerable<OP> allNames =
                Enum.GetValues(typeof(OP))
                    .Cast<OP>()
                    .Where(x => x != OP.PushData1 && x != OP.PushData2 && x != OP.PushData4);

            OP min = (OP)0;
            OP max = (OP)255;

            switch (n)
            {
                case OpGroupNames.All:
                    break;
                case OpGroupNames.Numbers:
                    min = OP._0;
                    max = OP._16;
                    allNames = allNames.Where(x => x != OP.Reserved);
                    break;
                case OpGroupNames.FlowControl:
                    min = OP.NOP; max = OP.RETURN;
                    break;
                case OpGroupNames.Stack:
                    min = OP.ToAltStack; max = OP.TUCK;
                    break;
                case OpGroupNames.Splice:
                    min = OP.CAT; max = OP.SIZE;
                    break;
                case OpGroupNames.BitwiseLogic:
                    min = OP.INVERT; max = OP.Reserved2;
                    break;
                case OpGroupNames.Arithmetic:
                    min = OP.ADD1; max = OP.WITHIN;
                    break;
                case OpGroupNames.Cryptography:
                    min = OP.RIPEMD160; max = OP.CheckMultiSigVerify;
                    break;
                case OpGroupNames.NewOps:
                    min = OP.NOP1;
                    break;
            }

            allNames = allNames.Where(x => x >= min && x <= max);
            AvailableOps = new ObservableCollection<ButtonModel>(allNames.Select(x => new ButtonModel(x, IsEnabled(x), Add)));
            RaisePropertyChanged(nameof(AvailableOps));
        }

        private void Add(object param)
        {
            OperationList.Add(OpConverter.ConvertToOperation((OP)param));
        }

        private bool IsEnabled(OP op)
        {
            return
                !(op == OP.VerIf ||
                op == OP.VerNotIf ||
                op == OP.CAT ||
                op == OP.SubStr ||
                op == OP.LEFT ||
                op == OP.RIGHT ||
                op == OP.INVERT ||
                op == OP.AND ||
                op == OP.OR ||
                op == OP.XOR ||
                op == OP.MUL2 ||
                op == OP.DIV2 ||
                op == OP.MUL ||
                op == OP.DIV ||
                op == OP.MOD ||
                op == OP.LSHIFT ||
                op == OP.RSHIFT

                || op == OP.CodeSeparator // not implemented
                || op == OP.CheckLocktimeVerify
                || op == OP.CheckSequenceVerify
                );
        }


        private OpGroupNames _sel;
        public OpGroupNames SelectedOpGroup
        {
            get => _sel;
            set
            {
                if (SetField(ref _sel, value))
                {
                    SetGroups(value);
                }
            }
        }


        private ScriptVMBase _vm;
        public ScriptVMBase SelectedCustomScript
        {
            get => _vm;
            set => SetField(ref _vm, value);
        }


        private string _res;
        public string ScriptString
        {
            get => _res;
            set
            {
                if (SetField(ref _res, value))
                {
                    ClearCommand.RaiseCanExecuteChanged();
                    CopyCommand.RaiseCanExecuteChanged();
                }
            }
        }


        private string _hex;
        public string ScriptHex
        {
            get => _hex;
            set
            {
                if (SetField(ref _hex, value))
                {
                    ClearCommand.RaiseCanExecuteChanged();
                    CopyHexCommand.RaiseCanExecuteChanged();
                }
            }
        }


        private string _data;
        public string DataToPush
        {
            get => _data;
            set
            {
                if (SetField(ref _data, value))
                {
                    PushDataCommand.RaiseCanExecuteChanged();
                }
            }
        }


        private string _runres;
        public string RunResult
        {
            get => _runres;
            set => SetField(ref _runres, value);
        }


        public RelayCommand SetOperationsCommand { get; private set; }
        private void SetOperations()
        {
            if (SelectedCustomScript.SetOperations() && SelectedCustomScript.OpsToAdd.Count() != 0)
            {
                SelectedCustomScript.OpsToAdd.ToList().ForEach(x => OperationList.Add(x));
            }
        }

        public RelayCommand PushDataCommand { get; private set; }
        private void Push()
        {
            PushDataOp op = new PushDataOp(Base16.ToByteArray(DataToPush));
            OperationList.Add(op);
        }

        public RelayCommand ClearCommand { get; private set; }
        private void Clear()
        {
            RunResult = string.Empty;
            StackItems.Clear();
            OperationList.Clear();
        }

        public RelayCommand CopyCommand { get; private set; }

        public RelayCommand CopyHexCommand { get; private set; }

        public RelayCommand RunCommand { get; private set; }
        private void Run()
        {
            // if the IfOp is set using the buttons, the mainOps list of it will be null so we have to read the script hex
            IEnumerable<IfElseOp> listOfIfOps = OperationList.Where(x => x is IfElseOp).Cast<IfElseOp>();
            if (listOfIfOps.Count() != 0 && listOfIfOps.Any(x => x.mainOps is null))
            {
                var resp = ScrReader.Read(ScriptHex);
                if (resp.Errors.Any())
                {
                    RunResult = $"Error while reading the script from hex: {resp.Errors.GetErrors()}";
                    return;
                }
                else
                {
                    // don't assign list to a new instance, it will break the event that we are listening to above
                    OperationList.Clear();
                    resp.Result.ToList().ForEach(x => OperationList.Add(x));
                }
            }

            OpData opdt = new OpData();
            bool success = true;
            string error = null;
            foreach (var op in OperationList)
            {
                if (!op.Run(opdt, out error))
                {
                    success = false;
                    break;
                }
                StackItems = new ObservableCollection<string>(
                    opdt.Peek(opdt.ItemCount)
                        .Select(x =>
                                    (x.Length == 0) ? "(Empty array = False)" :
                                    (x.Length == 1 && x[0] == 1) ? "01 (True)" :
                                    x.ToBase16())
                        .Reverse());
            }

            RunResult = success ?
                $"Success!{Environment.NewLine}{opdt.ItemCount} item(s) left on the stack." :
                $"Script ended with an error: {error}";
        }

    }
}
