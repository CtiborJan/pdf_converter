
Public Class frm_event_desc
    Private event_codes(12) as Integer
    Public listener As cls_event_listener 'tohle dostaneme
    Private ke_desc As cls_keyevent_args
    Private me_desc As cls_mousevent_args
    Public tm As cls_tools_manager
    Public addnew As Boolean
    Public disp_list_form As Object
    Private Sub frm_event_desc_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        With lst_events.Items
            Dim u As Object

            .Add("Stisk klávesy")
            event_codes(.Count - 1) = EN.evn_FRM_KEY_DOWN
            .Add("Uvolnění klávesy")
            event_codes(.Count - 1) = EN.evn_FRM_KEY_UP
            .Add("Úder klávesy")
            event_codes(.Count - 1) = EN.evn_FRM_KEY_PRESS
            .Add("Kliknutí myší")
            event_codes(.Count - 1) = EN.evn_RTB_MOUSE_CLICK
            .Add("Dvojité kliknutí myší")
            event_codes(.Count - 1) = EN.evn_RTB_MOUSE_DBL_CLICK
            .Add("Stisk tl. myši")
            event_codes(.Count - 1) = EN.evn_RTB_MOUSE_DOWN
            .Add("Uvolnění tl. myši")
            event_codes(.Count - 1) = EN.evn_RTB_MOUSE_UP
            .Add("Pohnutí myší")
            event_codes(.Count - 1) = EN.evn_RTB_MOUSE_MOVE
            .Add("Vložení textu")
            event_codes(.Count - 1) = EN.evn_TEXT_INSERTED
            .Add("Změna textu v text. poli")
            event_codes(.Count - 1) = EN.evn_RTB_CHANGED
            .Add("Změna výběru nebo pozice kurzoru")
            event_codes(.Count - 1) = EN.evn_RTB_SELECTION_CHANGED
            .Add("Otevření stránky")
            event_codes(.Count - 1) = EN.evn_PAGE_OPENED
            .Add("Zavření stránky")
            event_codes(.Count - 1) = EN.evn_PAGE_CLOSED
        End With
    End Sub

    Private Sub lst_events_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lst_events.SelectedIndexChanged
        Dim index as Integer
        index = lst_events.SelectedIndex
        If event_codes(index) > EN.first_keyevent And event_codes(index) < EN.last_keyevent Then
            tbc_main.SelectedIndex = 0
        ElseIf event_codes(index) > EN.first_mouseevent And event_codes(index) < EN.last_mouseevent Then
            tbc_main.SelectedIndex = 1
        End If
    End Sub



    Private Sub txt_ke_keycode_KeyDown(sender As Object, e As KeyEventArgs) Handles txt_ke_keycode.KeyDown
        e.SuppressKeyPress = True
        If lst_events.SelectedIndex > -1 And e.KeyCode <> 17 And e.KeyCode <> 16 And e.KeyCode <> 18 Then
            If event_codes(lst_events.SelectedIndex) = EN.evn_FRM_KEY_DOWN Or event_codes(lst_events.SelectedIndex) = EN.evn_FRM_KEY_UP Then
                txt_ke_keycode.Text = GetType(Keys).GetEnumName(e.KeyCode)
                e.SuppressKeyPress = True
                chb_ke_alt.Checked = e.Alt
                chb_ke_shift.Checked = e.Shift
                chb_ke_ctrl.Checked = e.Control
                ke_desc = New cls_keyevent_args(e.KeyCode, e.Control, e.Alt, e.Shift)
            End If
        End If
    End Sub

    Private Sub txt_ke_keycode_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txt_ke_keycode.KeyPress
        If lst_events.SelectedIndex > -1 And Val(e.KeyChar) <> 17 And Val(e.KeyChar) <> 16 And Val(e.KeyChar) <> 18 Then
            If event_codes(lst_events.SelectedIndex) = EN.evn_FRM_KEY_DOWN Or event_codes(lst_events.SelectedIndex) = EN.evn_FRM_KEY_UP Then
                txt_ke_keycode.Text = GetType(Keys).GetEnumName(e.KeyChar)
                ke_desc = New cls_keyevent_args(Val(e.KeyChar), chb_ke_ctrl.Checked, chb_ke_alt.Checked, chb_ke_shift.Checked)
            End If
        End If
    End Sub

    Private Sub cmd_ok_Click(sender As Object, e As EventArgs) Handles cmd_ok.Click
        Dim index as Integer
        index = lst_events.SelectedIndex
        If event_codes(index) > EN.first_keyevent And event_codes(index) < EN.last_keyevent Then
            If addnew = True Then
                tm.add_event_listener(listener,
                                       New cls_event_description(event_codes(index),
                                                                 New cls_keyevent_args(ke_desc.value, chb_ke_ctrl.Checked, chb_ke_alt.Checked, chb_ke_shift.Checked), Nothing))
            End If
        ElseIf event_codes(index) > EN.first_mouseevent And event_codes(index) < EN.last_mouseevent Then
            If addnew = True Then
                tm.add_event_listener(listener,
                                       New cls_event_description(event_codes(index), Nothing,
                                                                 New cls_mousevent_args(cmb_button.SelectedIndex, chb_me_ctrl.Checked, chb_me_alt.Checked, chb_me_shift.Checked)))
            End If
        Else
            If addnew = True Then
                tm.add_event_listener(listener, New cls_event_description(event_codes(index), Nothing, Nothing))
            End If
        End If
        If disp_list_form IsNot Nothing Then
            disp_list_form.list_dispatchers
            disp_list_form.list_listeners
        End If
    End Sub
End Class