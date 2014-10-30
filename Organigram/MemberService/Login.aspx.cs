using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberService
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
      var table = new Table();

      table.AddRow("<img width=\"200px\" src=\"./Images/ppslogo.png\"/>", 2);
      table.AddRow("Member Service Login", 2);

      _usernameTextBox = new TextBox();
      table.AddRow(Resources.Username, _usernameTextBox);

      _passwordTextBox = new TextBox();
      _passwordTextBox.TextMode = TextBoxMode.Password;
      table.AddRow(Resources.Password, _passwordTextBox);

      _loginButton = new Button();
      _loginButton.Text = Resources.Login;
      _loginButton.Click += new EventHandler(LoginButton_Click);
      table.AddRow(string.Empty, _loginButton);

      table.AddVerticalSpace(20);
      table.AddRow(string.Empty, ToLabel("<a href=\"" + BasicGlobal.Config.MemberAdminAddress + "\">MemberAdmin</a>"));

      this.panel.Controls.Add(table);
    }

    private void LoginButton_Click(object sender, EventArgs e)
    {
      if (Ldap.Login(Application, Session, Request, _usernameTextBox.Text, _passwordTextBox.Text))
      {
        RedirectFromLogin();
      }
    }
  }
}