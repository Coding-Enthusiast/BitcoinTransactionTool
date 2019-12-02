// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using System;

namespace BitcoinTransactionTool.Services
{
    public class OperationConverter
    {
        // A Fake reader used only for the coversion of OP to IOperation
        private class ScriptReader : Script
        {
            public ScriptReader()
            {
                IsWitness = false;
            }
        }

        // two Fake ops only used for returning an IOperation, not to be used for anything else.
        private class ElseOp : IOperation
        {
            public OP OpValue => OP.ELSE;
            public bool Run(IOpData opData, out string error) => throw new NotImplementedException();
        }
        private class EndIfOp : IOperation
        {
            public OP OpValue => OP.EndIf;
            public bool Run(IOpData opData, out string error) => throw new NotImplementedException();
        }

        private readonly ScriptReader reader = new ScriptReader();

        public IOperation ConvertToOperation(OP op)
        {
            switch (op)
            {
                case OP.IF:
                    return new IFOp();
                case OP.NotIf:
                    return new NotIfOp();
                case OP.ELSE:
                    return new ElseOp();
                case OP.EndIf:
                    return new EndIfOp();
            }

            byte[] data = new byte[2] { 1, (byte)op };
            int offset = 0;
            if (reader.TryDeserialize(data, ref offset, out string error))
            {
                return reader.OperationList[0];
            }
            else
            {
                throw new ArgumentException($"An error occured while trying to convert: {error}");
            }
        }
    }
}
