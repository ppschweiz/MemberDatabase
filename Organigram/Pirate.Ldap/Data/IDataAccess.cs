using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pirate.Ldap.Data
{
  public interface IDataAccess
  {
    void AddEmailAddressChange(EmailAddressChange change);

    EmailAddressChange GetEmailAddressChange(string id);

    void RemoveEmailAddressChange(string id);

    void RemoveOutdatedEmailAddressChanges();

    void AddCertificateEntry(CertificateEntry entry);

    void RemoveCertificateEntry(long key);

    CertificateEntry FindCertificateEntry(long key);

    int GetCertificateAuthorizationLevel(string fingerprint, int userUniqueIdentifier);

    IEnumerable<CertificateEntry> ListCertificateEntries(int userUniqueIdentifier);

    IEnumerable<CertificateEntry> ListCertificateEntries();

    void AuthorizeCertificateEntry(long key, int authorizationLevel);

    int CreateRequest(Request request);

    void DeleteRequest(int id);

    Request GetRequest(int id);

    IEnumerable<Request> GetRequests(string dn);

    IEnumerable<Request> GetallRequests();

    void SetText(TextObject text);

    TextObject GetText(string id);

    IEnumerable<TextObject> GetAllTexts();

    void DeleteText(string id);

    void SaveSearchTemplate(SearchTemplate template);

    IEnumerable<SearchTemplate> ListSearchTemplates();

    void DeleteSearchTemplate(string name);

    void Check();

    void Close();
  }
}