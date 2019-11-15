// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using BitcoinTransactionTool.Backend.Encoders;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace BitcoinTransactionTool.Backend
{
    internal class ByteArrayHexConverter : JsonConverter
    {
        public ByteArrayHexConverter(bool reverseHex)
        {
            reverse = reverseHex;
        }

        private readonly bool reverse;


        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType) => objectType == typeof(byte[]);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string hex = serializer.Deserialize<string>(reader);
                return Base16.ToByteArray(hex);
            }
            return serializer.Deserialize(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            byte[] data = (byte[])value;
            string hexString = (data == null) ? string.Empty : reverse ? data.Reverse().ToArray().ToBase16() : data.ToBase16();
            writer.WriteValue(hexString);
        }
    }



    internal class TransactionLocktimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LockTime);
        }

        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((LockTime)value).ToString());
        }
    }


    internal class TransactionScriptConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SignatureScript)
                || objectType == typeof(PubkeyScript)
                || objectType == typeof(WitnessScript)
                || objectType == typeof(RedeemScript);
        }

        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IScript scr = (IScript)value;

            writer.WriteValue(ScriptToString(scr.OperationList));
        }

        private string ScriptToString(IOperation[] ops)
        {
            StringBuilder sb = new StringBuilder(ops.Length);
            foreach (var op in ops)
            {
                if (op is PushDataOp)
                {
                    sb.Append($"PushData<{((PushDataOp)op).data.ToBase16()}> ");
                }
                else if (op is ReturnOp)
                {
                    sb.Append($"OP_{op.OpValue.ToString()}<{((ReturnOp)op).Data.ToBase16()}>");
                }
                else if (op is IfElseOp)
                {
                    string main = ScriptToString(((IfElseOp)op).mainOps);
                    sb.Append($"OP_{op.OpValue.ToString()} {main} ");
                    if (((IfElseOp)op).elseOps != null)
                    {
                        string branch = ScriptToString(((IfElseOp)op).elseOps);
                        sb.Append($"OP_{op.OpValue.ToString()} {branch} ");
                    }
                    sb.Append($"OP_{OP.EndIf.ToString()} ");
                }
                else
                {
                    sb.Append($"OP_{op.OpValue.ToString()} ");
                }
            }

            return sb.ToString().TrimEnd(' ');
        }
    }

}
