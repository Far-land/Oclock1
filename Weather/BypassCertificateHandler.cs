using UnityEngine.Networking;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// 这是一个自定义的证书处理器，它会接受任何SSL证书，用于绕过验证错误。
/// </summary>
public class BypassCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // 直接返回true，意味着“无论证书是什么，都验证通过”
        return true;
    }
}