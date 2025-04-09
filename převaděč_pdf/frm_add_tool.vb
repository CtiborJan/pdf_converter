Public Class frm_add_tool
    Public tm As cls_tools_manager
    Public collection_to_add_to As cls_tools_collection
    Public added As Object
    Private Sub load_collections()
        Dim i As Integer
        Dim n As TreeNode
        trv_collections.Nodes.Clear()
        For i = 0 To tm.n_coll
            n = trv_collections.Nodes.Add(tm.collections(i).name)
            n.Tag = tm.collections(i)
            tm.collections(i).list_tools(n)
        Next
    End Sub
    Public Sub load_me(tm_ As cls_tools_manager, coll_ As cls_tools_collection)
        tm = tm_
        collection_to_add_to = coll_
        load_collections()
        Dim i As Long

        Dim types() As Type = Reflection.Assembly.GetExecutingAssembly.GetTypes
        If types IsNot Nothing Then
            For i = 0 To types.Count - 1
                If Strings.Left(types(i).Name, 9) = "cls_tool_" Then
                    lst_templates.Items.Add(types(i).Name)
                End If
            Next i
        End If
        Me.Show()
    End Sub

    Private Sub frm_add_tool_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub btn_add_to_collection_Click(sender As Object, e As EventArgs) Handles btn_add_to_collection.Click
        If collection_to_add_to IsNot Nothing And added IsNot Nothing Then
            Dim new_t As Object
            new_t = added.clone

            collection_to_add_to.add_tool(new_t)
            collection_to_add_to.list_tools()
        End If
    End Sub

    Private Sub trv_collections_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles trv_collections.AfterSelect
        If trv_collections.SelectedNode.Tag.GetType <> GetType(cls_tools_collection) Then
            added = trv_collections.SelectedNode.Tag
        End If
    End Sub

    Private Sub btn_add_template_Click(sender As Object, e As EventArgs) Handles btn_add_template.Click
        If collection_to_add_to IsNot Nothing And lst_templates.SelectedIndex <> -1 Then
            If txt_name.Text <> "" And txt_name_id.Text <> "" Then

                Dim t As Type
                Dim tps() As Type
                tps = Reflection.Assembly.GetExecutingAssembly.GetTypes
                Dim i As Long
                For i = 0 To tps.Count - 1
                    If tps(i).Name = lst_templates.SelectedItem Then t = tps(i)
                Next
                collection_to_add_to.add_tool(System.Activator.CreateInstance(t, txt_name.Text, txt_name_id.Text, txt_mark.Text, txt_description.Text,
                                                                              New cls_highligh_rule(txt_hgl.Text)))
                'collection_to_add_to.display(collection_to_add_to.parent.pnl_coll_container)
            End If
        End If
    End Sub
End Class