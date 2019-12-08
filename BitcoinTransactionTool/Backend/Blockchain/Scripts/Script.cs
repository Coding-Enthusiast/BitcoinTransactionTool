// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using System;
using System.Collections.Generic;

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts
{
    /// <summary>
    /// Base (abstract) class for scripts
    /// </summary>
    public abstract class Script : IScript, IDeserializable
    {
        /* A short reminder of how all this works:
         * We split scripts into two categories, the "scripts" and "witnesses".
         * All of them have the same structure of a CompactInt followed by a list of operations.
         * For "scripts" (SignatureScript, PubkeyScript, RedeemScript) the CompactInt value indicates length of the following data.
         * For "witnesses" the CompactInt is the number of (count) operations to follow.
         * 
         * There is also another difference in SegWit when it comes to pushing data:
         * In "scripts" PushDataOp uses StackInt to indicate size of the data pushed on top of the stack.
         * In "witnesses" PushDataOp uses CompactInt to indicate size of the data pushed on top of the stack.
         * 
         * */


        /// <summary>
        /// [Default value = false] Returns whether the script instance is of witness type. It will affect (de)serialization methods.
        /// </summary>
        public bool IsWitness { get; protected set; } = false;
        /// <summary>
        /// Type of this script instance
        /// </summary>
        public virtual ScriptType ScriptType { get; set; }
        /// <summary>
        /// List of operations that the script contains.
        /// </summary>
        public IOperation[] OperationList { get; set; }

        private int len, endIndex; // We have to set this for OP_Return cases that need them
        protected int maxLenOrCount;



        /// <summary>
        /// Converts this instance into its byte array representation.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <returns>An array of bytes.</returns>
        public virtual byte[] Serialize()
        {
            byte[] result = ToByteArray();

            CompactInt lengthOrCount = new CompactInt(IsWitness ? OperationList.Length : result.Length);

            return lengthOrCount.ToByteArray().ConcatFast(result);
        }

        public byte[] ToByteArray()
        {
            if (OperationList == null)
                throw new ArgumentNullException(nameof(OperationList), "Lis of operations can not be null.");


            // TODO: we may need more checks on IsWitness
            byte[] result = { };
            foreach (var op in OperationList)
            {
                if (op is PushDataOp)
                {
                    result = result.ConcatFast(((PushDataOp)op).ToByteArray(IsWitness));
                }
                else if (op is IfElseOp)
                {
                    result = result.ConcatFast(((IfElseOp)op).ToByteArray());
                }
                else if (op is ReturnOp)
                {
                    result = result.ConcatFast(((ReturnOp)op).ToByteArray());
                }
                else
                {
                    result = result.AppendToEnd((byte)op.OpValue);
                }
            }

            return result;
        }


        /// <summary>
        /// Deserializes the given byte array starting from the specified offset. The return value indicates success.
        /// </summary>
        /// <param name="data">Byte array containing a <see cref="Script"/>.</param>
        /// <param name="offset">The offset inside the <paramref name="data"/> to start from.</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <returns>True if deserialization was successful, false if otherwise.</returns>
        public virtual bool TryDeserialize(byte[] data, ref int offset, out string error)
        {
            if (offset < 0)
            {
                error = "Offset can not be negative.";
                return false;
            }
            if (data == null || data.Length - offset < 0)
            {
                error = "Data length is not valid.";
                return false;
            }


            if (!CompactInt.TryReadFromBytes(data, ref offset, out CompactInt lengthOrCount, out error))
            {
                return false;
            }
            if (lengthOrCount.Value > int.MaxValue)
            {
                error = (IsWitness ? "Item count" : "Script length") + "is too big.";
                return false;
            }

            if (IsWitness)
            {
                // cast is ok since the check happend in previous line.
                int count = (int)lengthOrCount;
                if (count > maxLenOrCount)
                {
                    error = "Invalid witness item count.";
                    return false;
                }


                OperationList = new IOperation[count];
                for (int i = 0; i < count; i++)
                {
                    // TODO: the assumption here is that witness doesn't have anything but PushDataOp, this may be wrong.
                    PushDataOp op = new PushDataOp();
                    if (!op.TryRead(data, ref offset, out error, true))
                    {
                        return false;
                    }
                    OperationList[i] = op;
                }
            }
            else
            {
                // cast is ok same as above
                len = (int)lengthOrCount;

                // Empty script (offset doesn't change, it already changed when reading CompactInt)
                if (len == 0)
                {
                    OperationList = new IOperation[0];
                    error = null;
                    return true;
                }

                List<IOperation> opList = new List<IOperation>();
                endIndex = offset + len;
                while (offset < endIndex)
                {
                    if (!TryRead(data, opList, ref offset, out error))
                    {
                        return false;
                    }
                }

                if (offset != endIndex)
                {
                    error = "Invalid stack format.";
                    return false;
                }
                OperationList = opList.ToArray();
            }

            error = null;
            return true;
        }


        private bool IsPushOp(byte b)
        {
            return b >= 0 && b <= (byte)OP._16 && b != (byte)OP.Reserved;
        }
        private bool TryRead(byte[] data, List<IOperation> opList, ref int offset, out string error)
        {
            if (IsPushOp(data[offset]))
            {
                PushDataOp op = new PushDataOp();
                if (!op.TryRead(data, ref offset, out error, IsWitness))
                {
                    return false;
                }
                opList.Add(op);
            }
            else if (data[offset] == (byte)OP.RETURN)
            {
                ReturnOp op = new ReturnOp();
                if (!op.TryRead(data, ref offset, out error, len))
                {
                    return false;
                }
                opList.Add(op);
            }
            else
            {
                switch ((OP)data[offset])
                {
                    // Invalid OPs:
                    case OP.VerIf:
                    case OP.VerNotIf:
                        error = $"Invalid OP was found: {GetOpName((OP)data[offset])}";
                        return false;

                    // Disabled OPs:
                    case OP.CAT:
                    case OP.SubStr:
                    case OP.LEFT:
                    case OP.RIGHT:
                    case OP.INVERT:
                    case OP.AND:
                    case OP.OR:
                    case OP.XOR:
                    case OP.MUL2:
                    case OP.DIV2:
                    case OP.MUL:
                    case OP.DIV:
                    case OP.MOD:
                    case OP.LSHIFT:
                    case OP.RSHIFT:
                        error = $"Disabled OP was found: {GetOpName((OP)data[offset])}";
                        return false;

                    // Special case of IFs:
                    case OP.IF:
                        IFOp ifop = new IFOp();
                        List<IOperation> ifOps = new List<IOperation>();
                        offset++;
                        // (offset < endIndex) needs to be first to prevent OutOfRange exception
                        while (offset < endIndex && data[offset] != (byte)OP.EndIf && data[offset] != (byte)OP.ELSE)
                        {
                            if (!TryRead(data, ifOps, ref offset, out error))
                            {
                                return false;
                            }
                            if (offset >= endIndex)// there must be at least 1 more item remaining that is equal to EndIf
                            {
                                error = "Bad format.";
                                return false;
                            }
                            if (ifOps.Count == 0)
                            {
                                error = "Empty OP_IF";
                                return false;
                            }
                        }
                        // (offset < endIndex) needs to be checked again to prevent another OutOfRange exception
                        if (offset < endIndex && data[offset] == (byte)OP.ELSE)
                        {
                            List<IOperation> elseOps = new List<IOperation>();
                            offset++;
                            while (offset < endIndex && data[offset] != (byte)OP.EndIf)
                            {
                                if (!TryRead(data, elseOps, ref offset, out error))
                                {
                                    return false;
                                }
                            }
                            if (offset >= endIndex)
                            {
                                error = "Bad format.";
                                return false;
                            }
                            if (elseOps.Count == 0)
                            {
                                error = "Empty OP_ELSE";
                                return false;
                            }
                            ifop.elseOps = elseOps.ToArray();
                        }
                        // (offset < endIndex) needs to be checked again to prevent another OutOfRange exception
                        if (offset >= endIndex || data[offset] != (byte)OP.EndIf)
                        {
                            error = "No OP_ENDIF was found.";//this may never happen!
                            return false;
                        }

                        ifop.mainOps = ifOps.ToArray();

                        opList.Add(ifop);
                        break;

                    case OP.NotIf:
                        NotIfOp notifOp = new NotIfOp();
                        List<IOperation> ifOps2 = new List<IOperation>();
                        offset++;
                        // (offset < endIndex) needs to be first to prevent OutOfRange exception
                        while (offset < endIndex && data[offset] != (byte)OP.EndIf && data[offset] != (byte)OP.ELSE)
                        {
                            if (!TryRead(data, ifOps2, ref offset, out error))
                            {
                                return false;
                            }
                            if (offset >= endIndex)// there must be at least 1 more item remaining that is equal to EndIf
                            {
                                error = "Bad format.";
                                return false;
                            }
                            if (ifOps2.Count == 0)
                            {
                                error = "Empty OP_NotIf";
                                return false;
                            }
                        }
                        // (offset < endIndex) needs to be first to prevent OutOfRange exception
                        if (data[offset] == (byte)OP.ELSE)
                        {
                            List<IOperation> elseOps2 = new List<IOperation>();
                            offset++;
                            while (offset < endIndex && data[offset] != (byte)OP.EndIf)
                            {
                                if (!TryRead(data, elseOps2, ref offset, out error))
                                {
                                    return false;
                                }
                            }
                            if (offset >= endIndex)
                            {
                                error = "Bad format.";
                                return false;
                            }
                            if (elseOps2.Count == 0)
                            {
                                error = "Empty OP_ELSE";
                                return false;
                            }
                            notifOp.elseOps = elseOps2.ToArray();
                        }
                        // (offset < endIndex) needs to be checked again to prevent another OutOfRange exception
                        if (offset >= endIndex || data[offset] != (byte)OP.EndIf)
                        {
                            error = "No OP_ENDIF was found.";//this may never happen!
                            return false;
                        }

                        notifOp.mainOps = ifOps2.ToArray();

                        opList.Add(notifOp);
                        break;

                    case OP.ELSE:
                        error = "OP_ELSE found without prior OP_IF or OP_NOTIF.";
                        return false;
                    case OP.EndIf:
                        error = "OP_EndIf found without prior OP_IF or OP_NOTIF.";
                        return false;

                    // From OP_0 to OP_16 except OP_Reserved is already handled.

                    case OP.Reserved:
                        opList.Add(new ReservedOp());
                        break;

                    case OP.NOP:
                        opList.Add(new NOPOp());
                        break;
                    case OP.VER:
                        opList.Add(new VEROp());
                        break;

                    // OP.IF and OP.NotIf moved to top
                    // OP.VerIf and OP.VerNotIf moved to top (Invalid tx)
                    // OP.ELSE and OP.EndIf moved to top

                    case OP.VERIFY:
                        opList.Add(new VerifyOp());
                        break;

                    // OP.RETURN is already handled

                    case OP.ToAltStack:
                        opList.Add(new ToAltStackOp());
                        break;
                    case OP.FromAltStack:
                        opList.Add(new FromAltStackOp());
                        break;
                    case OP.DROP2:
                        opList.Add(new DROP2Op());
                        break;
                    case OP.DUP2:
                        opList.Add(new DUP2Op());
                        break;
                    case OP.DUP3:
                        opList.Add(new DUP3Op());
                        break;
                    case OP.OVER2:
                        opList.Add(new OVER2Op());
                        break;
                    case OP.ROT2:
                        opList.Add(new ROT2Op());
                        break;
                    case OP.SWAP2:
                        opList.Add(new SWAP2Op());
                        break;
                    case OP.IfDup:
                        opList.Add(new IfDupOp());
                        break;
                    case OP.DEPTH:
                        opList.Add(new DEPTHOp());
                        break;
                    case OP.DROP:
                        opList.Add(new DROPOp());
                        break;
                    case OP.DUP:
                        opList.Add(new DUPOp());
                        break;
                    case OP.NIP:
                        opList.Add(new NIPOp());
                        break;
                    case OP.OVER:
                        opList.Add(new OVEROp());
                        break;
                    case OP.PICK:
                        opList.Add(new PICKOp());
                        break;
                    case OP.ROLL:
                        opList.Add(new ROLLOp());
                        break;
                    case OP.ROT:
                        opList.Add(new ROTOp());
                        break;
                    case OP.SWAP:
                        opList.Add(new SWAPOp());
                        break;
                    case OP.TUCK:
                        opList.Add(new TUCKOp());
                        break;

                    // OP_ (CAT SubStr LEFT RIGHT INVERT AND OR XOR) are moved to top

                    case OP.SIZE:
                        opList.Add(new SizeOp());
                        break;

                    case OP.EQUAL:
                        opList.Add(new EqualOp());
                        break;
                    case OP.EqualVerify:
                        opList.Add(new EqualVerifyOp());
                        break;
                    case OP.Reserved1:
                        opList.Add(new Reserved1Op());
                        break;
                    case OP.Reserved2:
                        opList.Add(new Reserved2Op());
                        break;
                    case OP.ADD1:
                        opList.Add(new ADD1Op());
                        break;
                    case OP.SUB1:
                        opList.Add(new SUB1Op());
                        break;

                    // OP.MUL2 and OP.DIV2 are moved to top (disabled op).

                    case OP.NEGATE:
                        opList.Add(new NEGATEOp());
                        break;
                    case OP.ABS:
                        opList.Add(new ABSOp());
                        break;
                    case OP.NOT:
                        opList.Add(new NOTOp());
                        break;
                    case OP.NotEqual0:
                        opList.Add(new NotEqual0Op());
                        break;
                    case OP.ADD:
                        opList.Add(new AddOp());
                        break;
                    case OP.SUB:
                        opList.Add(new SUBOp());
                        break;

                    // OP_ (MUL DIV MOD LSHIFT RSHIFT) are moved to top (disabled op).

                    case OP.BoolAnd:
                        opList.Add(new BoolAndOp());
                        break;
                    case OP.BoolOr:
                        opList.Add(new BoolOrOp());
                        break;
                    case OP.NumEqual:
                        opList.Add(new NumEqualOp());
                        break;
                    case OP.NumEqualVerify:
                        opList.Add(new NumEqualVerifyOp());
                        break;
                    case OP.NumNotEqual:
                        opList.Add(new NumNotEqualOp());
                        break;
                    case OP.LessThan:
                        opList.Add(new LessThanOp());
                        break;
                    case OP.GreaterThan:
                        opList.Add(new GreaterThanOp());
                        break;
                    case OP.LessThanOrEqual:
                        opList.Add(new LessThanOrEqualOp());
                        break;
                    case OP.GreaterThanOrEqual:
                        opList.Add(new GreaterThanOrEqualOp());
                        break;
                    case OP.MIN:
                        opList.Add(new MINOp());
                        break;
                    case OP.MAX:
                        opList.Add(new MAXOp());
                        break;
                    case OP.WITHIN:
                        opList.Add(new WITHINOp());
                        break;

                    case OP.RIPEMD160:
                        opList.Add(new RipeMd160Op());
                        break;
                    case OP.SHA1:
                        opList.Add(new Sha1Op());
                        break;
                    case OP.SHA256:
                        opList.Add(new Sha256Op());
                        break;
                    case OP.HASH160:
                        opList.Add(new Hash160Op());
                        break;
                    case OP.HASH256:
                        opList.Add(new Hash256Op());
                        break;
                    //case OP.CodeSeparator:
                    //    break;
                    case OP.CheckSig:
                        opList.Add(new CheckSigOp());
                        break;
                    case OP.CheckSigVerify:
                        opList.Add(new CheckSigVerifyOp());
                        break;
                    case OP.CheckMultiSig:
                        opList.Add(new CheckMultiSigOp());
                        break;
                    case OP.CheckMultiSigVerify:
                        opList.Add(new CheckMultiSigVerifyOp());
                        break;
                    case OP.NOP1:
                        opList.Add(new NOP1Op());
                        break;
                    case OP.CheckLocktimeVerify:
                        opList.Add(new CheckLocktimeVerifyOp());
                        break;
                    case OP.CheckSequenceVerify:
                        opList.Add(new CheckSequenceVerifyOp());
                        break;
                    case OP.NOP4:
                        opList.Add(new NOP4Op());
                        break;
                    case OP.NOP5:
                        opList.Add(new NOP5Op());
                        break;
                    case OP.NOP6:
                        opList.Add(new NOP6Op());
                        break;
                    case OP.NOP7:
                        opList.Add(new NOP7Op());
                        break;
                    case OP.NOP8:
                        opList.Add(new NOP8Op());
                        break;
                    case OP.NOP9:
                        opList.Add(new NOP9Op());
                        break;
                    case OP.NOP10:
                        opList.Add(new NOP10Op());
                        break;

                    default:
                        error = "Undefined OP code";
                        return false;
                }

                offset++;
            }

            error = null;
            return true;
        }


        private string GetOpName(OP val)
        {
            return $"OP_{val.ToString()}";
        }

    }
}
