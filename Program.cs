using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SQLServerCrypto;
namespace TestQr
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // EncryptByPassPhrase 
            var passphrase = "test1234";
            var encrypted = SQLServerCryptoMethod.EncryptByPassPhrase(@passphrase, "Hello world.");
            System.Console.WriteLine(encrypted.ToString().ToUpper());

            // DecryptByPassPhrase 
            var ciphertext = "0x0100000038C94F7223E0BA2F772B611857F9D45DAF781607CC77F4A856CF08CC2DB9DF14A0593259CB3A4A2BFEDB485C002CA04B6A98BEB1B47EB107";
            var password = "test1234";
            var decrypted = SQLServerCryptoMethod.DecryptByPassPhraseWithoutVerification(password, ciphertext);
            Console.WriteLine(decrypted);


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());




        }
    }
}
