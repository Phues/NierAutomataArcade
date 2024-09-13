using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using DUCK.Crypto;

public class ShowFlag : MonoBehaviour
{
    public GameObject flagPanel;
    private Enemy enemy;
    private SimpleAESEncryption.AESEncryptedText _aesEncryptedText;
    
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<Enemy>();
        _aesEncryptedText.IV = "M8a53dRYKDiLg/TSNVzBEg==";
        _aesEncryptedText.EncryptedText = "Vlr3NQUqj0nNzOueM00aEcDeTCyXMSwVLUwG4eJOQmw6D7nOvYmkB7g87+DFoOOl72zHAWbeKTllFp32zXYCdQ==";
    }
    
    private string decryptFlag(string encryptedFlag, string aesKeyHex, string ivHex)
    {
        byte[] encryptedData = Convert.FromBase64String(encryptedFlag);
        byte[] key = StringToByteArray(aesKeyHex);
        byte[] iv = StringToByteArray(ivHex);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Padding = PaddingMode.None; // Set the padding mode to None

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead;
                        while ((bytesRead = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            msOutput.Write(buffer, 0, bytesRead);
                        }
                        return Encoding.ASCII.GetString(msOutput.ToArray());
                    }
                }
            }
        }
    }

    private byte[] StringToByteArray(string hex)
    {
        int length = hex.Length;
        byte[] bytes = new byte[length / 2];
        for (int i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.currentHP <= 1)
        {
            flagPanel.SetActive(true);
            flagPanel.GetComponentInChildren<TextMeshProUGUI>().text = SimpleAESEncryption.Decrypt(_aesEncryptedText, "AbcdjnpqsJHDAmldkal");
        }
    }
}
