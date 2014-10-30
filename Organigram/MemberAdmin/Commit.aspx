<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Commit.aspx.cs" Inherits="MemberAdmin.Commit" %>

<%@ Register src="AdminMenu.ascx" tagname="AdminMenu" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>MemberAdmin</title>
    <link rel="stylesheet" type="text/css" href="Style.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div id="master">
    
      <asp:Panel ID="panel" runat="server">
        <uc1:AdminMenu ID="AdminMenu1" runat="server" />
      </asp:Panel>
    
    </div>
    </form>
</body>
</html>
