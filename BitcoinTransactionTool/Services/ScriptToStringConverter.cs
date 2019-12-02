// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend;
using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using System.Text;

namespace BitcoinTransactionTool.Services
{
    public class ScriptToStringConverter
    {
        private readonly StringBuilder sb = new StringBuilder();

        private string OpValueToString(OP op)
        {
            return $"OP_{op.ToString().Replace("_", string.Empty)} ";
        }

        private void Convert(IOperation op)
        {
            if (op is PushDataOp push)
            {
                // for OP_numbers data is null for the rest ToByteArray() contains the length
                if (push.data is null)
                {
                    sb.Append(OpValueToString(op.OpValue));
                }
                else
                {
                    sb.Append($"<{((PushDataOp)op).data.ToBase16()}> ");
                }
            }
            else if (op is ReturnOp ret)
            {
                if (ret.Data.Length == 1)
                {
                    sb.Append(OpValueToString(op.OpValue));
                }
                else
                {
                    sb.Append($"{OpValueToString(op.OpValue).TrimEnd(' ')}<{ret.Data.ToBase16()}> ");
                }
            }
            else if (op is IfElseOp conditional)
            {
                sb.Append(OpValueToString(op.OpValue));
                if (conditional.mainOps is null)
                {
                    return; // This happens when the buttons are used to create an OP_IF
                }
                foreach (var main in conditional.mainOps)
                {
                    Convert(main);
                }
                if (!(conditional.elseOps is null))
                {
                    sb.Append(OpValueToString(OP.ELSE));
                    foreach (var branch in conditional.elseOps)
                    {
                        Convert(branch);
                    }
                }
                sb.Append(OpValueToString(OP.EndIf));
            }
            else
            {
                sb.Append(OpValueToString(op.OpValue));
            }
        }

        public string GetString(IEnumerable<IOperation> opList)
        {
            sb.Clear();

            foreach (var op in opList)
            {
                Convert(op);
            }

            return sb.ToString();
        }

        public string GetHex(IEnumerable<IOperation> opList)
        {
            sb.Clear();

            foreach (var op in opList)
            {
                if (op is PushDataOp push)
                {
                    sb.Append(push.ToByteArray().ToBase16());
                }
                else if (op is ReturnOp ret)
                {
                    sb.Append(ret.ToByteArray().ToBase16());
                }
                else if (op is IfElseOp conditional)
                {
                    if (conditional.mainOps is null)
                    {
                        sb.Append($"{(byte)op.OpValue:x2}");
                    }
                    else
                    {
                        sb.Append(conditional.ToByteArray().ToBase16());
                    }
                }
                else
                {
                    sb.Append($"{(byte)op.OpValue:x2}");
                }
            }

            return sb.ToString();
        }
    }
}
