using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using System.Security.Cryptography;
using System.Text;
using Web.Apps.Chat.Models;

namespace Web.Apps.Chat.Services;

public class ChatHandler(EventSink eventSink, ChatConfig chatConfig) : IService
{
    public byte[] EncryptedWordList { get; private set; }

    public void Initialize() => eventSink.ServerStarted += (_) => GenerateChat();

    private void GenerateChat() => EncryptedWordList =
        Encrypt(string.Join(string.Empty, chatConfig.Words.Select(x => $"{x}{chatConfig.TerminationCharacter}")));

    public byte[] Encrypt(string data)
    {
        var dataArray = Encoding.UTF8.GetBytes(data);
        var keyArray = chatConfig.CrispKey.StringToByteArray();

        using var aes = Aes.Create();

        aes.KeySize = 128;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = keyArray;

        using var cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);

        cryptoStream.Write(dataArray, 0, data.Length);
        cryptoStream.FlushFinalBlock();

        return ms.ToArray();
    }
}
