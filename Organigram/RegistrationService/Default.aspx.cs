using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Web;

namespace RegistrationService
{
  public partial class Default : CustomPage
  {
    protected override string PageName
    {
      get { return "Default"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      var table = new Table();

      switch (Language.TwoLetterISOLanguageName.ToLowerInvariant())
      {
        case "de":
          table.AddRow("<img height=\"50px\" src=\"./Images/ppslogo.de.png\"/>", 2);
          break;
        case "fr":
          table.AddRow("<img height=\"50px\" src=\"./Images/ppslogo.fr.png\"/>", 2);
          break;
        default:
          table.AddRow("<img height=\"50px\" src=\"./Images/ppslogo.en.png\"/>", 2);
          break;
      }

      table.AddVerticalSpace(30);

      var languagePanel = new Panel();

      var languageGermanLink = new LinkButton();
      languageGermanLink.CssClass = "standard-margin";
      languageGermanLink.Text = "<img width=\"50px\" src=\"./Images/german.png\"/>";
      languageGermanLink.Click += new EventHandler(LanguageGermanLink_Click);
      languagePanel.Controls.Add(languageGermanLink);

      var languageFrenchLink = new LinkButton();
      languageFrenchLink.CssClass = "standard-margin";
      languageFrenchLink.Text = "<img width=\"50px\" src=\"./Images/french.png\"/>";
      languageFrenchLink.Click += new EventHandler(LanguageFrenchLink_Click);
      languagePanel.Controls.Add(languageFrenchLink);

      var languageEnglishLink = new LinkButton();
      languageEnglishLink.CssClass = "standard-margin";
      languageEnglishLink.Text = "<img width=\"50px\" src=\"./Images/english.png\"/>";
      languageEnglishLink.Click += new EventHandler(LanguageEnglishLink_Click);
      languagePanel.Controls.Add(languageEnglishLink);

      table.AddRow(languagePanel);

      table.AddRow(
        CreateHyperlinkButton(
        "BeginVerifyEmail.aspx?action=registermember",
        Resources.CreateAccountMember,
        Resources.CreateAccountMemberHint,
        true));

      table.AddRow(
        CreateHyperlinkButton(
        "BeginVerifyEmail.aspx?action=registerforum",
        Resources.CreateAccountForum,
        Resources.CreateAccountForumHint,
        true));

      table.AddRow(
        CreateHyperlinkButton(
        "BeginPasswordReset.aspx",
        Resources.PasswordReset,
        Resources.PasswordResetHint,
        true)); 

      table.AddRow(
        CreateHyperlinkButton(
        BasicGlobal.Config.MemberServiceAddress,
        Resources.MemberService,
        Resources.MemberServiceHint,
        true)); 

      this.panel.Controls.Add(table);
    }

    private void LanguageEnglishLink_Click(object sender, EventArgs e)
    {
      Language = CultureInfo.CreateSpecificCulture("en-US");
      RedirectSelf();
    }

    private void LanguageFrenchLink_Click(object sender, EventArgs e)
    {
      Language = CultureInfo.CreateSpecificCulture("fr-FR");
      RedirectSelf();
    }

    private void LanguageGermanLink_Click(object sender, EventArgs e)
    {
      Language = CultureInfo.CreateSpecificCulture("de-DE");
      RedirectSelf();
    }
  }
}