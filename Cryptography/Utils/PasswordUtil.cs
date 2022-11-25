using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptography.Utils
{
    public class PasswordUtil
    {
        public string GetDataBasePassword(int height, int width, int position, int shift, string salt, string cipherPassword)
        {
            string password = null;

            if (!string.IsNullOrEmpty(cipherPassword))
            {
                var skitalaBytes = Encoding.UTF8.GetBytes(cipherPassword);
                var saltData = Encoding.UTF8.GetBytes(salt);

                var bytes = new byte[ height * width ];

                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        bytes[j * width + k] = skitalaBytes[j + k * height];
                    }
                }

                var passData = new List<byte>();
                int i = 0;
                while (saltData[(position + i * shift) % bytes.Length] != bytes[(position + i * shift) % bytes.Length])
                {
                    byte b;
                    if (saltData[(position + i * shift) % bytes.Length] > bytes[(position + i * shift) % bytes.Length])
                    {
                        b = (byte)(128 + (int)bytes[(position + i * shift) % bytes.Length] - (int)saltData[(position + i * shift) % bytes.Length]);
                    }
                    else
                    {
                        b = (byte)(bytes[(position + i * shift) % bytes.Length] - saltData[(position + i * shift) % bytes.Length]);
                    }
                    passData.Add(b);
                    i++;
                }
                password = Encoding.UTF8.GetString(passData.ToArray());
            }

            return password;
        }

        public string GetCipherPassword(int height, int width, int position, int shift, string salt, string password)
        {
            var passwordData = Encoding.UTF8.GetBytes(password);
            var saltData = Encoding.UTF8.GetBytes(salt);

            int i = 0;
            foreach (var p in passwordData)
            {
                var b = (byte)(((int)p + (int)saltData[(position + i * shift) % saltData.Length]) % 128);
                saltData[(position + i * shift) % saltData.Length] = b;
                i++;
            }

            byte[] skitalaBytes = new byte[ height * width ];

            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < height; k++)
                {
                    skitalaBytes[height * j + k] = saltData[j + width * k];
                }
            }

            return Encoding.UTF8.GetString(skitalaBytes);
        }
    }
}
