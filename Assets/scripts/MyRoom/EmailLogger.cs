using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

internal class EmailLogger : MonoSingleton<EmailLogger>
{
    private List<string> LogList = new List<string>();

    /// <summary>
    /// 시간, 태그, 내용
    /// </summary>
    private const string CommonFormat = "{0}:::{1}:::{2}";
    private int SubjectIndex = 1;
    public string UserName;

    public void AddLog(string tag, string content)
    {
        LogList.Add(string.Format(CommonFormat, DateTime.Now.ToString("[yyyy:MM:dd:HH:mm:ss]"), tag, content));
        if (LogList.Count == 50)
        {
            Send();
            LogList.Clear();
        }
    }

    private void Send()
    {
        MailMessage mail = new MailMessage();

        mail.From = new MailAddress("koohoo89@fivestargames.co.kr"); // 보내는사람
        mail.To.Add("koohoo89@fivestargames.co.kr"); // 받는 사람
        mail.Subject = string.Format("{0}_{1}_{2}", DateTime.Now.ToString("[yyyy:MM:dd:HH:mm:ss]"), UserName, SubjectIndex);

        string sendContent = "";

        foreach(var text in LogList)
        {
            sendContent = string.Concat(sendContent, text);
            sendContent = string.Concat(sendContent, "\n");
        }

        mail.Body = sendContent;

        // 첨부파일 - 대용량은 안됨.
        //System.Net.Mail.Attachment attachment;
        //attachment = new System.Net.Mail.Attachment("D:\\Test\\2018-06-11-09-03-17-E7104.mp4"); // 경로 및 파일 선택
        //mail.Attachments.Add(attachment);

        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new System.Net.NetworkCredential("보내는사람 메일 주소", "보내는 사람 메일 비밀번호") as ICredentialsByHost; // 보내는사람 주소 및 비밀번호 확인
        smtpServer.EnableSsl = true;
        smtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;

        ServicePointManager.ServerCertificateValidationCallback =

        delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        { return true; };

        smtpServer.Send(mail);

        Debug.Log("success");

        SubjectIndex += 1;
    }

    private void OnApplicationQuit()
    {
        Send();
        LogList.Clear();
    }
}
