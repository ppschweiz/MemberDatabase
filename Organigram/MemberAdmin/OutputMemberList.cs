using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Pirate.Ldap;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberAdmin
{
  /// <summary>
  /// Data output handler for the member list in CSV format.
  /// </summary>
  public class OutputMemberList : IHttpHandler, IRequiresSessionState
  {
    private Dictionary<object, string> _buttonIdMap;

    private List<LdapAttributeBase> _attributes;

    public void ProcessRequest(HttpContext context)
    {
      var parameters = context.Session[ListMember.SearchParameters] as SearchParameters;

      var text = new StringBuilder();
      var access = Ldap.Connect(context.Application, context.Session, context.Request);

      if (access != null)
      {
        var currentUser = access.Connection.SearchFirst<Person>(Ldap.GetUserDn(context.Application, context.Session, context.Request), LdapScope.Base);
        var builder = new StringBuilder();

        try
        {
          builder.AppendLine("Export by user id {0} name {1}", currentUser.Id, currentUser.Name);

          foreach (var searchDn in parameters.SearchDns)
          {
            builder.AppendLine("Search DN: {0}", searchDn);
          }

          builder.AppendLine("Filter: {0}", parameters.Filter == null ? "N/A" : parameters.Filter.Text);
          builder.AppendLine("Attributes: {0}", string.Join(", ", parameters.Attributes.Select(a => (string)a).ToArray()));

          foreach (var postFilter in parameters.PostFilters)
          {
            builder.AppendLine("Postfilter: {0}", postFilter.Description);
          }
          
          var persons =
            parameters.SearchDns.SelectMany(dn =>
              access.Connection.Search<Person>(dn, LdapScope.Sub, parameters.Filter))
            .Where(p => parameters.PostFilters.All(f => f.Function(p)));

          _buttonIdMap = new Dictionary<object, string>();
          _attributes = new List<LdapAttributeBase>();
          var binDir = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
          var headings = new List<string>();

          foreach (var attribute in parameters.Attributes)
          {
            headings.Add(attribute.Text());
          }

          text.AppendLine(string.Join(";", headings.Select(h => "\"" + h + "\"").ToArray()));

          foreach (var person in persons.OrderBy(parameters.OrderBySelector))
          {
            var texts = new List<string>();

            foreach (var attribute in parameters.Attributes)
            {
              var field = person.GetField(attribute);

              if (field.NeedsEvaluation)
              {
                texts.Add(field.EvaluateString(access.Connection));
              }
              else
              {
                texts.Add(field.ValueString(MultiValueOutput.FirstOnly));
              }
            }

            text.AppendLine(string.Join(";", texts.Select(t => "\"" + t + "\"").ToArray()));
          }

          builder.AppendLine("Results: " + persons.Count());
        }
        catch (Exception exception)
        {
          builder.AppendLine("Error: " + exception.ToString());
          BasicGlobal.Mailer.SendToAdmin("Error", builder.ToString());
        }
        finally
        {
          BasicGlobal.Logger.Log(LogLevel.Info, builder.ToString());
        }
      }
      else
      {
        text.AppendLine("Unauthorized Access");
      }

      context.Response.Clear();
      context.Response.ContentEncoding = Encoding.UTF8;
      context.Response.ContentType = "text/csv";
      context.Response.Write(text.ToString());
      context.Response.Flush();
    }

    public bool IsReusable
    {
      get { return false; }
    }
  }
}
