using System;

using CommonLibrary;

namespace BitcoinTransactionTool.Models
{
    public class BitcoinAddress : ValidatableBase
    {
        /// <summary>
        /// Address is the 160-bit hash of pubKey (Base58 Encoded) (RIPEMD-160)
        /// </summary>
        private string address;
        public string Address
        {
            get { return address; }
            set
            {
                if (address != value)
                {
                    address = value;
                    // Check to see if input is a valid bitcoin address
                    Validate(value);
                    RaisePropertyChanged("Address");
                }
            }
        }
    }

    public class SendingAddress : BitcoinAddress
    {
        /// <summary>
        /// BalanceSatoshi is the real balance value which is used in the code.
        /// </summary>
        private int balanceSatoshi;
        public int BalanceSatoshi
        {
            get { return balanceSatoshi; }
            set
            {
                if (balanceSatoshi != value)
                {
                    balanceSatoshi = value;
                    RaisePropertyChanged("BalanceSatoshi");
                    RaisePropertyChanged("Balance");
                }
            }
        }

        /// <summary>
        /// Balance is the user friendly way of showing the balance in decimal format.
        /// </summary>
        public decimal Balance
        {
            get { return BalanceSatoshi * BitcoinConversions.Satoshi; }
            set { }
        }
    }

    public class ReceivingAddress : BitcoinAddress
    {
        /// <summary>
        /// PaymentSatoshi is the real payment value which is used in the code.
        /// </summary>
        public UInt64 PaymentSatoshi
        {
            get { return (UInt64)(Payment * (1 / BitcoinConversions.Satoshi)); }
            set { }
        }

        /// <summary>
        /// Payment is the user friendly way of showing amount to pay in decimal format.
        /// </summary>
        private decimal payment;
        public decimal Payment
        {
            get { return payment; }
            set
            {
                if (payment != value)
                {
                    payment = value;
                    RaisePropertyChanged("Payment");
                    RaisePropertyChanged("PaymentSatoshi");
                }
            }
        }
    }
}
