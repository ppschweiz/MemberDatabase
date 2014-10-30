using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirate.Ldap;
using System.Reflection;

namespace Pirate.Ldap.Data
{
  public static class Texts
  {
    public static class SectionItem
    {
      public const string UserMail = "UserMail";
    }

    public abstract class UserItem : TextItem
    {
      public const string NicknameField = "Nickname";
      public const string UniqueIdentiferField = "UniqueIdentifer";
    }

    public abstract class MemberItem : UserItem
    {
      public const string GivennameField = "Givenname";
      public const string SurnameField = "Surname";
    }

    public abstract class MembershipRequestItem : MemberItem
    {
      public const string NewSectionNameField = "NewSectionName";
    }

    public abstract class TransferItem : MemberItem
    {
      public const string OldSectionNameField = "OldSectionName";
      public const string NewSectionNameField = "NewSectionName";
    }

    public class CertificateCall : TextItem
    {
      public override string DefaultText
      {
        get { return string.Empty; }
      }

      public override IEnumerable<TextField> Fields
      {
        get { return new TextField[0]; }
      }
    }

    public class PasswordResetEmail : TextItem
    {
      public const string ResetUrlField = "ResetUrl";
      public const string NicknameField = "Nickname";
      public const string UniqueIdentiferField = "UniqueIdentifer";

      public override IEnumerable<TextField> Fields
      {
        get 
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(ResetUrlField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class MembershipRequestAbortMessage : MemberItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class ForumAccountCreatedMessage : UserItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class UpdateMailMessage : UserItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }
    
    public class UpdateMailEmail : UserItem
    {
      public const string UpdateUrl = "UpdateUrl";

      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(UpdateUrl, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class MembershipRequestPendingEmail : MemberItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class TransferRequestPendingEmail : MemberItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class TransferRequestPendingMessage : MemberItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }
    
    public class LeaveRequestPendingEmail : MemberItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class LeaveRequestPendingMessage : MemberItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class MembershipRequestPendingMessage : MembershipRequestItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class MembershipRequestAcceptedEmail : MembershipRequestItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(NewSectionNameField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class MembershipRequestRejectedMail : MembershipRequestItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(NewSectionNameField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class MembershipRequestAcceptedSectionEmail : MembershipRequestItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(SectionItem.UserMail, null);
          yield return new TextField(NewSectionNameField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class MembershipRequestRejectedSectionMail : MembershipRequestItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(SectionItem.UserMail, null);
          yield return new TextField(NewSectionNameField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class TransferAcceptedMail : TransferItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(OldSectionNameField, null);
          yield return new TextField(NewSectionNameField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class TransferAcceptedOldSectionMail : TransferItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(OldSectionNameField, null);
          yield return new TextField(NewSectionNameField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class TransferAcceptedNewSectionMail : TransferItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(OldSectionNameField, null);
          yield return new TextField(NewSectionNameField, null);
          yield return new TextField(SectionItem.UserMail, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class TransferRejectedMail : TransferItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(OldSectionNameField, null);
          yield return new TextField(NewSectionNameField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class TransferRejectedOldSectionMail : TransferItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(OldSectionNameField, null);
          yield return new TextField(NewSectionNameField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class TransferRejectedNewSectionMail : TransferItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(OldSectionNameField, null);
          yield return new TextField(NewSectionNameField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class LeaveAcceptedEmail : MemberItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class LeaveRejectedEmail : MemberItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class LeaveAcceptedSectionEmail : MemberItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(SectionItem.UserMail, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class LeaveRejectedSectionEmail : MemberItem
    {
      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(NicknameField, p => p.Nickname);
          yield return new TextField(UniqueIdentiferField, p => p.Id.ToString());
          yield return new TextField(GivennameField, p => p.Givenname);
          yield return new TextField(SurnameField, p => p.Surname);
          yield return new TextField(SectionItem.UserMail, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody"; }
      }
    }

    public class VerifyEmailForMemberEmail : TextItem
    {
      public const string ContinueUrlField = "ContinueUrl";

      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(ContinueUrlField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody\r\n\r\n$$$ContinueUrl"; }
      }
    }

    public class VerifyEmailForForumEmail : TextItem
    {
      public const string ContinueUrlField = "ContinueUrl";

      public override IEnumerable<TextField> Fields
      {
        get
        {
          yield return new TextField(ContinueUrlField, null);
        }
      }

      public override string DefaultText
      {
        get { return "Subject\r\nBody\r\n\r\n$$$ContinueUrl"; }
      }
    }

    public static TextItem GetItem(string id)
    {
      return Activator.CreateInstance(typeof(Texts).GetNestedType(id)) as TextItem;
    }

    public static IEnumerable<TextItem> GetTexts()
    {
      foreach (var subType in typeof(Texts).GetNestedTypes())
      {
        if (!subType.IsAbstract)
        {
          yield return Activator.CreateInstance(subType) as TextItem;
        }
      }
    }

    private static IEnumerable<StringPair> GetTranslations(TextItem item, Person user, IEnumerable<StringPair> addtionalTranslations)
    {
      foreach (var field in item.Fields)
      {
        if (field.Get != null)
        {
          yield return new StringPair(field.Key, field.Get(user));
        }
        else
        {
          var value = addtionalTranslations.Where(x => x.Item1 == field.Key).Single().Item2;
          yield return new StringPair(field.Key, value);
        }
      }
    }

    public static StringPair GetText(IDataAccess dataAccess, TextItem item, Person user, params StringPair[] addtionalTranslations)
    {
      return GetTextInternal(
        dataAccess,
        GetTextIds(item.Id, user),
        item.DefaultText,
        GetTranslations(item, user, addtionalTranslations));
    }

    public static StringPair GetText(IDataAccess dataAccess, TextItem item, string addtionalId, params StringPair[] addtionalTranslations)
    {
      return GetTextInternal(
        dataAccess,
        new[] { item.Id + "." + addtionalId, item.Id },
        item.DefaultText,
        addtionalTranslations);
    }

    private static IEnumerable<string> GetTextIds(string id, Person user)
    {
      if (user == null)
      {
        yield return id;
      }
      else
      {
        yield return id + "." + user.SectionDnState + "." + user.SectionDnLocation + "." + user.PreferredLanguage;
        yield return id + "." + user.SectionDnState + "." + user.SectionDnLocation;
        yield return id + "." + user.SectionDnState + "." + user.PreferredLanguage;
        yield return id + "." + user.SectionDnState;
        yield return id + "." + user.PreferredLanguage;
        yield return id;
      }
    }

    private static StringPair GetTextInternal(IDataAccess dataAccess, IEnumerable<string> ids, string defaultText, IEnumerable<StringPair> translations)
    {
      TextObject text = null;
      var idEnumerator = ids.GetEnumerator();
      idEnumerator.MoveNext();

      do
      {
        text = dataAccess.GetText(idEnumerator.Current);
      }
      while (text == null && idEnumerator.MoveNext());

      var fulltext = text == null ? defaultText : text.Text;

      foreach (var translation in translations)
      {
        fulltext = fulltext
          .Replace("$$$" + translation.Item1, translation.Item2);
      }

      var subject = fulltext.Head("\r\n");
      var body = fulltext.Tail("\r\n");

      return new StringPair(subject, body);
    }
  }
}
