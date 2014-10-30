using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Util;

namespace RegistrationService
{
  public partial class DisplayMessage : CustomPage
  {
    protected override string PageName
    {
      get { return "DisplayMessage"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      var table = new Table();

      var ciphertext = (Request.Params["data"] ?? string.Empty).TryParseHexBytes();
      var plaintext = ciphertext == null ? null : BasicGlobal.Config.DeSecure(ciphertext);
      var message = plaintext == null ? null : Message.FromBinary(plaintext);

      if (message != null)
      {
        if (string.IsNullOrEmpty(message.TextId))
        {
          table.AddRow(message.Text);
        }
        else
        {
          var user = Global.LdapAccess.Connection.SearchFirst<Person>(message.UserDn);
          var preText = Texts.GetText(DataGlobal.DataAccess, Texts.GetItem(message.TextId), user);
          var text = Pirate.Textile.TextParser.Parse(preText.Item2);
          table.AddRow(text.ToHtml());
        }

        table.AddVerticalSpace(20);

        var link = new HyperLink();
        link.Text = Resources.Ok;
        link.NavigateUrl = message.ContinueTo;
        table.AddRow(link);
      }
      else
      {
        RedirectHome();
      }

      this.panel.Controls.Add(table);
    }
  }
}