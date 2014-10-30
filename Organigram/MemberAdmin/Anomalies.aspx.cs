using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Novell.Directory.Ldap;
using System.IO;
using PA = Pirate.Ldap.Attributes.Person;

namespace MemberAdmin
{
  public partial class Anomalies : CustomPage
  {
    protected override string PageName
    {
      get { return "Anomalies"; }
    }

    public class AnomalyType
    {
      public string Reason { get; private set; }

      public Func<Person, bool> Check { get; private set; }

      public AnomalyType(Func<Person, bool> check, string reason)
      {
        Check = check;
        Reason = reason;
      }
    }

    private Dictionary<object, string> _buttonIdMap;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      _buttonIdMap = new Dictionary<object, string>();
      var table = new Table();

      var anomalyTypes = new List<AnomalyType>();
      anomalyTypes.Add(
        new AnomalyType(
          p => p.Name.IsNullOrEmpty() && p.Surname.IsNotNullOrEmpty(),
          "Name (cn) fehlt."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.IsMemberType && !p.EmployeeNumber.HasValue,
          "Mitgliedsnummer (employeeNumber) fehlt."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.IsNonMemberType && p.IsMemberDn,
          "Status Landratte/Ausgetreten/Ausgeschlossen, aber bei Mitglieder eingeordnet.")); 
      anomalyTypes.Add(
        new AnomalyType(
          p => p.IsMemberType && !p.IsMemberDn,
          "Status Pirate/Sympathisant, aber bei Nichtmitglieder eingeordnet."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.IsMemberType && p.Joining.Count != p.Leaving.Count + 1,
          "Status Pirate/Sympathisant, aber kein Beitrittsdatum."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.EmployeeType == EmployeeType.LandLubber && p.Joining.Count > 0,
          "Status Landratte, hat aber ein Beitrittsdatum."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.EmployeeType == EmployeeType.LandLubber && p.Leaving.Count > 0,
          "Status Landratte, hat aber ein Austrittsdatum."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.IsFormerMemberType && p.Leaving.Count < 1,
          "Status Ausgetreten/Ausgeschlossen, hat aber kein Austrittsdatum."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.IsFormerMemberType && p.Joining.Count != p.Leaving.Count,
          "Status Ausgetreten/Ausgeschlossen, hat aber kein Austrittsdatum."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.IsMemberType  && (p.PreferredNotificationMethod == PreferredNotificationMethod.Letter && p.PostalCode.IsNullOrEmpty()),
          "Wünscht Brief, hat aber keine PLZ."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.IsMemberType  && (p.PreferredNotificationMethod == PreferredNotificationMethod.Letter && p.Location.IsNullOrEmpty()),
          "Wünscht Brief, hat aber keinen Ort."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.IsMemberType  && (p.PreferredNotificationMethod == PreferredNotificationMethod.Email && p.Emails.Count < 1),
          "Wünscht Email, hat aber keine Emailadresse."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.EmployeeType == EmployeeType.Pirate && p.VotingRightUntil < DateTime.Now,
          "Ist als Pirat markiert, hat aber aktuell kein Stimmrecht."));
      anomalyTypes.Add(
        new AnomalyType(
          p => p.EmployeeType != EmployeeType.Pirate && p.VotingRightUntil >= DateTime.Now,
          "Ist nicht als Pirat markiert, hat aber aktuell Stimmrecht."));

      table.AddHeaderRow(LdapResources.UniqueIdentifier, LdapResources.Surname, LdapResources.Givenname, Resources.Username, Resources.Reason);

      var persons = Connection.Search<Person>(Names.Party, LdapScope.Sub).ToList();

      foreach (var anomaly in anomalyTypes)
      {
        foreach (var person in persons)
        {
          if (anomaly.Check(person))
          {
            var controls = new List<Control>();

            var idLabel = new Label();
            idLabel.Text = person.Id.ToString();
            controls.Add(idLabel);

            var surnameLabel = new Label();
            surnameLabel.Text = person.Surname;
            controls.Add(surnameLabel);

            var givennameLabel = new Label();
            givennameLabel.Text = person.Givenname;
            controls.Add(givennameLabel);

            var nickLabel = new Label();
            nickLabel.Text = person.Nickname;
            controls.Add(nickLabel);

            var reasonLabel = new Label();
            reasonLabel.Text = anomaly.Reason;
            controls.Add(reasonLabel);

            var editButton = new LinkButton();
            editButton.Text = "Edit";
            editButton.Click += new EventHandler(EditButton_Click);
            _buttonIdMap.Add(editButton, person.Id.ToString());
            controls.Add(editButton);

            table.AddRow(controls.ToArray());
          }
        }
      }

      this.panel.Controls.Add(table);
    }

    private void EditButton_Click(object sender, EventArgs e)
    {
      var id = _buttonIdMap[sender];
      Redirect("DisplayMember.aspx?member=" + id);
    }
  }
}