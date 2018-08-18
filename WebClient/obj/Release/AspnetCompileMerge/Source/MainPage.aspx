<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MainPage.aspx.cs" Inherits="FtpViewer.MainPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>FTP Viewer</title>
    <link rel="stylesheet" type="text/css" href="style.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="section">
            <div class="form-wrapper">
                <h4>Credentials</h4>
                <div class="form">
                    <div>
                        <table>
                            <tbody>
                                <tr class="form-text-input-tr">
                                    <td class="label-td">
                                        <label>Ftp:</label>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="tbFtpHost" runat="server" TextMode="Url" Text="ftp://"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr class="form-text-input-tr">
                                    <td class="label-td">
                                        <label>User:</label>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="tbUser" runat="server" Text=""></asp:TextBox>
                                    </td>
                                </tr>

                                <tr class="form-text-input-tr">
                                    <td class="label-td">
                                        <label>Password:</label>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="tbPassword" TextMode="Password" runat="server" Text=""></asp:TextBox>
                                    </td>
                                </tr>
                                <tr class="form-submit-tr">
                                    <td>&nbsp;</td>
                                    <td>
                                        <asp:Button CssClass="button" ID="btnLoadFtp" runat="server" Text="Load Files" OnClick="btnLoadFtp_Click" />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
        <div class="section">
            <div class="listData-wrapper">
                <h4>Files</h4>
                <div>
                    <asp:GridView ID="ftpFiles" runat="server" AutoGenerateColumns="false" CssClass="Grid" RowStyle-HorizontalAlign="Center">
                        <Columns>
                            <asp:TemplateField ItemStyle-Width="24">
                                <ItemTemplate>
                                    <%# Container.DataItemIndex + 1 %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Name" HeaderText="Name" ItemStyle-CssClass="nameClass" />
                            <asp:BoundField DataField="Size" HeaderText="Size (KB)" DataFormatString="{0:N2}" ItemStyle-Width="200" />
                            <asp:BoundField DataField="Date" HeaderText="Created Date" ItemStyle-Width="180" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnDownload" CssClass="button" runat="server" Text="Download" OnClick="DownloadFile" CommandArgument='<%# Eval("Name") %>'></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
