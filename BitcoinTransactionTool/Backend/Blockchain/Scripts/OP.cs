// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System.ComponentModel;

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts
{
    public enum OP : byte
    {
        # region  Constants 

        [Description("Push an emtpy array of bytes onto the stack.")]
        _0 = 0x00,

        /* From 0x01 to 0x4b don't have names. They indicate size of the following data to be pushed. */

        [Description("Next 1 byte indicates size of the data to be pushed.")]
        PushData1 = 0x4c,
        [Description("Next 2 bytes indicate size of the data to be pushed.")]
        PushData2 = 0x4d,
        [Description("Next 4 bytes indicate size of the data to be pushed.")]
        PushData4 = 0x4e,
        [Description("Push number -1")]
        Negative1 = 0x4f,
        [Description("Reserved OP code. Transaction is invalid unless occuring in an unexecuted OP_IF branch")]
        Reserved = 0x50,
        [Description("Push number 1")]
        _1 = 0x51,
        [Description("Push number 2")]
        _2 = 0x52,
        [Description("Push number 3")]
        _3 = 0x53,
        [Description("Push number 4")]
        _4 = 0x54,
        [Description("Push number 5")]
        _5 = 0x55,
        [Description("Push number 6")]
        _6 = 0x56,
        [Description("Push number 7")]
        _7 = 0x57,
        [Description("Push number 8")]
        _8 = 0x58,
        [Description("Push number 9")]
        _9 = 0x59,
        [Description("Push number 10")]
        _10 = 0x5a,
        [Description("Push number 11")]
        _11 = 0x5b,
        [Description("Push number 12")]
        _12 = 0x5c,
        [Description("Push number 13")]
        _13 = 0x5d,
        [Description("Push number 14")]
        _14 = 0x5e,
        [Description("Push number 15")]
        _15 = 0x5f,
        [Description("Push number 16")]
        _16 = 0x60,

        #endregion



        #region Flow control

        [Description("Does nothing!")]
        NOP = 0x61,
        [Description("Transaction is invalid unless occuring in an unexecuted OP_IF branch")]
        VER = 0x62,
        [Description("Marks the beginning of a conditional statement.\nRemoves the top stack item, if the value is True, the statements are executed.\nFormat (must end with EndIf): [expression] if [statements] [else [statements]]* endif")]
        IF = 0x63,
        [Description("Marks the beginning of a conditional statement.\nRemoves the top stack item, if the value is False, the statements are executed.\nFormat (must end with EndIf): [expression] notif [statements] [else [statements]]* endif")]
        NotIf = 0x64,
        [Description("Transaction is invalid even when occuring in an unexecuted OP_IF branch")]
        VerIf = 0x65,
        [Description("Transaction is invalid even when occuring in an unexecuted OP_IF branch")]
        VerNotIf = 0x66,
        [Description("Marks the 'else' part of the conditional statement. Can only exist after an OP_IF or OP_NotIf.\nThe statements will only be executed if the preceding statements weren't executed.\nFormat: [expression] if [statements] [else [statements]]* endif")]
        ELSE = 0x67,
        [Description("Marks end of a conditional block. All blocks must end or the script is invalid.\nIt also can't exist without a prior OP_IF or OP_NotIf\nFormat: [expression] if [statements] [else [statements]]* endif")]
        EndIf = 0x68,
        [Description("Removes the top stack item and fails if it was False.")]
        VERIFY = 0x69,
        [Description("Creates an unspendable output. Used for attaching extra data to transactions.\n(it should start with a pushdata OP after OP_RETURN)")]
        RETURN = 0x6a,

        #endregion



        #region Stack
        [Description("Removes one item from stack and puts it on top of alt-stack.")]
        ToAltStack = 0x6b,
        [Description("Removes one item from alt-stack and puts it on top of stack.")]
        FromAltStack = 0x6c,
        [Description("Removes the top two stack items.")]
        DROP2 = 0x6d,
        [Description("Duplicates the top 2 stack items.")]
        DUP2 = 0x6e,
        [Description("Duplicates the top 3 stack items.")]
        DUP3 = 0x6f,
        [Description("Copies the pair of items two spaces back in the stack to the front.\nExample: x1 x2 x3 x4 -> x1 x2 x3 x4 x1 x2")]
        OVER2 = 0x70,
        [Description("The fifth and sixth items back are moved to the top of the stack.\nExample: x1 x2 x3 x4 x5 x6 -> x3 x4 x5 x6 x1 x2")]
        ROT2 = 0x71,
        [Description("Swaps the top two pairs of items.\nExample: x1 x2 x3 x4 -> x3 x4 x1 x2")]
        SWAP2 = 0x72,
        [Description("Duplicates top stack item if its value is not 0.")]
        IfDup = 0x73,
        [Description("Puts the number of stack items onto the stack.")]
        DEPTH = 0x74,
        [Description("Removes the top stack item.")]
        DROP = 0x75,
        [Description("Duplicates the top stack item.")]
        DUP = 0x76,
        [Description("Removes the second item from top of stack.\nExample: x1 x2 -> x2")]
        NIP = 0x77,
        [Description("Copies the second item from top of the stack to the top.\nExample: x1 x2 -> x1 x2 x1")]
        OVER = 0x78,
        [Description("The item n back in the stack is 'copied' to the top.\nExample: xn ... x2 x1 x0 -> xn ... x2 x1 x0 xn")]
        PICK = 0x79,
        [Description("The item n back in the stack is 'moved' to the top.\nExample: xn x(n-1) ... x2 x1 x0 -> x(n-1) ... x2 x1 x0 xn")]
        ROLL = 0x7a,
        [Description("The top three items on the stack are rotated to the left.\nExample: x1 x2 x3 -> x2 x3 x1")]
        ROT = 0x7b,
        [Description("The top two items on the stack are swapped.\nExample: x1 x2 -> x2 x1")]
        SWAP = 0x7c,
        [Description("The item at the top of the stack is copied and inserted before the second-to-top item.\nExample: x1 x2 -> x2 x1 x2")]
        TUCK = 0x7d,

        #endregion



        #region Splice

        [Description("[Disabled] Concatenates two strings.")]
        CAT = 0x7e,
        [Description("[Disabled] Returns a section of a string.")]
        SubStr = 0x7f,
        [Description("[Disabled] Keeps only characters left of the specified point in a string.")]
        LEFT = 0x80,
        [Description("[Disabled] Keeps only characters right of the specified point in a string.")]
        RIGHT = 0x81,
        [Description("Pushes the string length of the top element of the stack (without popping it).")]
        SIZE = 0x82,

        #endregion



        #region Bitwise logic


        [Description("[Disabled] Flips all of the bits in the input.")]
        INVERT = 0x83,
        [Description("[Disabled] Boolean and between each bit in the inputs.")]
        AND = 0x84,
        [Description("[Disabled] Boolean or between each bit in the inputs.")]
        OR = 0x85,
        [Description("[Disabled] Boolean exclusive or between each bit in the inputs.")]
        XOR = 0x86,
        [Description("Pops two top stack items, compares them and pushes the equality result onto the stack.\n1 if the inputs are exactly equal, 0 otherwise.")]
        EQUAL = 0x87,
        [Description("Runs both OP_EQUAL then OP_VERIFY respectively.")]
        EqualVerify = 0x88,
        [Description("Reserved OP code. Transaction is invalid unless occuring in an unexecuted OP_IF branch")]
        Reserved1 = 0x89,
        [Description("Reserved OP code. Transaction is invalid unless occuring in an unexecuted OP_IF branch")]
        Reserved2 = 0x8a,

        #endregion



        #region Arithmetic

        [Description("1 is added to the input.")]
        ADD1 = 0x8b,
        [Description("1 is subtracted from the input.")]
        SUB1 = 0x8c,
        [Description("[Disabled] The input is multiplied by 2.")]
        MUL2 = 0x8d,
        [Description("[Disabled] The input is divided by 2.")]
        DIV2 = 0x8e,
        [Description("The sign of the input is flipped.")]
        NEGATE = 0x8f,
        [Description("The input is made positive.")]
        ABS = 0x90,
        [Description("If the input is 0 or 1, it is flipped. Otherwise the output will be 0.")]
        NOT = 0x91,
        [Description("Returns 0 if the input is 0. 1 otherwise.")]
        NotEqual0 = 0x92,
        [Description("Adds two items")]
        ADD = 0x93,
        [Description("b is subtracted from a.")]
        SUB = 0x94,
        [Description("[Disabled] a is multiplied by b.")]
        MUL = 0x95,
        [Description("[Disabled] a is divided by b.")]
        DIV = 0x96,
        [Description("[Disabled] Returns the remainder after dividing a by b.")]
        MOD = 0x97,
        [Description("[Disabled] Shifts a left b bits, preserving sign.")]
        LSHIFT = 0x98,
        [Description("[Disabled] Shifts a right b bits, preserving sign.")]
        RSHIFT = 0x99,
        [Description("If both a and b are not 0, the output is 1. Otherwise 0.")]
        BoolAnd = 0x9a,
        [Description("If a or b is not 0, the output is 1. Otherwise 0.")]
        BoolOr = 0x9b,
        [Description("Returns 1 if the numbers are equal, 0 otherwise.")]
        NumEqual = 0x9c,
        [Description("Same as OP_NUMEQUAL, but runs OP_VERIFY afterward.")]
        NumEqualVerify = 0x9d,
        [Description("Returns 1 if the numbers are not equal, 0 otherwise.")]
        NumNotEqual = 0x9e,
        [Description("Returns 1 if a is less than b, 0 otherwise.")]
        LessThan = 0x9f,
        [Description("Returns 1 if a is greater than b, 0 otherwise.")]
        GreaterThan = 0xa0,
        [Description("Returns 1 if a is less than or equal to b, 0 otherwise.")]
        LessThanOrEqual = 0xa1,
        [Description("Returns 1 if a is greater than or equal to b, 0 otherwise.")]
        GreaterThanOrEqual = 0xa2,
        [Description("Returns the smaller of a and b.")]
        MIN = 0xa3,
        [Description("Returns the larger of a and b.")]
        MAX = 0xa4,
        [Description("Returns 1 if x is within the specified range (left-inclusive), 0 otherwise.")]
        WITHIN = 0xa5,

        #endregion



        #region Crypto
        [Description("The input is hashed using RIPEMD-160.")]
        RIPEMD160 = 0xa6,
        [Description("The input is hashed using SHA-1.")]
        SHA1 = 0xa7,
        [Description("The input is hashed using SHA-256.")]
        SHA256 = 0xa8,
        [Description("The input is hashed first with SHA-256 and then with RIPEMD-160.")]
        HASH160 = 0xa9,
        [Description("The input is hashed two times with SHA-256.")]
        HASH256 = 0xaa,
        [Description("All of the signature checking words will only match signatures to the data after the most recently-executed OP_CODESEPARATOR.")]
        CodeSeparator = 0xab,
        [Description("The entire transaction's outputs, inputs, and script (from the most recently-executed OP_CODESEPARATOR to the end) are hashed.\nThe signature used by OP_CHECKSIG must be a valid signature for this hash and public key. If it is, 1 is returned, 0 otherwise.")]
        CheckSig = 0xac,
        [Description("Runs OP_CheckSig first then OP_VERIFY.")]
        CheckSigVerify = 0xad,
        [Description("Compares the first signature against each public key until it finds an ECDSA match. Starting with the subsequent public key, it compares the second signature against each remaining public key until it finds an ECDSA match. The process is repeated until all signatures have been checked or not enough public keys remain to produce a successful result. All signatures need to match a public key. Because public keys are not checked again if they fail any signature comparison, signatures must be placed in the scriptSig using the same order as their corresponding public keys were placed in the scriptPubKey or redeemScript. If all signatures are valid, 1 is returned, 0 otherwise. Due to a bug, one extra unused value is removed from the stack.")]
        CheckMultiSig = 0xae,
        [Description("Runs OP_CheckMultiSig then OP_VERIFY.")]
        CheckMultiSigVerify = 0xaf,

        #endregion


        #region ??


        [Description("OP is ignored.")]
        NOP1 = 0xb0,
        [Description("Marks transaction as invalid if the top stack item is greater than the transaction's nLockTime field")]
        CheckLocktimeVerify = 0xb1,
        [Description("Marks transaction as invalid if the relative lock time of the input is\nnot equal to or longer than the value of the top stack item.")]
        CheckSequenceVerify = 0xb2,
        [Description("OP is ignored.")]
        NOP4 = 0xb3,
        [Description("OP is ignored.")]
        NOP5 = 0xb4,
        [Description("OP is ignored.")]
        NOP6 = 0xb5,
        [Description("OP is ignored.")]
        NOP7 = 0xb6,
        [Description("OP is ignored.")]
        NOP8 = 0xb7,
        [Description("OP is ignored.")]
        NOP9 = 0xb8,
        [Description("OP is ignored.")]
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
