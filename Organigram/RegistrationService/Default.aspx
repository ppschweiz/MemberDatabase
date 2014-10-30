<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="RegistrationService.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>RegistrationService</title>
    <link rel="stylesheet" type="text/css" href="Style.css" />
    <link rel="stylesheet" href="css/smoothness/jquery-ui-1.10.3.custom.css" />
    <script type="text/javascript" src="js/jquery-1.9.1.js"></script>
    <script type="text/javascript" src="js/jquery-ui-1.10.3.custom.js"></script>
    <script type="text/javascript">
    $(function () {
      $(document).tooltip();
    });
  </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
      <asp:Panel ID="panel" runat="server">
      </asp:Panel>
    
    </div>
    </form>
</body>
</html>
