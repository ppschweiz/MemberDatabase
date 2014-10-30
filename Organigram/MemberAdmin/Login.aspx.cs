using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Web;
using Pirate.Openssl;
using Pirate.Util.Logging;

namespace MemberAdmin
{
  public partial class Login : CustomPage
  {
    protected override string PageName
    {
      get { return "Login"; }
    }
    
    private TextBox _usernameTextBox;

    private TextBox _passwordTextBox;

    private Button _loginButton;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (ClientCertificate == null)
      {
        Redirect("NoCertificate.aspx");
        return;
      } 
      
      var table = new Table();

      table.AddRow("<img width=\"200px\" src=\"./Images/ppslogo.png\"/>", 2);
      table.AddRow("Member Admin Login", 2);

      _usernameTextBox = new TextBox();
      table.AddRow(Resources.Username, _usernameTextBox);

      _passwordTextBox = new TextBox();
      _passwordTextBox.TextMode = TextBoxMode.Password;
      table.AddRow(Resources.Password, _passwordTextBox);

      _loginButton = new Button();
      _loginButton.Text = Resources.Login;
      _loginButton.Click += new EventHandler(LoginButton_Click);
      table.AddRow(string.Empty, _loginButton);

      this.panel.Controls.Add(table);
    }

    private void LoginButton_Click(object sender, EventArgs e)
    {
      if (ClientCertificate == null)
      {
        Redirect("NoCertificate.aspx");
        return;
      } 

      if (Ldap.Login(Application, Session, Request, _usernameTextBox.Text, _passwordTextBox.Text))
      {
        var access = Ldap.Connect(Application, Session, Request);
        var currentUser = access.Connection.SearchFirst<Person>(Ldap.GetUserDn(Application, Session, Request), LdapScope.Base);
        var certificate = ClientCertificate;

        if (certificate != null)
        {
          var authorizationLevel = DataGlobal.DataAccess.GetCertificateAuthorizationLevel(certificate.Thumbprint, currentUser.Id);

          if (authorizationLevel >= 1)
          {
            BasicGlobal.Logger.Log(LogLevel.Info, "User {0} authorized at level {1} from IP {2}", currentUser.DN, authorizationLevel, Request.UserHostAddress);
            RedirectFromLogin();
            return;
          }
          else
          {
            BasicGlobal.Logger.Log(LogLevel.Info, "User {0} has insufficient authoization level {1} from IP {2}", currentUser.DN, authorizationLevel, Request.UserHostAddress);
          }
        }

        Redirect("Unauthorized.aspx");
      }
    }
  }
}