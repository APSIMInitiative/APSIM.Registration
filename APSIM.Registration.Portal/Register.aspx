<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="ProductRegistration.Registration" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>APSIM Initiative Product Registration</title>
    <link rel="stylesheet" media="screen" href="Screen_Styles.css" type="text/css" />
    <script type="text/javascript" src="https://code.jquery.com/jquery-3.5.0.min.js""></script>
    <script type="text/javascript" src="Register.js"></script>
</head>
<body>
   <h1>
      APSIM Initiative Product Registration
   </h1>
   To download software you must complete the registration form below. All fields are mandatory.
    <form id="form1" runat="server">
      <asp:Label ID="ErrorLabel" runat="server" Font-Bold="True" ForeColor="#FF3300" 
         Visible="False"></asp:Label>
      <asp:Table 
         ID="Table1" runat="server"
         EnableTheming="True">
         <asp:TableRow ID="TableRow1" runat="server">
            <asp:TableCell ID="TableCell1" runat="server">Product to download:</asp:TableCell>
            <asp:TableCell ID="TableCell2" runat="server"><asp:DropDownList ID="Product" runat="server" Width="200px"/></asp:TableCell>
         </asp:TableRow>
         <asp:TableRow runat="server" ID="versionRow">
            <asp:TableCell ID="VersionLabel" Text="Version:" runat="server"> </asp:TableCell>
            <asp:TableCell runat="server"><asp:DropDownList ID="Version" runat="server" Width="200px"></asp:DropDownList></asp:TableCell>
         </asp:TableRow>
         <asp:TableRow runat="server">
            <asp:TableCell runat="server"> </asp:TableCell>
            <asp:TableCell runat="server"> </asp:TableCell>
         </asp:TableRow>
         <asp:TableRow runat="server">
            <asp:TableCell runat="server">First name:</asp:TableCell>
            <asp:TableCell runat="server"><asp:TextBox ID="FirstName" runat="server" Width="400px"></asp:TextBox></asp:TableCell>
         </asp:TableRow>
         <asp:TableRow runat="server">
            <asp:TableCell runat="server">Last name:</asp:TableCell>
            <asp:TableCell runat="server"><asp:TextBox ID="LastName" runat="server" Width="400px"></asp:TextBox></asp:TableCell>
         </asp:TableRow>
         <asp:TableRow runat="server">
            <asp:TableCell runat="server">Organisation:</asp:TableCell>
            <asp:TableCell runat="server"><asp:TextBox ID="Organisation" runat="server" Width="400px"></asp:TextBox></asp:TableCell>
         </asp:TableRow>
         <asp:TableRow runat="server">
            <asp:TableCell runat="server">Country:</asp:TableCell>
            <asp:TableCell runat="server">
                <asp:DropDownList ID="Country" runat="server" Width="200px">
                    <asp:ListItem Value=""></asp:ListItem>
               </asp:DropDownList>
            </asp:TableCell>
         </asp:TableRow>
         <asp:TableRow runat="server">
            <asp:TableCell runat="server">Email:</asp:TableCell>
            <asp:TableCell runat="server"><asp:TextBox ID="Email" runat="server" Width="400px"></asp:TextBox></asp:TableCell>
         </asp:TableRow>
         <asp:TableRow runat="server">
             <asp:TableCell runat="server">Licence type:</asp:TableCell>
             <asp:TableCell runat="server" Width="400px">
                 <asp:RadioButton id="radioNonCom" Text="Non-Commercial" GroupName="LicenseType" Checked="true" autocomplete="off" runat="server"></asp:RadioButton>
                 <asp:RadioButton id="radioCom" Text="Commercial" GroupName="LicenseType" autocomplete="off" runat="server"></asp:RadioButton>
             </asp:TableCell>
         </asp:TableRow>
         <asp:TableRow class="commercialInput" runat="server">
             <asp:TableCell>Licensor name:</asp:TableCell>
             <asp:TableCell><asp:TextBox ID="LicensorName" runat="server" Width="400px" /></asp:TableCell>
         </asp:TableRow>
         <asp:TableRow class="commercialInput" runat="server">
             <asp:TableCell>Licensor email:</asp:TableCell>
             <asp:TableCell><asp:TextBox ID="LicensorEmail" runat="server" Width="400px" /></asp:TableCell>
         </asp:TableRow>
         <asp:TableRow class="commercialInput" runat="server">
             <asp:TableCell>Company Sales/Turnover:</asp:TableCell>
             <asp:TableCell runat="server">
                 <asp:RadioButton GroupName="Turnover" ID="radioLessThan2Mil" Text="<$2 million" runat="server" />
                 <asp:RadioButton GroupName="Turnover" ID="radio2ToFortyMil" Text="$2-40 million" runat="server" />
                 <asp:RadioButton GroupName="Turnover" ID="radioBigBucks" Text=">$40 million" runat="server" />
             </asp:TableCell>
         </asp:TableRow>
          <asp:TableRow CssClass="commercialInput" runat="server">
              <asp:TableCell>Company Registration Number:</asp:TableCell>
              <asp:TableCell>
                  <asp:TextBox ID="companyID" Width="400px" runat="server"></asp:TextBox>
              </asp:TableCell>
          </asp:TableRow>
          <asp:TableRow CssClass="commercialInput" runat="server">
              <asp:TableCell runat="server">Street Address:</asp:TableCell>
              <asp:TableCell runat="server">
                  <asp:TextBox ID="companyAddress" TextMode="MultiLine" Rows="2" Width="400" runat="server"></asp:TextBox>
              </asp:TableCell>
          </asp:TableRow>
      </asp:Table>
      <br />
      <asp:CheckBox ID="ChkSubscribe" runat="server" Text ="The APSIM Initiative will forward updates on APSIM-related developments and improvements as well as the upcoming APSIM-related events including training workshops." />
      <br />
      <br />
      <div>
          <asp:Button ID="YesButton" runat="server" Text="Yes I agree, begin download" onclick="YesButton_Click" Width="238px" />
          <asp:Button ID="NoButton" runat="server" Text="No I don't agree, go back" onclick="NoButton_Click" />
      </div>
      <br />
      <asp:Label ID="CompleteLabel" runat="server" Text="            " Font-Bold="True" 
         ForeColor="#0066FF"></asp:Label>
      </form>
      <asp:Label runat="server">Terms:</asp:Label>
      <asp:Table 
         ID="Table2" runat="server" Height="100px" Width="836px"
         EnableTheming="True">
         <asp:TableRow ID="TableRow2" runat="server">
            <asp:TableCell ID="Terms" runat="server"></asp:TableCell>
         </asp:TableRow>
      </asp:Table>      
</body>
</html>

