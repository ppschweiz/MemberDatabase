<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Unauthorized.aspx.cs" Inherits="MemberAdmin.Unauthorized" %>

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
        Text="Unauthorized Access"></asp:Label>
      <br />
      <br />
      <asp:HyperLink ID="loginLink" runat="server" NavigateUrl="~/Login.aspx">Go to Login</asp:HyperLink>
    
    </div>
    </form>
</body>
</html>
