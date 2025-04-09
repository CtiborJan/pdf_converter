Public Class frm_dispatchers
    Private tm As cls_tools_manager
    Dim selected_connection as Integer = -1
    Private Sub frm_dispatchers_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
    Public Sub loadme(tm_ As cls_tools_manager)
        tm = tm_
        Me.Show()
        list_dispatchers()
        list_listeners()
    End Sub
    Public Sub list_listeners()
        Dim i as Integer, j as Integer
        Dim n As TreeNode
        Dim n2 As TreeNode
        Dim p As Object
        Dim popisek As String
        trv_listeners.Nodes.Clear()
        For i = 0 To tm.n_coll
            With tm.collections(i)
                n = trv_listeners.Nodes.Add(.name) 'tady se vypíš jméno kolekce
                n.Tag = tm.collections(i)
                .list_my_event_listeners(n)
            End With
        Next
        trv_listeners.ExpandAll()
    End Sub
    Public Sub list_dispatchers()
        Dim i as Integer, j as Integer
        Dim n As TreeNode
        Dim n2 As TreeNode
        Dim p As Object
        Dim popisek As String
        trv_dispatchers.Nodes.Clear()

        For i = 0 To tm.n_edisp
            With tm.event_dispatchers(i)
                n = trv_dispatchers.Nodes.Add(tm.event_dispatchers(i).toString)
                n.Tag = tm.event_dispatchers(i)
                n.ForeColor = Color.DodgerBlue
                For j = 0 To .n_connections
                    popisek = ""
                    p = .connections(j).event_listener.parent
                    Do While p.GetType = GetType(cls_tools_collection)
                        popisek &= p.name & " > " & popisek
                        p = p.parent
                    Loop
                    popisek &= " - " & .connections(j).event_listener.description & " (" & .connections(j).connection_id & ")"
                    n2 = n.Nodes.Add(.connections(j).connection_id, popisek)
                    n2.Tag = .connections(j)
                    n2.ForeColor = Color.Gray
                Next
            End With
        Next
        trv_dispatchers.ExpandAll()
    End Sub

    Private Sub trv_listeners_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles trv_listeners.AfterSelect
        Dim i as Integer
        selected_connection = -1
        If trv_listeners.SelectedNode IsNot Nothing Then

            Dim id as Integer
            If trv_listeners.SelectedNode.Name <> "" Then
                id = CStr(trv_listeners.SelectedNode.Name)
                Dim tmp As Object
                tmp = trv_dispatchers.Nodes.Find(id, True)(0)
                If tmp IsNot Nothing Then trv_dispatchers.SelectedNode = tmp
                selected_connection = id
            End If
        End If
    End Sub

    Private Sub trv_dispatchers_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles trv_dispatchers.AfterSelect

    End Sub

    Private Sub mnu_remove_connection_Click(sender As Object, e As EventArgs) Handles mnu_remove_connection.Click
        If selected_connection <> -1 Then
            Dim i as Integer
            Dim tmp As cls_DL_connection
            For i = 0 To tm.n_edisp
                tmp = tm.event_dispatchers(i).get_connection_by_id(selected_connection)
                If tmp IsNot Nothing Then
                    tmp.delete()
                    list_dispatchers()
                    list_listeners()
                    Exit Sub
                End If
            Next
        End If
    End Sub

    Private Sub trv_listeners_BeforeSelect(sender As Object, e As TreeViewCancelEventArgs) Handles trv_listeners.BeforeSelect

    End Sub

    Private Sub trv_listeners_MouseDown(sender As Object, e As MouseEventArgs) Handles trv_listeners.MouseDown
        Dim hti As TreeViewHitTestInfo
        hti = trv_listeners.HitTest(New Point(e.X, e.Y))
        If hti.Node IsNot Nothing Then
            If hti.Node.Tag.GetType = GetType(cls_event_listener) Then
                trv_listeners.ContextMenuStrip = cxm_actionname
            ElseIf hti.Node.Tag.GetType = GetType(cls_DL_connection) Then
                trv_listeners.ContextMenuStrip = cxm
            Else
                trv_listeners.ContextMenuStrip = Nothing
            End If
        End If
    End Sub

    Private Sub mnu_set_dispatcher_Click(sender As Object, e As EventArgs) Handles mnu_set_dispatcher.Click
        'If trv_listeners.SelectedNode.Tag 
        If trv_listeners.SelectedNode.Tag IsNot Nothing Then
            frm_event_desc.Show()
            frm_event_desc.tm = Me.tm
            frm_event_desc.listener = trv_listeners.SelectedNode.Tag
            frm_event_desc.addnew = True
            frm_event_desc.disp_list_form = Me
        End If
    End Sub

    Private Sub cxm_actionname_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles cxm_actionname.Opening

    End Sub

End Class