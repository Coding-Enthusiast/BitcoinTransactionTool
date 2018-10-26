using CommonLibrary;

namespace BitcoinTransactionTool.Models
{
    public class BitcoinAddress : ValidatableBase
    {
        private string address;
        public string Address
        {
            get { return address; }
            set
            {
                if (SetField(ref address, value))
                {
                    Validate(value);
                }
            }
        }
    }


    public class SendingAddress : BitcoinAddress
    {
        private ulong balanceSatoshi;
        public ulong BalanceSatoshi
        {
            get { return balanceSatoshi; }
            set { SetField(ref balanceSatoshi, value); }
        }

        /// <summary>
        /// Balance is the user friendly way of showing the balance in decimal format.
        /// </summary>
        [DependsOnProperty(nameof(BalanceSatoshi))]
        public decimal Balance
        {
            get { return BalanceSatoshi * BitcoinConversions.Satoshi; }
        }
    }


    public class ReceivingAddress : BitcoinAddress
    {
        /// <summary>
        /// PaymentSatoshi is the real payment value which is used in the code.
        /// </summary>
        [DependsOnProperty(nameof(Payment))]
        public ulong PaymentSatoshi
        {
            get { return (ulong)(Payment * (1 / BitcoinConversions.Satoshi)); }
        }

        /// <summary>
        /// Payment is the user friendly way of showing amount to pay in decimal format.
        /// </summary>
        public decimal Payment
        {
            get { return payment; }
            set { SetField(ref payment, value); }
        }
        private decimal payment;

    }
}
