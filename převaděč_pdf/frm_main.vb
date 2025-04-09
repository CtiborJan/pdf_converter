Imports System.ComponentModel

Public Class frm_main

    Public Function get_env_container() As Control
        Return tbp_doc
    End Function
    Public Function get_pdf_container() As Control
        Return tbp_pdf
    End Function
    Public Function get_pdf_control() As AxAcroPDFLib.AxAcroPDF
        Return pdf
    End Function

    Public Function get_tools_container() As Control
        Return tbp_tools
    End Function
    Public Function get_page_rtb_container() As Control
        Return spc_main.Panel2
    End Function
    Public Sub set_left_panel_width(w As Integer)
        spc_main.SplitterDistance = w
    End Sub
    Public Function get_left_panel_width() As Integer
        If Math.Abs(spc_main.Panel1.Width - spc_main.Panel2.Width) > 5 Then
            Return spc_main.Panel1.Width
        Else 'je-li to rozděleno napůl, vrátíme 0
            Return 0
        End If
    End Function
    Public Sub activate_pdf_container()
        Dim i As Integer
        For i = 0 To tbc.TabCount - 1
            If tbc.TabPages(i).Name = "tbp_pdf" Then tbc.SelectTab(i)
        Next
    End Sub

    Private Sub frm_main_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If env IsNot Nothing Then
            If env.opened_document IsNot Nothing Then
                env.close_document()
            End If
        End If
    End Sub

    Private Sub web_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs)

    End Sub
End Class