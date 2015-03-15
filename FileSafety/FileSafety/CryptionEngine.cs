using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileSafety
{
    public static class CryptionEngine
    {
        public static IEnumerable<double> EncryptFile(this FileInfo fileInfo, string password)
        {
            FileStream fsCrypt = null;
            FileStream fsIn = null;
            CryptoStream cs = null;
            RijndaelManaged RMCrypto = null;
            try
            {
                
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] temp = UE.GetBytes(password).Where(p => p!= 0).Select(a =>a).ToArray();
                byte[] key = UE.GetBytes("****************").Where(p => p != 0).Select(a => a).ToArray();
                if (temp.Length > 16) throw new ArgumentException("Password can't be more than 16 characters");
                for (int i = 0; i < temp.Length; i++)
                {
                    key[i] = temp[i];
                }

                string cryptFile = fileInfo.FullName+".cryp";
                fsCrypt = new FileStream(cryptFile, FileMode.Create);

                RMCrypto = new RijndaelManaged();
                RMCrypto.Key = key;
                RMCrypto.IV = key;
                ICryptoTransform transform = RMCrypto.CreateEncryptor();
                cs = new CryptoStream(fsCrypt,
                    transform,
                    CryptoStreamMode.Write);

                fsIn = fileInfo.Open(FileMode.Open);
                var count = 1;
                int data;
                while ((data = fsIn.ReadByte()) != -1)
                {
                    cs.WriteByte((byte)data);
                    if (count % (fsIn.Length / 100) == 0) 
                        yield return count / (fsIn.Length / 100);
                    count++;
                }
                    
            }
            finally
            {
                if (fsIn != null) fsIn.Close();
                if (cs != null) cs.Close();
                if (fsCrypt != null) fsCrypt.Close();
                if (RMCrypto != null) RMCrypto.Dispose();
            }
        }

        public static IEnumerable<double> DecryptFile(this FileInfo fileInfo, string password)
        {
            FileStream fsCrypt = null;
            FileStream fsOut = null;
            CryptoStream cs = null;
            RijndaelManaged RMCrypto = null;
            try
            {
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] temp = UE.GetBytes(password).Where(p => p != 0).Select(a => a).ToArray();
                byte[] key = UE.GetBytes("****************").Where(p => p != 0).Select(a => a).ToArray();
                if (temp.Length > 16) throw new ArgumentException("Password can't be more than 16 characters");
                for (int i = 0; i < temp.Length; i++)
                {
                    key[i] = temp[i];
                }


                fsCrypt = fileInfo.Open(FileMode.Open);

                RMCrypto = new RijndaelManaged();

                cs = new CryptoStream(fsCrypt,
                   RMCrypto.CreateDecryptor(key, key),
                   CryptoStreamMode.Read);

                var outputFile = fileInfo.FullName.Remove(fileInfo.FullName.Length - 4);
                fsOut = new FileStream(outputFile, FileMode.Create);

                var count = 1;
                int data;
                while ((data = cs.ReadByte()) != -1)
                {
                    fsOut.WriteByte((byte)data);
                    if (count % (fsCrypt.Length / 100) == 0)
                        yield return count / (fsCrypt.Length / 100);
                    count++;
                }
            }
            finally
            {
                if (fsOut != null) fsOut.Close();
                if (cs != null) cs.Close();
                if (fsCrypt != null) fsCrypt.Close();
                if (RMCrypto != null) RMCrypto.Dispose();
            }
        }
    }
}
