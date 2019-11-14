// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operation that transfers one item from main stack to the alt stack.
    /// </summary>
    public class ToAltStackOp : BaseOperation
    {
        public override OP OpValue => OP.ToAltStack;

        /// <summary>
        /// Removes top stack item and puts it in alt-stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = "There was not enough items left in the stack to copy to alt-stack.";
                return false;
            }

            opData.AltPush(opData.Pop());

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation that transfers one item from alt stack to the main stack.
    /// </summary>
    public class FromAltStackOp : BaseOperation
    {
        public override OP OpValue => OP.FromAltStack;

        /// <summary>
        /// Removes top alt-stack item and puts it in stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.AltItemCount < 1)
            {
                error = "There was not enough items left in the alt-stack to copy to stack.";
                return false;
            }

            opData.Push(opData.AltPop());

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation that removes the top 2 stack items.
    /// </summary>
    public class DROP2Op : BaseOperation
    {
        public override OP OpValue => OP.DROP2;

        /// <summary>
        /// Removes (discards) top two stack items. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2)
            {
                error = "There was not enough items left in the stack to drop.";
                return false;
            }

            opData.Pop(2); // Throw away the 2 items

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation that duplicates the top 2 stack items.
    /// </summary>
    public class DUP2Op : BaseOperation
    {
        public override OP OpValue => OP.DUP2;

        /// <summary>
        /// Duplicates top two stack items. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2)
            {
                error = "There was not enough items in the stack to duplicate.";
                return false;
            }

            opData.Push(opData.Peek(2));

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation that duplicates the top 3 stack items.
    /// </summary>
    public class DUP3Op : BaseOperation
    {
        public override OP OpValue => OP.DUP3;

        /// <summary>
        /// Duplicates top three stack items. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 3)
            {
                error = "There was not enough items in the stack to duplicate.";
                return false;
            }

            opData.Push(opData.Peek(3));

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation copies two items to the top (x1 x2 x3 x4 -> x1 x2 x3 x4 x1 x2).
    /// </summary>
    public class OVER2Op : BaseOperation
    {
        public override OP OpValue => OP.OVER2;

        /// <summary>
        /// Copies 2 items from stack to top of the stack like this: x1 x2 x3 x4 -> x1 x2 x3 x4 x1 x2
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 4)
            {
                error = "There was not enough items left in the stack to copy over.";
                return false;
            }

            byte[] data1 = opData.PeekAtIndex(3);
            byte[] data2 = opData.PeekAtIndex(2);

            opData.Push(new byte[2][] { data1, data2 });

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation moves two items to the top (x1 x2 x3 x4 x5 x6 -> x3 x4 x5 x6 x1 x2).
    /// </summary>
    public class ROT2Op : BaseOperation
    {
        public override OP OpValue => OP.ROT2;

        /// <summary>
        /// Moves 2 items from stack to top of the stack like this: x1 x2 x3 x4 x5 x6 -> x3 x4 x5 x6 x1 x2
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 6)
            {
                error = "There was not enough items left in the stack to rotate.";
                return false;
            }

            byte[][] data = opData.Pop(6);
            // (x0 x1 x2 x3 x4 x5 -> x2 x3 x4 x5 x0 x1)
            opData.Push(new byte[6][] { data[2], data[3], data[4], data[5], data[0], data[1] });

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation that swaps top two pairs of items (x1 x2 x3 x4 -> x3 x4 x1 x2).
    /// </summary>
    public class SWAP2Op : BaseOperation
    {
        public override OP OpValue => OP.SWAP2;

        /// <summary>
        /// Swaps top two item pairs on top of the stack: x1 x2 x3 x4 -> x3 x4 x1 x2
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 4)
            {
                error = "There was not enough items left in the stack to swap.";
                return false;
            }

            byte[][] data = opData.Pop(4);
            // x0 x1 x2 x3 -> x2 x3 x0 x1
            opData.Push(new byte[4][] { data[2], data[3], data[0], data[1] });

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation that duplicates top stack item if its value is not zero.
    /// </summary>
    public class IfDupOp : BaseOperation
    {
        public override OP OpValue => OP.IfDup;

        /// <summary>
        /// Duplicates top stack item if its value is not 0. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = "There was no item left in the stack to check and duplicate.";
                return false;
            }

            byte[] data = opData.Peek();
            if (OpHelper.IsNotZero(data))
            {
                opData.Push(data);
            }

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation that pushes the number of stack items onto the stack.
    /// </summary>
    public class DEPTHOp : BaseOperation
    {
        public override OP OpValue => OP.DEPTH;

        /// <summary>
        /// Pushes the number of stack items onto the stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            opData.Push(OpHelper.IntToByteArray(opData.ItemCount));

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation that removes the top stack item.
    /// </summary>
    public class DROPOp : BaseOperation
    {
        public override OP OpValue => OP.DROP;

        /// <summary>
        /// Removes the top stack item. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = "There was no item left in the stack to drop.";
                return false;
            }

            opData.Pop(); // Throw away the top stack item

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation that duplicates the top stack item.
    /// </summary>
    public class DUPOp : BaseOperation
    {
        public override OP OpValue => OP.DUP;

        /// <summary>
        /// Duplicates the top stack item. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = "There was no item left in the stack to duplicate.";
                return false;
            }

            opData.Push(opData.Peek());

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation that removes the item before last from the stack.
    /// </summary>
    public class NIPOp : BaseOperation
    {
        public override OP OpValue => OP.NIP;

        /// <summary>
        /// Removes the second item from top of stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2)
            {
                error = "There was not enough items left in the stack to drop (\"nip\").";
                return false;
            }

            opData.PopAtIndex(1); // discard the popped value

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation that the item before last to the top of the stack.
    /// </summary>
    public class OVEROp : BaseOperation
    {
        public override OP OpValue => OP.OVER;

        /// <summary>
        /// Copies the second item from top of the stack to the top. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2)
            {
                error = "There was not enough items left in the stack to copy over.";
                return false;
            }

            byte[] data = opData.PeekAtIndex(1);
            opData.Push(data);

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation to pick an item from specified index and copies it to the top of the stack.
    /// </summary>
    public class PICKOp : BaseOperation
    {
        public override OP OpValue => OP.PICK;

        /// <summary>
        /// Copies the nth item from top of the stack to the top. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2) // At least 2 items is needed. 1 telling us the index and the other to copy
            {
                error = "There was not enough items left in the stack to copy to top.";
                return false;
            }

            byte[] data = opData.Pop();
            if (data.Length > 4) // Initial test to reject any huge numbers
            {
                error = "'n' is too big.";
                return false;
            }

            if (!OpHelper.TryConvertByteArrayToInt(data, out long n, true)) // TODO: set isStrict field base don BIP62
            {
                error = "Invalid number format.";
                return false;
            }

            if (n < 0)
            {
                error = "'n' can not be negative.";
                return false;
            }
            if (opData.ItemCount <= n) // 'n' is index so it can't be equal to ItemCount
            {
                error = "There was not enough items left in the stack to copy to top.";
                return false;
            }

            opData.Push(opData.PeekAtIndex((int)n));

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation to pick an item from specified index and move it to the top of the stack.
    /// </summary>
    public class ROLLOp : BaseOperation
    {
        public override OP OpValue => OP.ROLL;

        /// <summary>
        /// Moves the nth item from top of the stack to the top. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2) // At least 2 items is needed. 1 telling us the index and the other to move
            {
                error = "There was not enough items left in the stack to move to top.";
                return false;
            }

            byte[] data = opData.Pop();
            if (data.Length > 4) // Initial test to reject any huge numbers
            {
                error = "'n' is too big.";
                return false;
            }

            if (!OpHelper.TryConvertByteArrayToInt(data, out long n, true)) // TODO: set isStrict field based on BIP62
            {
                error = "Invalid number format.";
                return false;
            }
            if (n < 0)
            {
                error = "'n' can not be negative.";
                return false;
            }
            if (opData.ItemCount <= n) // 'n' is index so it can't be equal to ItemCount
            {
                error = "There was not enough items left in the stack to move to top.";
                return false;
            }

            opData.Push(opData.PopAtIndex((int)n));

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation to rotate the top 3 stack items.
    /// </summary>
    public class ROTOp : BaseOperation
    {
        public override OP OpValue => OP.ROT;

        /// <summary>
        /// Rotates top 3 items on top of the stack to the left. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 3)
            {
                error = "There was not enough items left in the stack to rotate.";
                return false;
            }

            byte[][] data = opData.Pop(3);
            // (x0 x1 x2 -> x1 x2 x0)
            opData.Push(new byte[3][] { data[1], data[2], data[0] });

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation to swap the top 2 stack items.
    /// </summary>
    public class SWAPOp : BaseOperation
    {
        public override OP OpValue => OP.SWAP;

        /// <summary>
        /// Swaps the position of top 2 items on top of the stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2)
            {
                error = "There was not enough items left in the stack to swap.";
                return false;
            }

            byte[][] data = opData.Pop(2);
            opData.Push(new byte[2][] { data[1], data[0] });

            error = null;
            return true;
        }
    }



    /// <summary>
    /// Operation to tuck the top stack item before item before last.
    /// </summary>
    public class TUCKOp : BaseOperation
    {
        public override OP OpValue => OP.TUCK;

        /// <summary>
        /// The item at the top of the stack is copied and inserted before the second-to-top item. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2)
            {
                error = "There was not enough items left in the stack to tuck.";
                return false;
            }

            byte[] data = opData.Peek();
            opData.Insert(data, 2);

            error = null;
            return true;
        }
    }

}
