using UnityEngine.Networking;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// ����һ���Զ����֤�鴦��������������κ�SSL֤�飬�����ƹ���֤����
/// </summary>
public class BypassCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // ֱ�ӷ���true����ζ�š�����֤����ʲô������֤ͨ����
        return true;
    }
}