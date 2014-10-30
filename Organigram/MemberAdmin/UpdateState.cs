using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pirate.Ldap;
using Pirate.Util.Logging;
using System.Net.Mail;

namespace MemberAdmin
{
  public class UpdateState
  {
    public UpdateOperation Operation { get; private set; }

    public LdapObject Current { get; private set; }

    public string Message { get; private set; }

    public int? DeleteRequestId { get; private set; }

    public IEnumerable<UpdateMail> AcceptMails { get; private set; }

    public IEnumerable<UpdateMail> RejectMails { get; private set; }

    public UpdateState(
      UpdateOperation operation, 
      LdapObject current, 
      IEnumerable<UpdateMail> acceptMails = null,
      IEnumerable<UpdateMail> rejectMails = null,
      string message = null,
      int? deleteRequestId = null)
    {
      Operation = operation;
      Current = current;
      AcceptMails = acceptMails;
      RejectMails = rejectMails;
      Message = message;
      DeleteRequestId = deleteRequestId;
    }
  }

  public class UpdateMail
  {
    public string Subject { get; private set; }

    public string Body { get; private set; }

    public MailAddress To { get; private set; }

    public UpdateMail(string subject, string body, MailAddress to)
    {
      Subject = subject;
      Body = body;
      To = to;
    }
  }

  public enum UpdateOperation
  {
    None,
    Update,
    Create,
    Delete
  }
}