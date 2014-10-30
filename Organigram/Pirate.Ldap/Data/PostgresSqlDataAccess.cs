using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Devart.Data;
using Devart.Data.PostgreSql;
using Pirate.Ldap.Config;

namespace Pirate.Ldap.Data
{
  public class PostgresSqlDataAccess : IDataAccess
  {
    private object _lock = new object();

    private IConfig _config;

    private PgSqlConnection _connection;

    public PostgresSqlDataAccess()
    {
      _config = new Config.Config();
    }

    public void AddEmailAddressChange(EmailAddressChange change)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("INSERT INTO emailaddresschange (id, username, removeaddress, addaddress, created) VALUES (@Id, @Username, @RemoveAddress, @AddAddress, @Created)", _connection);
        command.Parameters.Add(new PgSqlParameter("@Id", change.Id));
        command.Parameters.Add(new PgSqlParameter("@Username", change.Username));
        command.Parameters.Add(new PgSqlParameter("@RemoveAddress", change.RemoveAddress));
        command.Parameters.Add(new PgSqlParameter("@AddAddress", change.AddAddress));
        command.Parameters.Add(new PgSqlParameter("@Created", change.Creation));
        command.ExecuteNonQuery();
      }
    }

    public EmailAddressChange GetEmailAddressChange(string id)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("SELECT username, removeaddress, addaddress, created FROM emailaddresschange WHERE id = @Id", _connection);
        command.Parameters.Add(new PgSqlParameter("@Id", id));

        using (var reader = command.ExecuteReader())
        {
          if (reader.Read())
          {
            var username = reader.GetString(0);
            var removeAddress = reader.GetString(1);
            var addAddress = reader.GetString(2);
            var creation = reader.GetDateTime(3);
            return new EmailAddressChange(id, username, removeAddress, addAddress, creation);
          }
          else
          {
            return null;
          }
        }
      }
    }

    public void RemoveEmailAddressChange(string id)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("DELETE FROM emailaddresschange WHERE id = @Id", _connection);
        command.Parameters.Add(new PgSqlParameter("@Id", id));
        command.ExecuteNonQuery();
      }
    }

    public void RemoveOutdatedEmailAddressChanges()
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("DELETE FROM emailaddresschange WHERE created < @Date", _connection);
        command.Parameters.Add(new PgSqlParameter("@Date", DateTime.Now.AddHours(-2)));
        command.ExecuteNonQuery();
      }
    }

    public void AddCertificateEntry(CertificateEntry entry)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("INSERT INTO certificateentry (key, fingerprint, authorizationlevel, useruniqueidentifier, comment, certificatedata) VALUES (@key, @fingerprint, @authorizationlevel, @useruniqueidentifier, @comment, @certificatedata)", _connection);
        command.Parameters.Add(new PgSqlParameter("@key", entry.Key));
        command.Parameters.Add(new PgSqlParameter("@fingerprint", entry.Fingerprint));
        command.Parameters.Add(new PgSqlParameter("@authorizationlevel", entry.AuthorizationLevel));
        command.Parameters.Add(new PgSqlParameter("@useruniqueidentifier", entry.UserUniqueIdentifier));
        command.Parameters.Add(new PgSqlParameter("@comment", entry.Comment));
        command.Parameters.Add(new PgSqlParameter("@certificatedata", entry.CertificateData));
        command.ExecuteNonQuery();
      }
    }

    public void RemoveCertificateEntry(long key)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("DELETE FROM certificateentry WHERE key = @Key", _connection);
        command.Parameters.Add(new PgSqlParameter("@Key", key));
        command.ExecuteNonQuery();
      }
    }

    public CertificateEntry FindCertificateEntry(long key)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("SELECT fingerprint, authorizationlevel, userUniqueIdentifier, comment, certificatedata FROM certificateentry WHERE key = @key", _connection);
        command.Parameters.Add(new PgSqlParameter("@key", key));

        using (var reader = command.ExecuteReader())
        {
          if (reader.Read())
          {
            var fingerprint = reader.GetString(0);
            var authorizationLevel = reader.GetInt32(1);
            var userUniqueIdentifier = reader.GetInt32(2);
            var comment = reader.GetString(3);
            var certificateData = reader.GetString(4);
            return new CertificateEntry(key, fingerprint, authorizationLevel, userUniqueIdentifier, comment, certificateData);
          }
          else
          {
            return null;
          }
        }
      }
    }

    public IEnumerable<CertificateEntry> ListCertificateEntries()
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("SELECT key, fingerprint, authorizationlevel, useruniqueidentifier, comment, certificatedata FROM certificateentry", _connection);

        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            var key = reader.GetInt64(0);
            var fingerprint = reader.GetString(1);
            var authorizationLevel = reader.GetInt32(2);
            var userUniqueIdentifier = reader.GetInt32(3);
            var comment = reader.GetString(4);
            var certificateData = reader.GetString(5);
            yield return new CertificateEntry(key, fingerprint, authorizationLevel, userUniqueIdentifier, comment, certificateData);
          }
        }
      }
    }

    public IEnumerable<CertificateEntry> ListCertificateEntries(int userUniqueIdentifier)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("SELECT key, fingerprint, authorizationlevel, comment, certificatedata FROM certificateentry WHERE useruniqueidentifier = @useruniqueidentifier", _connection);
        command.Parameters.Add(new PgSqlParameter("@useruniqueidentifier", userUniqueIdentifier));

        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            var key = reader.GetInt64(0);
            var fingerprint = reader.GetString(1);
            var authorizationLevel = reader.GetInt32(2);
            var comment = reader.GetString(3);
            var certificateData = reader.GetString(4);
            yield return new CertificateEntry(key, fingerprint, authorizationLevel, userUniqueIdentifier, comment, certificateData);
          }
        }
      }
    }

    public void AuthorizeCertificateEntry(long key, int authorizationLevel)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("UPDATE certificateentry SET authorizationlevel = @authorizationlevel WHERE key = @key", _connection);
        command.Parameters.Add(new PgSqlParameter("@key", key));
        command.Parameters.Add(new PgSqlParameter("@authorizationlevel", authorizationLevel));
        command.ExecuteNonQuery();
      }
    }

    public int GetCertificateAuthorizationLevel(string fingerprint, int userUniqueIdentifier)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("SELECT authorizationlevel, userUniqueIdentifier FROM certificateentry WHERE fingerprint = @fingerprint", _connection);
        command.Parameters.Add(new PgSqlParameter("@fingerprint", fingerprint));

        using (var reader = command.ExecuteReader())
        {
          if (reader.Read())
          {
            var authorizationLevel = reader.GetInt32(0);
            var actualUserUniqueIdentifier = reader.GetInt32(1);

            if (actualUserUniqueIdentifier == userUniqueIdentifier)
            {
              return authorizationLevel;
            }
            else
            {
              return -1;
            }
          }
          else
          {
            return -1;
          }
        }
      }
    }

    public void Check()
    {
      lock (_lock)
      {
        if (_connection == null ||
            _connection.State != System.Data.ConnectionState.Open)
        {
          _connection = new PgSqlConnection(_config.PostgresConnectionString);
          _connection.Unicode = true;
          _connection.Open();
        }
      }
    }

    public void Close()
    {
      lock (_lock)
      {
        if (_connection != null)
        {
          _connection.Clone();
          _connection = null;
        }
      }
    }

    public int CreateRequest(Request request)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("INSERT INTO request (action, olddn, newdn, parameter, requested) VALUES (@action, @olddn, @newdn, @parameter, @requested)", _connection);
        command.Parameters.Add(new PgSqlParameter("@action", (int)request.Action));
        command.Parameters.Add(new PgSqlParameter("@olddn", request.OldDn));
        command.Parameters.Add(new PgSqlParameter("@newdn", request.NewDn));
        command.Parameters.Add(new PgSqlParameter("@parameter", request.Parameter));
        command.Parameters.Add(new PgSqlParameter("@requested", request.Requested));
        command.ExecuteNonQuery();

        return GetIdentity();
      }
    }

    public void DeleteRequest(int id)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("DELETE FROM request WHERE id = @id", _connection);
        command.Parameters.Add(new PgSqlParameter("@id", id));
        command.ExecuteNonQuery();
      }
    }

    public Request GetRequest(int id)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("SELECT action, oldDn, newDn, parameter, requested FROM request WHERE id = @id", _connection);
        command.Parameters.Add(new PgSqlParameter("@id", id));

        using (var reader = command.ExecuteReader())
        {
          if (reader.Read())
          {
            var action = (RequestAction)reader.GetInt32(0);
            var oldDn = reader.GetString(1);
            var newDn = reader.GetString(2);
            var parameter = reader.GetString(3);
            var requested = reader.GetDateTime(4);
            return new Request(id, action, oldDn, newDn, parameter, requested);
          }
          else
          {
            return null;
          }
        }
      }
    }

    public IEnumerable<Request> GetRequests(string dn)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("SELECT id, action, oldDn, newDn, parameter, requested FROM request WHERE olddn = @olddn", _connection);
        command.Parameters.Add(new PgSqlParameter("@olddn", dn));

        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            var id = reader.GetInt32(0);
            var action = (RequestAction)reader.GetInt32(1);
            var oldDn = reader.GetString(2);
            var newDn = reader.GetString(3);
            var parameter = reader.GetString(4);
            var requested = reader.GetDateTime(5);
            yield return new Request(id, action, oldDn, newDn, parameter, requested);
          }
        }
      }
    }

    public IEnumerable<Request> GetallRequests()
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("SELECT id, action, oldDn, newDn, parameter, requested FROM request", _connection);

        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            var id = reader.GetInt32(0);
            var action = (RequestAction)reader.GetInt32(1);
            var oldDn = reader.GetString(2);
            var newDn = reader.GetString(3);
            var parameter = reader.GetString(4);
            var requested = reader.GetDateTime(5);
            yield return new Request(id, action, oldDn, newDn, parameter, requested);
          }
        }
      }
    }

    public void SetText(TextObject text)
    {
      if (GetText(text.Id) == null)
      {
        lock (_lock)
        {
          var command = new PgSqlCommand("INSERT INTO text (id, text) VALUES (@id, @text)", _connection);
          command.Parameters.Add(new PgSqlParameter("@id", text.Id));
          command.Parameters.Add(new PgSqlParameter("@text", text.Text));
          command.ExecuteNonQuery();
        }
      }
      else
      {
        var command = new PgSqlCommand("UPDATE text SET text = @text WHERE id = @id", _connection);
        command.Parameters.Add(new PgSqlParameter("@id", text.Id));
        command.Parameters.Add(new PgSqlParameter("@text", text.Text));
        command.ExecuteNonQuery();
      }
    }

    public TextObject GetText(string id)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("SELECT text FROM text WHERE id = @id", _connection);
        command.Parameters.Add(new PgSqlParameter("@id", id));

        using (var reader = command.ExecuteReader())
        {
          if (reader.Read())
          {
            var text = reader.GetString(0);
            return new TextObject(id, text);
          }
          else
          {
            return null;
          }
        }
      }
    }

    public IEnumerable<TextObject> GetAllTexts()
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("SELECT id, text FROM text", _connection);

        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            var id = reader.GetString(0);
            var text = reader.GetString(1);
            yield return new TextObject(id, text);
          }
        }
      }
    }

    public void DeleteText(string id)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("DELETE FROM text WHERE id = @id", _connection);
        command.Parameters.Add(new PgSqlParameter("@id", id));
        command.ExecuteNonQuery();
      }
    }

    private int GetIdentity()
    {
      var command = new PgSqlCommand("SELECT lastval();", _connection);
      var data = command.ExecuteScalar();
      return (int)(long)data;
    }

    public void SaveSearchTemplate(SearchTemplate template)
    {
      lock (_lock)
      {
        if (ListSearchTemplates().Any(t => t.Name == template.Name))
        {
          var command = new PgSqlCommand("UPDATE searchtemplate SET definition = @definition WHERE @name = name", _connection);
          command.Parameters.Add(new PgSqlParameter("@name", template.Name));
          command.Parameters.Add(new PgSqlParameter("@definition", template.Definition));
          command.ExecuteNonQuery();
        }
        else
        {
          var command = new PgSqlCommand("INSERT INTO searchtemplate (name, definition) VALUES (@name, @definition)", _connection);
          command.Parameters.Add(new PgSqlParameter("@name", template.Name));
          command.Parameters.Add(new PgSqlParameter("@definition", template.Definition));
          command.ExecuteNonQuery();
        }
      }
    }

    public IEnumerable<SearchTemplate> ListSearchTemplates()
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("SELECT name, definition FROM searchtemplate", _connection);

        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            var name = reader.GetString(0);
            var definition = reader.GetString(1);
            yield return new SearchTemplate(name, definition);
          }
        }
      }
    }

    public void DeleteSearchTemplate(string name)
    {
      lock (_lock)
      {
        var command = new PgSqlCommand("DELETE FROM searchtemplate WHERE name = @name", _connection);
        command.Parameters.Add(new PgSqlParameter("@name", name));
        command.ExecuteNonQuery();
      }
    }
  }
}