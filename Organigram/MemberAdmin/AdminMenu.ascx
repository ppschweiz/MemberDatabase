<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AdminMenu.ascx.cs" Inherits="MemberAdmin.AdminMenu" %>
<style type="text/css">
  .style1
  {
    width: 100%;
  }
</style>
<table class="style1">
  <tr>
    <td>
      <asp:LinkButton ID="searchButton" runat="server" OnClick="searchButton_Click">Search</asp:LinkButton>
    &nbsp;<asp:LinkButton ID="createButton" runat="server" 
        OnClick="createButton_Click">Create</asp:LinkButton>
    &nbsp;<asp:LinkButton ID="mailButton" runat="server" OnClick="mailButton_Click">Mailer</asp:LinkButton>
    &nbsp;<asp:LinkButton ID="groupsButton" runat="server" 
        OnClick="groupsButton_Click">Groups</asp:LinkButton>
    &nbsp;<asp:LinkButton ID="anomaliesButton" runat="server" 
        OnClick="anomaliesButton_Click">Anomalies</asp:LinkButton>
    &nbsp;&nbsp;<asp:LinkButton ID="specialButton" runat="server" 
        OnClick="specialButton_Click">Special</asp:LinkButton>
    &nbsp;<asp:LinkButton ID="logoffButton" runat="server" OnClick="logoffButton_Click">Logoff</asp:LinkButton>
    </td>
  </tr>
  <tr>
    <td>
      &nbsp;
    </td>
  </tr>
</table>
