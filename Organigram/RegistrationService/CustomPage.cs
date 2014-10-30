using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace RegistrationService
{
  /// <summary>
  /// Parent class for all custom pages.
  /// </summary>
  public abstract class CustomPage : GeneralPage
  {
    protected override void OnLoad(EventArgs e)
    {
      Language = Language;
      base.OnLoad(e);
    }

    protected CultureInfo Language
    {
      get
      {
        var data = Session["language"];

        if (data != null)
        {
          return data as CultureInfo;
        }
        else
        {
          return CultureInfo.CreateSpecificCulture("de-DE");
        }
      }
      set
      {
        Thread.CurrentThread.CurrentCulture = value;
        Resources.Culture = value;
        LdapResources.Culture = value;
        Session["language"] = value;
      }
    }

    /// <summary>
    /// Redirects to the home page.
    /// </summary>
    protected override void RedirectHome()
    {
      Redirect("Default.aspx");
    }

    /// <summary>
    /// Redirects the specified target.
    /// </summary>
    /// <param name="target">The target.</param>
    protected override void Redirect(string target)
    {
      Response.Redirect(target, false);
    }

    protected override string HomePage
    {
      get { return "Default"; }
    }

    protected override string LoginPage
    {
      get { throw new NotSupportedException(); }
    }
  }
}