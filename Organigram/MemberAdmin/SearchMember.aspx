<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SearchMember.aspx.cs" Inherits="MemberAdmin.SearchMember" %>

<%@ Register src="AdminMenu.ascx" tagname="AdminMenu" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>MemberAdmin</title>
    <link rel="stylesheet" type="text/css" href="Style.css" />
    <link rel="stylesheet" type="text/css" href="css/jquery-ui-1.8.13.custom.css" />
    <script type="text/javascript" src="js/jquery-1.6.1.min.js"></script>
    <script type="text/javascript" src="js/jquery-ui-1.8.13.custom.min.js"></script>
    <script type="text/javascript" src="js/ui.dropdownchecklist-1.4-min.js"></script>
    <script type="text/javascript">
      $(document).ready(function () {
        $("#sdn").dropdownchecklist({ width: 500, maxDropHeight: 300, firstItemChecksAll: true });
        $("#ml1").dropdownchecklist({ width: 500, maxDropHeight: 300, firstItemChecksAll: true });
        $("#ml2").dropdownchecklist({ width: 500, maxDropHeight: 300, firstItemChecksAll: true });
        $("#ml3").dropdownchecklist({ width: 500, maxDropHeight: 300, firstItemChecksAll: true });
        $("#ml4").dropdownchecklist({ width: 500, maxDropHeight: 300, firstItemChecksAll: true });
        $("#ml5").dropdownchecklist({ width: 500, maxDropHeight: 300, firstItemChecksAll: true });
        $("#atr").dropdownchecklist({ width: 500, maxDropHeight: 300, firstItemChecksAll: true });
        $("#ord").dropdownchecklist({ width: 500, maxDropHeight: 300 });
      });
      </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
      <asp:Panel ID="panel" runat="server">
        <uc1:AdminMenu ID="AdminMenu1" runat="server" />
      </asp:Panel>
    
    </div>
    </form>
</body>
</html>
