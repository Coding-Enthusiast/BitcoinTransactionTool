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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            PushCommand = new RelayCommand(Push, () => !string.IsNullOrWhiteSpace(DataToPush));
            CopyCommand = new RelayCommand(() => Clipboard.SetText(Result), () => !string.IsNullOrEmpty(Result));
            CopyHexCommand = new RelayCommand(() => Clipboard.SetText(ResultHex), () => !string.IsNullOrEmpty(ResultHex));
            ClearCommand = new RelayCommand(
                            () => { Result = string.Empty; ResultHex = string.Empty; RunResult = string.Empty; StackItems.Clear(); },
                            () => !string.IsNullOrEmpty(Result) || !string.IsNullOrEmpty(ResultHex));

            RunCommand = new RelayCommand(Run, () => false /*!string.IsNullOrEmpty(Result) || !string.IsNullOrEmpty(ResultHex)*/);
        }



        public ObservableCollection<ButtonModel> AvailableOps { get; set; }
        public ObservableCollection<OpGroupNames> OpGroups { get; set; }
        public ObservableCollection<string> StackItems { get; set; } = new ObservableCollection<string>();


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
            Result += $"OP_{((OP)param).ToString().Replace("_", string.Empty)} ";
            ResultHex += $"{(byte)(OP)param:x2}";
        }

        private bool IsEnabled(OP op)
        {
            return
                !(op == OP.CAT ||
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
            get { return _sel; }
            set
            {
                if (SetField(ref _sel, value))
                {
                    SetGroups(value);
                }
            }
        }


        private string _res;
        public string Result
        {
            get => _res;
            set
            {
                if (SetField(ref _res, value))
                {
                    ClearCommand.RaiseCanExecuteChanged();
                    CopyCommand.RaiseCanExecuteChanged();
                    CopyHexCommand.RaiseCanExecuteChanged();
                    RunCommand.RaiseCanExecuteChanged();
                }
            }
        }


        private string _hex;
        public string ResultHex
        {
            get => _hex;
            set
            {
                if (SetField(ref _hex, value))
                {
                    ClearCommand.RaiseCanExecuteChanged();
                    CopyCommand.RaiseCanExecuteChanged();
                    CopyHexCommand.RaiseCanExecuteChanged();
                    RunCommand.RaiseCanExecuteChanged();

                    Run();
                }
            }
        }


        private string _data;
        public string DataToPush
        {
            get { return _data; }
            set
            {
                if (SetField(ref _data, value))
                {
                    PushCommand.RaiseCanExecuteChanged();
                }
            }
        }


        private string _runres;
        public string RunResult
        {
            get => _runres;
            set => SetField(ref _runres, value);
        }


        public RelayCommand PushCommand { get; private set; }
        private void Push()
        {
            Result += $"<{DataToPush}> ";

            PushDataOp op = new PushDataOp(Base16.ToByteArray(DataToPush));
            ResultHex += op.ToByteArray().ToBase16();
        }

        public RelayCommand ClearCommand { get; private set; }
        public RelayCommand CopyCommand { get; private set; }
        public RelayCommand CopyHexCommand { get; private set; }
        public RelayCommand RunCommand { get; private set; }
        private void Run()
        {
            if (ResultHex.Contains($"{(byte)OP.IF:x2}")  && !ResultHex.Contains($"{(byte)OP.EndIf:x2}"))
            {
                RunResult = "Script contains OP_(NOT)IF but no OP_ENDIF, read/run was skipped.";
                return;
            }

            ScriptReader scr = new ScriptReader();
            int i = 0;
            byte[] data = Base16.ToByteArray(ResultHex);
            CompactInt size = new CompactInt(data.Length);
            data = size.ToByteArray().ConcatFast(data);

            if (!scr.TryDeserialize(data, ref i, out string error))
            {
                RunResult = $"An error occured while reading the script: {error}";
                return;
            }

            OpData opdt = new OpData();
            bool success = true;

            foreach (var op in scr.OperationList)
            {
                if (!op.Run(opdt, out error))
                {
                    success = false;
                    break;
                }
                StackItems = new ObservableCollection<string>(opdt.Peek(opdt.ItemCount)
                                                                  .Select(x => (x.Length == 0) ? "(Empty array)" : x.ToBase16())
                                                                  .Reverse());
                RaisePropertyChanged(nameof(StackItems));
            }

            RunResult = success ?
                $"Success{Environment.NewLine}{opdt.ItemCount} item(s) left on the stack." :
                $"Script ended with an error: {error}";
        }

        private class ScriptReader : Script
        {
            public ScriptReader()
            {
                IsWitness = false;
                OperationList = new IOperation[0];
            }
        }
    }
}
