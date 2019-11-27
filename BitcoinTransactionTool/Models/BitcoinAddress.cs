// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend;
using BitcoinTransactionTool.Backend.Encoders;
using BitcoinTransactionTool.Backend.MVVM;

namespace BitcoinTransactionTool.Models
{
    public class BitcoinAddress : ValidatableBase
    {
        private string _addr;
        public string Address
        {
            get { return _addr; }
            set
            {
                if (SetField(ref _addr, value))
                {
                    Validate();
                }
            }
        }


        public override void Validate()
        {
            ClearErrors(nameof(Address));

            if (string.IsNullOrEmpty(Address))
            {
                AddError(nameof(Address), "Address can not be empty.");
            }
            else if (Address.StartsWith("1") || Address.StartsWith("3"))
            {
                if (!new Base58().IsValid(Address))
                {
                    AddError(nameof(Address), "Invalid Base58 encoded address.");
                }
            }
            else if (Address.StartsWith("bc1"))
            {
                if (!new Bech32().IsValid(Address))
                {
                    AddError(nameof(Address), "Invalid Bech32 encoded address.");
                }
            }
            else
            {
                AddError(nameof(Address), "Invalid address format.");
            }
        }
    }


    public class SendingAddress : BitcoinAddress
    {
        private ulong _satBal;
        public ulong BalanceSatoshi
        {
            get { return _satBal; }
            set { SetField(ref _satBal, value); }
        }

        /// <summary>
        /// Balance is the user friendly way of showing the balance in decimal format.
        /// </summary>
        [DependsOnProperty(nameof(BalanceSatoshi))]
        public decimal Balance
        {
            get { return BalanceSatoshi * Constants.Satoshi; }
        }
    }


    public class ReceivingAddress : BitcoinAddress
    {
        public ReceivingAddress() { }

        public ReceivingAddress(string addr, decimal amount)
        {
            Address = addr;
            Payment = amount;
        }

        public ReceivingAddress(string addr, ulong amount)
        {
            Address = addr;
            Payment = amount * Constants.Satoshi;
        }

        /// <summary>
        /// PaymentSatoshi is the real payment value which is used in the code.
        /// </summary>
        [DependsOnProperty(nameof(Payment))]
        public ulong PaymentSatoshi
        {
            get { return (ulong)(Payment * (1 / Constants.Satoshi)); }
        }

        /// <summary>
        /// Payment is the user friendly way of showing amount to pay in decimal format.
        /// </summary>
        public decimal Payment
        {
            get { return _payment; }
            set { SetField(ref _payment, value); }
        }
        private decimal _payment;

    }
}
