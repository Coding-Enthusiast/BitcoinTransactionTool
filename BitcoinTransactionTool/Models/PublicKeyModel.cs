// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Encoders;
using BitcoinTransactionTool.Backend.MVVM;
using System;

namespace BitcoinTransactionTool.Models
{
    public class PublicKeyModel : ValidatableBase
    {
        private string _key;
        public string PubKey
        {
            get => _key;
            set
            {
                if (SetField(ref _key, value))
                {
                    Validate();
                }
            }
        }


        public override void Validate()
        {
            ClearErrors(nameof(PubKey));

            // TODO: replace this with Autarkysoft.Cryptocurrency.Keypairs.PublicKey for real validations!

            if (string.IsNullOrEmpty(PubKey))
            {
                AddError(nameof(PubKey), "PubKey can not be empty.");
            }
            else if (!Base16.IsValid(PubKey))
            {
                AddError(nameof(PubKey), "PubKey input is not a base-16 encoded string.");
            }
            else
            {
                byte[] ba = Base16.ToByteArray(PubKey);
                if (ba.Length != 33 && ba.Length != 65)
                {
                    AddError(nameof(PubKey), "PubKey input has invalid length.");
                }
                else if (ba.Length == 33 && ba[0] != 0x02 && ba[0] != 0x03)
                {
                    AddError(nameof(PubKey), "Compressed PubKey needs to start with 0x02 or 0x03.");
                }
                else if (ba.Length == 65 && ba[0] != 0x04)
                {
                    AddError(nameof(PubKey), "Uncompressed PubKey needs to start with 0x04.");
                }
            }
        }
    }
}
