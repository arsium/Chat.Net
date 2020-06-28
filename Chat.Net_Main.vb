Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading


''This a sample of modern application chat on computer !
''All code has been produced Arsium 



'MIT License

'Copyright(c) 2020 Arsium

'Permission Is hereby granted, free Of charge, to any person obtaining a copy of this software And associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell copies of the Software, And to permit persons to whom the Software Is furnished to do so, subject to the following conditions

'The above copyright notice And this permission notice shall be included In all copies Or substantial portions Of the Software. 

'THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY, FITNESS For A PARTICULAR PURPOSE And NONINFRINGEMENT. In NO Event SHALL THE AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM, OUT Of Or In CONNECTION With THE SOFTWARE Or THE USE Or OTHER DEALINGS In THE SOFTWARE.
Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim o As New Thread(AddressOf Anima)

        o.Start()
    End Sub
    Public Sub Anima()
        Guna2AnimateWindow1.SetAnimateWindow(Me)
    End Sub


    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
    End Sub

    Public CLIHelper As TcpClient
    Public GETIP() As IPAddress = Nothing
    Public H As IPAddress

    Public CLI_Data As Thread

    Private Sub Guna2Button1_Click(sender As Object, e As EventArgs) Handles Guna2Button1.Click


        If Guna2CheckBox1.Checked And Guna2CheckBox2.Checked Then

            MessageBox.Show("Choice between server and client !")
        ElseIf Guna2CheckBox1.Checked Then

            CLIHelper = New TcpClient()

            Try
                Dim j As String() = Split(Guna2TextBox1.Text, ":")
                Dim b As Boolean = IPAddress.TryParse(j(0), H)
                If b = False Then
                    GETIP = Dns.GetHostAddresses(j(0))
                    CLIHelper.Connect(GETIP(0).ToString, j(1))
                Else
                    CLIHelper.Connect(j(0), j(1))
                End If

                CLI_Data = New Thread(AddressOf GetData)
                CLI_Data.Start()

            Catch ex As Exception

            End Try



        End If
    End Sub
    Public Async Sub GetData()
        Dim getMes As New StringBuilder
        While True
            Dim B(4096) As Byte

            Dim RD As Integer = Await CLIHelper.GetStream.ReadAsync(B, 0, B.Length)

            Dim M As String = System.Text.Encoding.UTF8.GetString(B)
            If RD > 0 Then


                getMes.Append(M)
                If getMes.ToString.Contains("||ENDING||") And getMes.ToString.Contains("/\HELP/>") Then
                    'NameTB.Text & " :" & "/\HELP/>" & "Test" & "||ENDING||"
                    Dim O0 As String = getMes.ToString.Replace("||ENDING||", "")
                    Dim o As String() = Split(O0, "/\HELP/>")
                    RichTextBox1.AppendText(o(0) & o(1))
                    RichTextBox1.AppendText(Environment.NewLine)
                    getMes.Clear()
                ElseIf getMes.ToString.Contains("|BEFOREYOU||>") Then
                    Dim o As String = getMes.ToString.Replace("|BEFOREYOU||>", "")
                    RichTextBox1.AppendText(o)
                    RichTextBox1.AppendText(Environment.NewLine)
                    getMes.Clear()
                End If



            End If

        End While
    End Sub


















    Public Tcp_server As TcpListener
    Public Handle_Client As Thread
    Public ListOfCLients As List(Of TcpClient)
    Private Async Sub Guna2Button3_Click(sender As Object, e As EventArgs) Handles Guna2Button3.Click

        If Guna2CheckBox1.Checked And Guna2CheckBox2.Checked Then

            MessageBox.Show("Choice between server and client !")

        ElseIf Guna2CheckBox2.Checked Then

            ListOfCLients = New List(Of TcpClient)
            Tcp_server = New TcpListener(IPAddress.Any, Guna2TextBox2.Text)

            Tcp_server.Start()



            Await Task.Run(Sub() HandleThem())
            '    Handle_Client = New Thread(AddressOf HandleThem)
            '  Handle_Client.Start()

        End If
    End Sub
    Public Async Sub HandleThem()
        While True
            Try
                Dim o As TcpClient = Tcp_server.AcceptTcpClient

                Dim p As Byte() = Encoding.UTF8.GetBytes(RichTextBox1.Text & "|BEFOREYOU||>")

                Await o.GetStream.WriteAsync(p, 0, p.Length)

                ListOfCLients.Add(o)


                Await Task.Run(Sub() ServerDataCLI(o.GetStream, o.Client.RemoteEndPoint.ToString))
            Catch ex As Exception

            End Try


        End While
    End Sub

    Public Async Sub ServerDataCLI(ByVal Dta As NetworkStream, ByVal u As String)
        Try


            Dim StringHelper As New StringBuilder

            While True
                Dim b(4096) As Byte
                Dim RD As Integer = Await Dta.ReadAsync(b, 0, b.Length)


                Dim M As String = System.Text.Encoding.UTF8.GetString(b)
                If RD > 0 Then


                    StringHelper.Append(M)

                    If StringHelper.ToString.Contains("||ENDING||") And StringHelper.ToString.Contains("/\HELP/>") Then

                        Dim O0 As String = StringHelper.ToString.Replace("||ENDING||", "")
                        Dim o As String() = Split(O0, "/\HELP/>")
                        RichTextBox1.AppendText(o(0) & o(1))
                        RichTextBox1.AppendText(Environment.NewLine)


                        Dim haze As Byte() = System.Text.Encoding.UTF8.GetBytes(StringHelper.ToString)

                        For Each h As TcpClient In ListOfCLients
                            Try

                                'If h.Client.RemoteEndPoint.ToString <> u Then


                                Await h.GetStream.WriteAsync(haze, 0, haze.Length)


                                'End If
                            Catch ex As Exception

                                ListOfCLients.Remove(h)
                            End Try
                        Next

                        StringHelper.Clear()




                    End If
                End If

            End While

        Catch ex As Exception

        End Try

    End Sub






    Private Async Sub Guna2Button2_Click(sender As Object, e As EventArgs) Handles Guna2Button2.Click
        If Guna2CheckBox1.Checked And Guna2CheckBox2.Checked Then ''CLI

            MessageBox.Show("Choice between server and client !")
        ElseIf Guna2CheckBox1.Checked Then

            Dim hj As Byte() = System.Text.Encoding.UTF8.GetBytes(NameTB.Text & " : " & "/\HELP/>" & TB_Mess.Text & "||ENDING||")
            '  Dim hj As Byte() = System.Text.Encoding.UTF8.GetBytes("Test")
            CLIHelper.GetStream.Write(hj, 0, hj.Length)

        ElseIf Guna2CheckBox2.Checked Then ''SERVER


            Dim hj As Byte() = System.Text.Encoding.UTF8.GetBytes(NameTB.Text & " : " & "/\HELP/>" & TB_Mess.Text & "||ENDING||")




            Me.RichTextBox1.AppendText(NameTB.Text & " : " & TB_Mess.Text)

            Me.RichTextBox1.AppendText(Environment.NewLine)
            Dim i As TcpClient
            Try
                For Each i In ListOfCLients



                    Await i.GetStream.WriteAsync(hj, 0, hj.Length)





                Next

            Catch ex As Exception
                ListOfCLients.Remove(i)
            End Try

        End If
    End Sub


    Private Sub Guna2Button4_Click(sender As Object, e As EventArgs) Handles Guna2Button4.Click
        Application.Exit()
    End Sub

    Private Sub Guna2Button5_Click(sender As Object, e As EventArgs) Handles Guna2Button5.Click
        Me.WindowState = FormWindowState.Maximized
    End Sub

    Private Sub CloseAll() Handles MyBase.Closing
        CLI_Data.Abort()
        Handle_Client.Abort()
    End Sub
    Private Sub Guna2Button6_Click(sender As Object, e As EventArgs) Handles Guna2Button6.Click
        Me.WindowState = FormWindowState.Normal
    End Sub
End Class
