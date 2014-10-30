<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NoCertificate.aspx.cs" Inherits="MemberAdmin.NoCertificate" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>MemberAdmin</title>
    <link rel="stylesheet" type="text/css" href="Style.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
      <asp:Label ID="notice" runat="server" Font-Bold="True" Font-Size="2em" 
        Text="Kein Zugriff / No Access"></asp:Label>
      <br />
    
      <asp:Label ID="notice0" runat="server" Font-Bold="True" Font-Size="1.2em" 
        
        Text="Du hast kein Clientzertifikat. Du kannst unter MemberService eines erstellen."></asp:Label>
      <br />
    
      <asp:Label ID="notice1" runat="server" Font-Bold="True" Font-Size="1.2em" 
        
        Text="You do not have a client certificate. You may create one on the MemberService."></asp:Label>
      <br />
      <br />
      <asp:HyperLink ID="loginLink" runat="server" 
        NavigateUrl="" Font-Size="1.2em">MemberService</asp:HyperLink>
    
    </div>
    </form>
</body>
</html>
