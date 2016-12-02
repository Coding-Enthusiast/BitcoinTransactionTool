using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace CommonLibrary
{
    public class ValidatableBase : CommonBase, INotifyDataErrorInfo
    {
        private Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !errors.ContainsKey(propertyName))
            {
                return null;
            }
            else
            {
                return errors[propertyName];
            }
        }

        public bool HasErrors
        {
            get
            {
                return errors.Count > 0;
            }
        }


        public void AddError(string propertyName, string error)
        {
            if (!errors.ContainsKey(propertyName))
            {
                errors[propertyName] = new List<string>();
            }
            if (!errors[propertyName].Contains(error))
            {
                errors[propertyName].Add(error);
                RaiseErrorsChanged(propertyName);
            }
        }

        public void RemoveError(string propertyName, string error)
        {
            if (errors.ContainsKey(propertyName))
            {
                errors.Remove(propertyName);
                RaiseErrorsChanged(propertyName);
            }
        }

        public void RaiseErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
            {
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        public void Validate(string address)
        {
            //A bitcoin address starts with either 1 or 3 and is 26-35 alphanumeric characters
            if (!string.IsNullOrWhiteSpace(address) && address.Length >= 26 && address.Length <= 35)
            {
                if (address.StartsWith("1") || address.StartsWith("3"))
                {
                    if (Base58Check(address))
                    {
                        RemoveError("Address", "");
                    }
                    else
                    {
                        AddError("Address", "Address is not Base58 encoded!");
                    }
                }
                else
                {
                    AddError("Address", "Must start with 1 or 3.");
                }
            }
            else
            {
                AddError("Address", "Address can not be empty!");
            }
        }

        private bool Base58Check(string btcAddressFormat)
        {
            //Characters used in Base58Encoding which is all chars excluding "0OIl" 
            string Base58Chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

            BigInteger intData = 0;
            foreach (char c in btcAddressFormat)
            {
                int digit = Base58Chars.IndexOf(c);
                intData = intData * 58 + digit;
            }
            int leadingZeroCount = btcAddressFormat.TakeWhile(c => c == '1').Count();
            var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
            var bytesWithoutLeadingZeros =
                intData.ToByteArray()
                .Reverse()// to big endian
                .SkipWhile(b => b == 0);//strip sign byte
            byte[] dataAsByte = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();

            int lengthWithoutChecksum = dataAsByte.Length - 4;
            byte[] bytesWithoutChecksum = new byte[lengthWithoutChecksum];
            Array.Copy(dataAsByte, bytesWithoutChecksum, lengthWithoutChecksum);

            // calculate the checksum
            SHA256 sha = new SHA256Managed();
            byte[] hash1 = sha.ComputeHash(bytesWithoutChecksum);
            byte[] hash2 = sha.ComputeHash(hash1);

            if (hash2[0] != dataAsByte[lengthWithoutChecksum] ||
                hash2[1] != dataAsByte[lengthWithoutChecksum + 1] ||
                hash2[2] != dataAsByte[lengthWithoutChecksum + 2] ||
                hash2[3] != dataAsByte[lengthWithoutChecksum + 3])
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
