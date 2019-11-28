// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend;
using BitcoinTransactionTool.Backend.Encoders;
using BitcoinTransactionTool.Backend.MVVM;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace BitcoinTransactionTool.ViewModels
{
    public class QrViewModel : ViewModelBase
    {
        public QrViewModel()
        {
            EncodingList = Enum.GetValues(typeof(Encoders)).Cast<Encoders>();
            ShowCommand = new RelayCommand(ShowQr, () => !string.IsNullOrEmpty(RawTx));
        }



        public enum Encoders
        {
            UTF8,
            Base16,
            Base58,
            Base58Check,
            Base43,
            Base64
        }

        private readonly Base58 b58enc = new Base58();
        private readonly Base43 b43enc = new Base43();



        public IEnumerable<Encoders> EncodingList { get; private set; }


        private Encoders _encIn;
        public Encoders SelectedInEncoder
        {
            get => _encIn;
            set => SetField(ref _encIn, value);
        }


        private Encoders _encOut;
        public Encoders SelectedOutEncoder
        {
            get => _encOut;
            set => SetField(ref _encOut, value);
        }


        private string _tx;
        public string RawTx
        {
            get => _tx;
            set
            {
                if (SetField(ref _tx, value))
                {
                    ShowCommand.RaiseCanExecuteChanged();
                }
            }
        }


        private BitmapImage _qr;
        public BitmapImage QRCode
        {
            get => _qr;
            private set => SetField(ref _qr, value);
        }


        public RelayCommand ShowCommand { get; private set; }
        private void ShowQr()
        {
            try
            {
                Errors = string.Empty;

                byte[] input = GetInput();
                string converted = (SelectedInEncoder == SelectedOutEncoder) ? RawTx : GetOutput(input);


                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(converted, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                using (MemoryStream memory = new MemoryStream())
                {
                    qrCodeImage.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    QRCode = bitmapimage;
                }
            }
            catch (FormatException ex)
            {
                Errors = $"Invalid input format. Exception details: {ex.Message}";
                QRCode = null;
            }
            catch (Exception ex)
            {
                Errors = $"An unexpected exception of type {ex.GetType()} was thrown with message: {ex.Message}";
                QRCode = null;
            }
        }

        private byte[] GetInput()
        {
            switch (SelectedInEncoder)
            {
                case Encoders.UTF8:
                    return Encoding.UTF8.GetBytes(RawTx);
                case Encoders.Base16:
                    return Base16.ToByteArray(RawTx);
                case Encoders.Base58:
                    return b58enc.Decode(RawTx);
                case Encoders.Base58Check:
                    return b58enc.DecodeWithCheckSum(RawTx);
                case Encoders.Base43:
                    return b43enc.Decode(RawTx);
                case Encoders.Base64:
                    return Convert.FromBase64String(RawTx);
                default:
                    throw new ArgumentException($"Encoder type {SelectedInEncoder} is not defined.");
            }
        }
        private string GetOutput(byte[] input)
        {
            switch (SelectedOutEncoder)
            {
                case Encoders.UTF8:
                    return Encoding.UTF8.GetString(input);
                case Encoders.Base16:
                    return input.ToBase16();
                case Encoders.Base58:
                    return b58enc.Encode(input);
                case Encoders.Base58Check:
                    return b58enc.EncodeWithCheckSum(input);
                case Encoders.Base43:
                    return b43enc.Encode(input);
                case Encoders.Base64:
                    return Convert.ToBase64String(input);
                default:
                    throw new ArgumentException($"Encoder type {SelectedInEncoder} is not defined.");
            }
        }

    }
}
