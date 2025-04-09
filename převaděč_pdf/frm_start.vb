Public Class frm_start
    Private Sub frm_start_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'My.Computer.FileSystem.CopyFile("C:\Users\jctibor\Documents\FLU\převaděč_pdf\převaděč_pdf\bin\Release\převaděč_pdf.exe","H:\na flashdisk\převaděč_pdf.exe")
        'RichTextBox1.Text = "a" & ChrW(&H305)
        main()
        Me.Close()
        'a̅
    End Sub
End Class