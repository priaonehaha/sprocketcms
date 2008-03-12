using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Sprocket.Utility
{
	public static class Crypto
	{
		public static string EncryptOneWay(string data)
		{
			byte[] buf = Encoding.ASCII.GetBytes(data);
			SHA512 sha = new SHA512Managed();
			byte[] enc = sha.ComputeHash(buf);
			return HexEncoding.ToString(enc);
		}

		public static byte[] CalculateMD5Hash(string data)
		{
			return new MD5CryptoServiceProvider().ComputeHash(Encoding.Unicode.GetBytes(data));
		}

		public static byte[] RC2Encrypt(string text, string encryptionKey, string vector)
		{
			byte[] key = CalculateMD5Hash(encryptionKey);
			byte[] IV = new byte[64];
			IV.Initialize();
			byte[] varr = Encoding.ASCII.GetBytes(vector);
			Buffer.BlockCopy(varr, 0, IV, 0, varr.Length > 64 ? 64 : varr.Length);
			RC2CryptoServiceProvider rc2 = new RC2CryptoServiceProvider();
			ICryptoTransform enc = rc2.CreateEncryptor(key, IV);
			MemoryStream stream = new MemoryStream();
			CryptoStream cs = new CryptoStream(stream, enc, CryptoStreamMode.Write);
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			cs.Write(bytes, 0, bytes.Length);
			cs.FlushFinalBlock();
			return stream.ToArray();
		}

		public static string RC2Decrypt(byte[] bytes, string encryptionKey, string vector)
		{
			byte[] key = CalculateMD5Hash(encryptionKey);
			byte[] IV = new byte[64];
			IV.Initialize();
			byte[] varr = Encoding.ASCII.GetBytes(vector);
			Buffer.BlockCopy(varr, 0, IV, 0, varr.Length > 64 ? 64 : varr.Length);

			RC2CryptoServiceProvider rc2 = new RC2CryptoServiceProvider();
			ICryptoTransform dec = rc2.CreateDecryptor(key, IV);
			MemoryStream stream = new MemoryStream(bytes);
			CryptoStream cs = new CryptoStream(stream, dec, CryptoStreamMode.Read);
			byte[] block = new byte[8000];
			MemoryStream ms = new MemoryStream();
			int n;
			do
			{
				n = cs.Read(block, 0, block.Length);
				ms.Write(block, 0, n);
			} while (n == block.Length);
			return Encoding.ASCII.GetString(ms.ToArray());
		}
	}
}
