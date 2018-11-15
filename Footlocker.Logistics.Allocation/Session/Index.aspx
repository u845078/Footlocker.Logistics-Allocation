<%@ Page Trace="true" %>

<script runat="server">

    Protected Sub Unnamed_Click(sender As Object, e As EventArgs)
        Session("holder") = "set"
    End Sub
</script>

<html>
<body>
    <form runat="server">
        <asp:button runat="server" text="Click" />
        <asp:button runat="server" onclick="Unnamed_Click" Text="Start session" />
    </form>
</body>
</html>