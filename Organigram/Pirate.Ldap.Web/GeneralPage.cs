using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Util;
using Pirate.Util.Logging;

namespace Pirate.Ldap.Web
{
  public abstract class GeneralPage : System.Web.UI.Page
  {
    protected const string NewLine = "<br>";

    protected abstract void Redirect(string target);

    protected abstract string PageName { get; }

    protected abstract string HomePage { get; }

    protected abstract string LoginPage { get; }

    /// <summary>
    /// Redirects to the same URL self.
    /// </summary>
    protected void RedirectSelf()
    {
      Redirect(Request.RawUrl);
    }

    /// <summary>
    /// Redirects to the unauthorized page.
    /// </summary>
    protected void RedirectAccessDenied()
    {
      Redirect("AccessDenied.aspx");
    }

    /// <summary>
    /// Redirects to a error page displaying the specified LDAP error code.
    /// </summary>
    /// <param name="resultCode">The result code.</param>
    protected void RedirectLdapError(int resultCode)
    {
      Redirect("Error.aspx?result=" + resultCode.ToString());
    }

    public void HandleError(Exception exception, HttpRequest request, string action)
    {
      var username = Ldap.GetUserDn(Application, Session, Request);
      BasicGlobal.LogAndReportError(
        exception, 
        request,
        new Info("Source", PageName),
        new Info("Action", action),
        new Info("User", username)); 
    }

    private void HandleLdapError(LdapException exception, string action)
    {
      var username = Ldap.GetUserDn(Application, Session, Request);
      var ldapResult = (LdapResultCode)exception.ResultCode;

      switch (ldapResult)
      {
        case LdapResultCode.InsufficientAccessRights:
          BasicGlobal.LogAndReportWarning(
            exception,
            Request,
            new Info("Source", PageName),
            new Info("Action", action),
            new Info("User", username),
            new Info("LdapResult", ldapResult));
          RedirectAccessDenied();
          break;
        default:
          BasicGlobal.LogAndReportError(
            exception,
            Request,
            new Info("Source", PageName),
            new Info("Action", action),
            new Info("User", username),
            new Info("LdapResult", ldapResult));
          RedirectLdapError(exception.ResultCode);
          break;
      }
    }

    protected bool ExecuteHandlingError(string text, Action action)
    {
      try
      {
        action();
        return true;
      }
      catch (LdapException exception)
      {
        HandleLdapError(exception, text);
        return false;
      }
      catch (Exception exception)
      {
        HandleError(exception, Request, text);
        return false;
      }
    }

    protected TResult ExecuteHandlingError<TResult>(string text, Func<TResult> action)
    {
      try
      {
        return action();
      }
      catch (LdapException exception)
      {
        HandleLdapError(exception, text);
        return default(TResult);
      }
      catch (Exception exception)
      {
        HandleError(exception, Request, text);
        return default(TResult);
      }
    }

    /// <summary>
    /// Creates a HTML multiline text for some lines.
    /// </summary>
    /// <param name="lines">The lines.</param>
    /// <returns></returns>
    protected string Multiline(IEnumerable<string> lines)
    {
      return Multiline(lines.ToArray());
    }

    /// <summary>
    /// Creates a HTML multiline text for some lines.
    /// </summary>
    /// <param name="lines">The lines.</param>
    /// <returns></returns>
    protected string Multiline(params string[] lines)
    {
      return string.Join(NewLine, lines);
    }

    /// <summary>
    /// Converts the text to label.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns></returns>
    protected Label ToLabel(string text, bool bold = false)
    {
      var label = new Label();
      label.Text = text;
      label.Font.Bold = bold;
      return label;
    }

    protected HyperLink ToLink(string text, string url)
    {
      var link = new HyperLink();
      link.Text = text;
      link.NavigateUrl = url;
      return link;
    }

    /// <summary>
    /// Combines the specified controls.
    /// </summary>
    /// <param name="controls">The controls.</param>
    /// <returns></returns>
    protected Panel Combine(params Control[] controls)
    {
      var panel = new Panel();

      foreach (var control in controls)
      {
        panel.Controls.Add(control);
      }

      return panel;
    }

    protected string FixEmpty(string input)
    {
      return string.IsNullOrEmpty(input) ? "!N/A!" : input;
    }

    /// <summary>
    /// Redirects to the home page.
    /// </summary>
    protected virtual void RedirectHome()
    {
      Redirect(HomePage + ".aspx");
    }

    public void RedirectLogin()
    {
      var parameters = new List<string>();
      parameters.Add("page=" + PageName);

      foreach (var key in Request.QueryString.AllKeys)
      {
          parameters.Add(key + "=" + Request.Params[key]);
      }

      Redirect(LoginPage + ".aspx?" + string.Join("&", parameters.ToArray()));
    }

    public void RedirectFromLogin()
    {
      var page = Request.Params["page"] ?? HomePage;
      var parameters = new List<string>();

      foreach (var key in Request.QueryString.AllKeys)
      {
        if (key != "page")
        {
          parameters.Add(key + "=" + Request.Params[key]);
        }
      }

      if (parameters.Count > 0)
      {
        Redirect(page + ".aspx?" + string.Join("&", parameters.ToArray()));
      }
      else
      {
        Redirect(page + ".aspx");
      }
    }

    protected Panel CreateHyperlinkButton(string url, string text, string tooltip = null, bool doubleMode = false)
    {
      var link = new HyperLink();
      link.Text = text;
      link.NavigateUrl = url;
      link.CssClass = doubleMode ? "ui-button-link-double" : "ui-button-link";
      var panel = new Panel();
      panel.CssClass = doubleMode ? "ui-button-double" : "ui-button";
      panel.ToolTip = tooltip;
      panel.Controls.Add(link);
      return panel;
    }

    protected Panel CreateEventButton(EventHandler handler, string text, string tooltip = null, bool doubleMode = false)
    {
      var link = new LinkButton();
      link.Text = text;
      link.Click += handler;
      link.CssClass = doubleMode ? "ui-button-link-double" : "ui-button-link";
      var panel = new Panel();
      panel.CssClass = doubleMode ? "ui-button-double" : "ui-button";
      panel.ToolTip = tooltip;
      panel.Controls.Add(link);
      return panel;
    }
  }
}
