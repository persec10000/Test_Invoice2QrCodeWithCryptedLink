using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace TestQr
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "Form1";


            //DecryptCombined("0x02000000266AD4F387FA9474E825B013B0232E73A398A5F72B79BC90D63BD1E45AE3AA5518828D187125BECC285D55FA7CAFED61", "Radames");
            //DecryptCombined("0x010000007854E155CEE338D5E34808BA95367D506B97C63FB5114DD4CE687FE457C1B5D5", "banana");
            DecryptCombined("0x01000000361BF8F4D6B864E9B681BA7725B5F35865016F909DF44E89CA147F21173DDCCD", "key");
            DecryptCombined("0x01000000C9061D89A44DABE14430785BCCFA7BEA4D9109A4F7485C31B3FD1FDB2802F894", "key");

            string strEnc = EncryptString("key", "054100064");
            Console.WriteLine("Result: {0}", strEnc);




        }

        private static string EncryptString(string FromSql, string Password)
        {
            byte[] passwordBytes = Encoding.Unicode.GetBytes(Password);
            SymmetricAlgorithm cryptoAlgo = null;
            cryptoAlgo = TripleDES.Create();
            cryptoAlgo.GenerateIV();
            cryptoAlgo.Padding = PaddingMode.PKCS7;
            cryptoAlgo.Mode = CipherMode.CBC;
            int version = 1;
            HashAlgorithm hashAlgo = null;
            hashAlgo = SHA1.Create();
            hashAlgo.TransformFinalBlock(passwordBytes, 0, passwordBytes.Length);
            int keySize = (version == 1 ? 16 : 32);
            cryptoAlgo.Key = hashAlgo.Hash.Take(keySize).ToArray();

            byte[] inBlock = Encoding.Unicode.GetBytes(FromSql);
            ICryptoTransform xfrm = cryptoAlgo.CreateEncryptor();
            byte[] decrypted = xfrm.TransformFinalBlock(inBlock, 0, inBlock.Length);
            int decryptLength = BitConverter.ToInt16(decrypted, 6);
            UInt32 magic = BitConverter.ToUInt32(decrypted, 0);
            if (magic != 0xbaadf00d)
            {
                Console.WriteLine("Encrypt failed");
                //throw new Exception("Decrypt failed");
            }
            bool isUtf16 = false;
            string decryptText = (isUtf16 ? Encoding.Unicode.GetString(decrypted) : Encoding.UTF8.GetString(decrypted));
            return decryptText;

        }
        private static byte[] DecryptString(string FromSql, string Password)
        {
            byte[] passwordBytes = Encoding.Unicode.GetBytes(Password);
            int version = BitConverter.ToInt32(StringToByteArray(FromSql.Substring(0, 8)), 0);
            HashAlgorithm hashAlgo = null;
            hashAlgo = SHA1.Create();
            SymmetricAlgorithm cryptoAlgo = null;
            cryptoAlgo = TripleDES.Create();
            cryptoAlgo.IV = StringToByteArray(FromSql.Substring(8, 16));
            var inBytes = Convert.FromBase64String(FromSql);
            byte[] encrypted = null;
            encrypted = StringToByteArray(FromSql.Substring(24));
            cryptoAlgo.Padding = PaddingMode.PKCS7;
            cryptoAlgo.Mode = CipherMode.CBC;
            hashAlgo.TransformFinalBlock(passwordBytes, 0, passwordBytes.Length);
            int keySize = (version == 1 ? 16 : 32);
            cryptoAlgo.Key = hashAlgo.Hash.Take(keySize).ToArray();

            ICryptoTransform xfrm = cryptoAlgo.CreateDecryptor();
            byte[] decrypted = xfrm.TransformFinalBlock(encrypted, 0, encrypted.Length);
            int decryptLength = BitConverter.ToInt16(decrypted, 6);
            UInt32 magic = BitConverter.ToUInt32(decrypted, 0);
            if (magic != 0xbaadf00d)
            {
                throw new Exception("Decrypt failed");
            }

            byte[] decryptedData = decrypted.Skip(8).ToArray();
            bool isUtf16 = (Array.IndexOf(decryptedData, (byte)0) != -1);
            string decryptText = (isUtf16 ? Encoding.Unicode.GetString(decryptedData) : Encoding.UTF8.GetString(decryptedData));

            Console.WriteLine("Result: {0}", decryptText);
            return decrypted;
        }

        void DecryptCombined(string FromSql, string Password)
        {
            // Encode password as UTF16-LE
            byte[] passwordBytes = Encoding.Unicode.GetBytes(Password);

            // Remove leading "0x"
            FromSql = FromSql.Substring(2);

            int version = BitConverter.ToInt32(StringToByteArray(FromSql.Substring(0, 8)), 0);
            byte[] encrypted = null;

            HashAlgorithm hashAlgo = null;
            SymmetricAlgorithm cryptoAlgo = null;
            int keySize = (version == 1 ? 16 : 32);

            if (version == 1)
            {
                hashAlgo = SHA1.Create();
                cryptoAlgo = TripleDES.Create();
                cryptoAlgo.IV = StringToByteArray(FromSql.Substring(8, 16));
                encrypted = StringToByteArray(FromSql.Substring(24));
            }
            else if (version == 2)
            {
                hashAlgo = SHA256.Create();
                cryptoAlgo = Aes.Create();
                cryptoAlgo.IV = StringToByteArray(FromSql.Substring(8, 32));
                encrypted = StringToByteArray(FromSql.Substring(40));
            }
            else
            {
                throw new Exception("Unsupported encryption");
            }

            cryptoAlgo.Padding = PaddingMode.PKCS7;
            cryptoAlgo.Mode = CipherMode.CBC;

            hashAlgo.TransformFinalBlock(passwordBytes, 0, passwordBytes.Length);
            cryptoAlgo.Key = hashAlgo.Hash.Take(keySize).ToArray();

            byte[] decrypted = cryptoAlgo.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length);
            int decryptLength = BitConverter.ToInt16(decrypted, 6);
            UInt32 magic = BitConverter.ToUInt32(decrypted, 0);
            if (magic != 0xbaadf00d)
            {
                throw new Exception("Decrypt failed");
            }

            byte[] decryptedData = decrypted.Skip(8).ToArray();
            bool isUtf16 = (Array.IndexOf(decryptedData, (byte)0) != -1);
            string decryptText = (isUtf16 ? Encoding.Unicode.GetString(decryptedData) : Encoding.UTF8.GetString(decryptedData));

            Console.WriteLine("Result: {0}", decryptText);
        }

        // Method taken from https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array?answertab=votes#tab-top
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        #endregion
    }
}

