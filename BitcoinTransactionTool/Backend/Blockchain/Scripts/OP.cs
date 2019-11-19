// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts
{
    public enum OP : byte
    {
        # region  Constants 

        /// <summary>
        /// Push an emtpy array of bytes onto the stack.
        /// </summary>
        _0 = 0x00,
        ///// <summary>
        ///// Same value as <see cref="_0"/> and same action (push empty array of bytes onto the stack)
        ///// </summary>
        //FALSE = _0,

        /* From 0x01 to 0x4b don't have names. They indicate size of the following data to be pushed. */

        /// <summary>
        /// Next 1 byte indicates size of the data to be pushed.
        /// </summary>
        PushData1 = 0x4c,
        /// <summary>
        /// Next 2 bytes indicate size of the data to be pushed.
        /// </summary>
        PushData2 = 0x4d,
        /// <summary>
        /// Next 4 bytes indicate size of the data to be pushed.
        /// </summary>
        PushData4 = 0x4e,
        /// <summary>
        /// Push number -1
        /// </summary>
        Negative1 = 0x4f,
        /// <summary>
        /// Reserved OP code. Transaction is invalid unless occuring in an unexecuted OP_IF branch
        /// </summary>
        Reserved = 0x50,
        /// <summary>
        /// Push number 1
        /// </summary>
        _1 = 0x51,
        ///// <summary>
        ///// Same value as <see cref="_1"/> and same action (push number 1)
        ///// </summary>
        //TRUE = _1,
        /// <summary>
        /// Push number 2
        /// </summary>
        _2 = 0x52,
        /// <summary>
        /// Push number 3
        /// </summary>
        _3 = 0x53,
        /// <summary>
        /// Push number 4
        /// </summary>
        _4 = 0x54,
        /// <summary>
        /// Push number 5
        /// </summary>
        _5 = 0x55,
        /// <summary>
        /// Push number 6
        /// </summary>
        _6 = 0x56,
        /// <summary>
        /// Push number 7
        /// </summary>
        _7 = 0x57,
        /// <summary>
        /// Push number 8
        /// </summary>
        _8 = 0x58,
        /// <summary>
        /// Push number 9
        /// </summary>
        _9 = 0x59,
        /// <summary>
        /// Push number 10
        /// </summary>
        _10 = 0x5a,
        /// <summary>
        /// Push number 11
        /// </summary>
        _11 = 0x5b,
        /// <summary>
        /// Push number 12
        /// </summary>
        _12 = 0x5c,
        /// <summary>
        /// Push number 13
        /// </summary>
        _13 = 0x5d,
        /// <summary>
        /// Push number 14
        /// </summary>
        _14 = 0x5e,
        /// <summary>
        /// Push number 15
        /// </summary>
        _15 = 0x5f,
        /// <summary>
        /// Push number 16
        /// </summary>
        _16 = 0x60,

        #endregion



        #region Flow control

        /// <summary>
        /// Does nothing!
        /// </summary>
        NOP = 0x61,
        /// <summary>
        /// Transaction is invalid unless occuring in an unexecuted OP_IF branch
        /// </summary>
        VER = 0x62,
        /// <summary>
        /// Marks the beginning of a conditional statement. 
        /// Removes the top stack item, if the value is True, the statements are executed.
        /// <para/>Format (must end with <see cref="EndIf"/>): [expression] if [statements] [else [statements]]* endif
        /// </summary>
        IF = 0x63,
        /// <summary>
        /// Marks the beginning of a conditional statement. 
        /// Removes the top stack item, if the value is False, the statements are executed.
        /// <para/>Format (must end with <see cref="EndIf"/>): [expression] notif [statements] [else [statements]]* endif
        /// </summary>
        NotIf = 0x64,
        /// <summary>
        /// Transaction is invalid even when occuring in an unexecuted OP_IF branch
        /// </summary>
        VerIf = 0x65,
        /// <summary>
        /// Transaction is invalid even when occuring in an unexecuted OP_IF branch
        /// </summary>
        VerNotIf = 0x66,
        /// <summary>
        /// Marks the "else" part of the conditional statement. Can only exist after an <see cref="IF"/> or <see cref="NotIf"/>.
        /// The statements will only be executed if the preceding statements weren't executed.
        /// <para/>Format: [expression] if [statements] [else [statements]]* endif
        /// </summary>
        ELSE = 0x67,
        /// <summary>
        /// Marks end of a conditional block. All blocks must end or the script is invalid. 
        /// It also can't exist without a prior <see cref="IF"/> or <see cref="NotIf"/>.
        /// <para/>Format: [expression] if [statements] [else [statements]]* endif
        /// </summary>
        EndIf = 0x68,
        /// <summary>
        /// Removes the top stack item and fails if it was False.
        /// </summary>
        VERIFY = 0x69,
        /// <summary>
        /// Creates an unspendable output. Used for attaching extra data to transactions. 
        /// (it should start with a pushdata OP after <see cref="RETURN"/>).
        /// </summary>
        RETURN = 0x6a,

        #endregion



        #region Stack

        /// <summary>
        /// Removes one item from stack and puts it on top of alt-stack.
        /// </summary>
        ToAltStack = 0x6b,
        /// <summary>
        /// Removes one item from alt-stack and puts it on top of stack.
        /// </summary>
        FromAltStack = 0x6c,
        /// <summary>
        /// Removes the top two stack items.
        /// </summary>
        DROP2 = 0x6d,
        /// <summary>
        /// Duplicates the top 2 stack items.
        /// </summary>
        DUP2 = 0x6e,
        /// <summary>
        /// Duplicates the top 3 stack items.
        /// </summary>
        DUP3 = 0x6f,
        /// <summary>
        /// Copies the pair of items two spaces back in the stack to the front.
        /// <para/> Example: x1 x2 x3 x4 -> x1 x2 x3 x4 x1 x2
        /// </summary>
        OVER2 = 0x70,
        /// <summary>
        /// The fifth and sixth items back are moved to the top of the stack.
        /// <para/> Example: x1 x2 x3 x4 x5 x6 -> x3 x4 x5 x6 x1 x2
        /// </summary>
        ROT2 = 0x71,
        /// <summary>
        /// Swaps the top two pairs of items.
        /// <para/> Example: x1 x2 x3 x4 -> x3 x4 x1 x2
        /// </summary>
        SWAP2 = 0x72,
        /// <summary>
        /// Duplicates top stack item if its value is not 0.
        /// </summary>
        IfDup = 0x73,
        /// <summary>
        /// Puts the number of stack items onto the stack.
        /// </summary>
        DEPTH = 0x74,
        /// <summary>
        /// Removes the top stack item.
        /// </summary>
        DROP = 0x75,
        /// <summary>
        /// Duplicates the top stack item.
        /// </summary>
        DUP = 0x76,
        /// <summary>
        /// Removes the second item from top of stack.
        /// <para/> Example: x1 x2 -> x2
        /// </summary>
        NIP = 0x77,
        /// <summary>
        /// Copies the second item from top of the stack to the top.
        /// <para/> Example: x1 x2 -> x1 x2 x1
        /// </summary>
        OVER = 0x78,
        /// <summary>
        /// The item n back in the stack is "copied" to the top.
        /// <para/> Example: xn ... x2 x1 x0 -> xn ... x2 x1 x0 xn
        /// </summary>
        PICK = 0x79,
        /// <summary>
        /// The item n back in the stack is "moved" to the top.
        /// <para/> Example: xn x(n-1) ... x2 x1 x0 -> x(n-1) ... x2 x1 x0 xn
        /// </summary>
        ROLL = 0x7a,
        /// <summary>
        /// The top three items on the stack are rotated to the left.
        /// <para/> Example: x1 x2 x3 -> x2 x3 x1
        /// </summary>
        ROT = 0x7b,
        /// <summary>
        /// The top two items on the stack are swapped.
        /// <para/> Example: x1 x2 -> x2 x1
        /// </summary>
        SWAP = 0x7c,
        /// <summary>
        /// The item at the top of the stack is copied and inserted before the second-to-top item.
        /// <para/> Example: x1 x2 -> x2 x1 x2
        /// </summary>
        TUCK = 0x7d,

        #endregion



        #region Splice
        /// <summary>
        /// [Disabled] Concatenates two strings.
        /// </summary>
        CAT = 0x7e,
        /// <summary>
        /// [Disabled] Returns a section of a string.
        /// </summary>
        SubStr = 0x7f,
        /// <summary>
        /// [Disabled] Keeps only characters left of the specified point in a string.
        /// </summary>
        LEFT = 0x80,
        /// <summary>
        /// [Disabled] Keeps only characters right of the specified point in a string.
        /// </summary>
        RIGHT = 0x81,
        /// <summary>
        /// Pushes the string length of the top element of the stack (without popping it).
        /// </summary>
        SIZE = 0x82,

        #endregion



        #region Bitwise logic

        /// <summary>
        /// [Disabled] Flips all of the bits in the input.
        /// </summary>
        INVERT = 0x83,
        /// <summary>
        /// [Disabled] Boolean and between each bit in the inputs.
        /// </summary>
        AND = 0x84,
        /// <summary>
        /// [Disabled] Boolean or between each bit in the inputs.
        /// </summary>
        OR = 0x85,
        /// <summary>
        /// [Disabled] Boolean exclusive or between each bit in the inputs. 
        /// </summary>
        XOR = 0x86,
        /// <summary>
        /// Pops two top stack items, compares them and pushes the equality result onto the stack. 
        /// 1 if the inputs are exactly equal, 0 otherwise.
        /// </summary>
        EQUAL = 0x87,
        /// <summary>
        /// Runs both <see cref="EQUAL"/> then <see cref="VERIFY"/> respectively.
        /// </summary>
        EqualVerify = 0x88,
        /// <summary>
        /// Reserved OP code. Transaction is invalid unless occuring in an unexecuted OP_IF branch
        /// </summary>
        Reserved1 = 0x89,
        /// <summary>
        /// Reserved OP code. Transaction is invalid unless occuring in an unexecuted OP_IF branch
        /// </summary>
        Reserved2 = 0x8a,

        #endregion



        #region Arithmetic
        /// <summary>
        /// 1 is added to the input.
        /// </summary>
        ADD1 = 0x8b,
        /// <summary>
        /// 1 is subtracted from the input.
        /// </summary>
        SUB1 = 0x8c,
        /// <summary>
        /// [Disabled] The input is multiplied by 2.
        /// </summary>
        MUL2 = 0x8d,
        /// <summary>
        /// [Disabled] The input is divided by 2.
        /// </summary>
        DIV2 = 0x8e,
        /// <summary>
        /// The sign of the input is flipped.
        /// </summary>
        NEGATE = 0x8f,
        /// <summary>
        /// The input is made positive.
        /// </summary>
        ABS = 0x90,
        /// <summary>
        /// If the input is 0 or 1, it is flipped. Otherwise the output will be 0.
        /// </summary>
        NOT = 0x91,
        /// <summary>
        /// Returns 0 if the input is 0. 1 otherwise.
        /// </summary>
        NotEqual0 = 0x92,
        /// <summary>
        /// Adds two items
        /// </summary>
        ADD = 0x93,
        /// <summary>
        /// b is subtracted from a.
        /// </summary>
        SUB = 0x94,
        /// <summary>
        /// [Disabled] a is multiplied by b.
        /// </summary>
        MUL = 0x95,
        /// <summary>
        /// [Disabled] a is divided by b.
        /// </summary>
        DIV = 0x96,
        /// <summary>
        /// [Disabled] Returns the remainder after dividing a by b. 
        /// </summary>
        MOD = 0x97,
        /// <summary>
        /// [Disabled] Shifts a left b bits, preserving sign.
        /// </summary>
        LSHIFT = 0x98,
        /// <summary>
        /// [Disabled] Shifts a right b bits, preserving sign.
        /// </summary>
        RSHIFT = 0x99,
        /// <summary>
        /// If both a and b are not 0, the output is 1. Otherwise 0.
        /// </summary>
        BoolAnd = 0x9a,
        /// <summary>
        /// If a or b is not 0, the output is 1. Otherwise 0.
        /// </summary>
        BoolOr = 0x9b,
        /// <summary>
        /// Returns 1 if the numbers are equal, 0 otherwise.
        /// </summary>
        NumEqual = 0x9c,
        /// <summary>
        /// Same as OP_NUMEQUAL, but runs OP_VERIFY afterward.
        /// </summary>
        NumEqualVerify = 0x9d,
        /// <summary>
        /// Returns 1 if the numbers are not equal, 0 otherwise.
        /// </summary>
        NumNotEqual = 0x9e,
        /// <summary>
        /// Returns 1 if a is less than b, 0 otherwise.
        /// </summary>
        LessThan = 0x9f,
        /// <summary>
        /// Returns 1 if a is greater than b, 0 otherwise.
        /// </summary>
        GreaterThan = 0xa0,
        /// <summary>
        /// Returns 1 if a is less than or equal to b, 0 otherwise.
        /// </summary>
        LessThanOrEqual = 0xa1,
        /// <summary>
        /// Returns 1 if a is greater than or equal to b, 0 otherwise.
        /// </summary>
        GreaterThanOrEqual = 0xa2,
        /// <summary>
        /// Returns the smaller of a and b.
        /// </summary>
        MIN = 0xa3,
        /// <summary>
        /// Returns the larger of a and b.
        /// </summary>
        MAX = 0xa4,
        /// <summary>
        /// Returns 1 if x is within the specified range (left-inclusive), 0 otherwise.
        /// </summary>
        WITHIN = 0xa5,

        #endregion



        #region Crypto

        /// <summary>
        /// The input is hashed using RIPEMD-160.
        /// </summary>
        RIPEMD160 = 0xa6,
        /// <summary>
        /// The input is hashed using SHA-1.
        /// </summary>
        SHA1 = 0xa7,
        /// <summary>
        /// The input is hashed using SHA-256.
        /// </summary>
        SHA256 = 0xa8,
        /// <summary>
        /// The input is hashed first with SHA-256 and then with RIPEMD-160.
        /// </summary>
        HASH160 = 0xa9,
        /// <summary>
        /// The input is hashed two times with SHA-256.
        /// </summary>
        HASH256 = 0xaa,
        /// <summary>
        /// All of the signature checking words will only match signatures to the data after the most recently-executed OP_CODESEPARATOR.
        /// </summary>
        CodeSeparator = 0xab,
        /// <summary>
        /// The entire transaction's outputs, inputs, and script (from the most recently-executed OP_CODESEPARATOR to the end) are hashed. 
        /// The signature used by OP_CHECKSIG must be a valid signature for this hash and public key. If it is, 1 is returned, 0 otherwise.
        /// </summary>
        CheckSig = 0xac,
        /// <summary>
        /// Runs <see cref="CheckSig"/> first then <see cref="VERIFY"/>.
        /// </summary>
        CheckSigVerify = 0xad,
        /// <summary>
        /// Compares the first signature against each public key until it finds an ECDSA match. Starting with the subsequent public key, it compares the second signature against each remaining public key until it finds an ECDSA match. The process is repeated until all signatures have been checked or not enough public keys remain to produce a successful result. All signatures need to match a public key. Because public keys are not checked again if they fail any signature comparison, signatures must be placed in the scriptSig using the same order as their corresponding public keys were placed in the scriptPubKey or redeemScript. If all signatures are valid, 1 is returned, 0 otherwise. Due to a bug, one extra unused value is removed from the stack.
        /// </summary>
        CheckMultiSig = 0xae,
        /// <summary>
        /// Runs <see cref="CheckMultiSig"/> then <see cref="VERIFY"/>.
        /// </summary>
        CheckMultiSigVerify = 0xaf,

        #endregion


        #region ??

        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP1 = 0xb0,
        /// <summary>
        /// Marks transaction as invalid if the top stack item is greater than the transaction's nLockTime field
        /// </summary>
        CheckLocktimeVerify = 0xb1,
        ///// <summary>
        ///// Same as: <see cref="CheckLocktimeVerify"/>
        ///// </summary>
        //NOP2 = CheckLocktimeVerify,
        /// <summary>
        /// Marks transaction as invalid if the relative lock time of the input is 
        /// not equal to or longer than the value of the top stack item.
        /// </summary>
        CheckSequenceVerify = 0xb2,
        ///// <summary>
        ///// Same as: <see cref="CheckSequenceVerify"/>.
        ///// </summary>
        //NOP3 = CheckSequenceVerify,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP4 = 0xb3,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP5 = 0xb4,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP6 = 0xb5,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP7 = 0xb6,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP8 = 0xb7,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP9 = 0xb8,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP10 = 0xb9,

        // TODO: 0xba to 0xf9 are missing, check if it is unasigned or missed here. unassigned OP codes make tx invalid.
        // ALSO: 0xfc is missing

        // TODO: do we need the following 5?!
        // template matching params
        //OP_SMALLINTEGER = 0xfa,
        //OP_PUBKEYS = 0xfb,
        //OP_PUBKEYHASH = 0xfd,
        //OP_PUBKEY = 0xfe,
        //OP_INVALIDOPCODE = 0xff,

        #endregion

    }
}
