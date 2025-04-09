Imports System.Text.RegularExpressions


'####################################################################################################################################################################################################################
'Modul obsahuje různé pomocné funkce
Module bas_main
    Public xyz
    Public Const program_version As String = " (v 1.20210720)" 'verze verse
    Public Enum EN
        first_textevent
        evn_RTB_CHANGED
        evn_RTB_SELECTION_CHANGED
        last_textevent
        first_keyevent
        evn_FRM_KEY_PRESS
        evn_FRM_KEY_DOWN
        evn_FRM_KEY_UP
        last_keyevent
        first_mouseevent
        evn_RTB_MOUSE_CLICK
        evn_RTB_MOUSE_DBL_CLICK
        evn_RTB_MOUSE_DOWN
        evn_RTB_MOUSE_UP
        evn_RTB_MOUSE_MOVE
        last_mouseevent
        first_documentevent
        evn_PAGE_CLOSED
        evn_PAGE_OPENED
        evn_TEXT_INSERTED
        last_documentevent
    End Enum
    Public Function get_singlenode_value(n As Xml.XmlNode, xpath As String, Optional def_on_notfound As String = "") As String
        Dim tmp As string_2
        Dim n2 As Xml.XmlNode
        If n Is Nothing Then Stop
        n2 = n.SelectSingleNode(xpath)
        If n2 IsNot Nothing Then
            If n2.Attributes IsNot Nothing Then
                If n2.Attributes.GetNamedItem("value") IsNot Nothing Then
                    Return n2.Attributes.GetNamedItem("value").Value 'protože třeba jen mezeru to prostě nevezme: 
                    'místo ní To předá prázdný řetězec, takže tohle je taková obezlička hlavně pro tenhle případ
                End If
            End If
            Return n2.InnerText
            Else
                Return def_on_notfound
        End If
    End Function
    Public Function string2Arr_to_stringArr(string2arr() As string_2) As String()
        Dim i As Long
        Dim tmp() As String
        If string2arr IsNot Nothing Then
            ReDim tmp(UBound(string2arr))
            For i = 0 To UBound(string2arr)
                tmp(i) = string2arr(i).str
            Next
        End If
        Return tmp
    End Function

    Public Class myEventArgs
        Public e As Object 'u událostí myši nebo klávesnice je tohle původní objekt event_args
        Public ctrl As Boolean
        Public alt As Boolean
        Public shift As Boolean
        Public acc As Object
    End Class


    Public Function create_myEventArgs(e As Object, k As Keys, active_control As Object) As myEventArgs
        Dim tmp As myEventArgs
        tmp = New myEventArgs
        tmp.e = e
        tmp.acc = active_control
        If (Control.ModifierKeys And Keys.Control) = Keys.Control Then tmp.ctrl = True
        If (Control.ModifierKeys And Keys.Alt) = Keys.Alt Then tmp.alt = True
        If (Control.ModifierKeys And Keys.Shift) = Keys.Shift Then tmp.shift = True


        Return tmp
    End Function


    Public initialized As Boolean
    Public initializing As Boolean



    Public env As cls_environment_2

    Public Sub main()


        initializing = True

        frm_main.Show()
        frm_main.Visible = False
        env = New cls_environment_2(frm_main)

        env.wsp = New cls_workspace()
        env.load_settings()
        env.run_workspace()
        If env.last_opened IsNot Nothing Then
            If env.last_opened(0) <> "" Then
                env.open_document(env.last_opened(0) & "\dokument.xml")
            End If
        End If
        'env.new_document("Testovací", "xyz", "xyz")
        frm_main.tbp_tools.Text = env.c("Nástroje")
        frm_main.tbp_doc.Text = env.c("Dokument")

        frm_main.Visible = True
    End Sub

    Public Function InStr_first(start As Integer, string_to_search_in As String, ByRef ret_position As Integer, ByRef index_of_found As Integer, ParamArray searched() As String) As Integer
        Dim i As Integer
        'xyz = Mid(string_to_search_in, start)
        Dim fi As Integer = -1
        Dim rv As Integer = -1
        Dim ri As Integer = -1 'return index
        If searched IsNot Nothing Then
            For i = 0 To UBound(searched)
                fi = InStr(CInt(start), string_to_search_in, searched(i))
                If fi <> 0 And (fi < rv Or rv = -1) Then
                    rv = fi
                    ri = i
                End If
            Next
            ret_position = rv
            Return rv
        Else
            Return -1
        End If
    End Function
    Public Function InStrRev_first(start As Integer, string_to_search_in As String, ByRef ret_position As Integer, ByRef index_of_found As Integer, ParamArray searched() As String) As Integer
        Dim i As Integer
        Dim fi As Integer = -1
        Dim rv As Integer = -1
        Dim ri As Integer = -1 'return index
        If searched IsNot Nothing Then
            For i = 0 To UBound(searched)
                fi = InStrRev(string_to_search_in, searched(i), CInt(start))
                If fi <> 0 And (fi > rv Or rv = -1) Then
                    rv = fi
                    ri = i
                End If
            Next
            index_of_found = ri
            ret_position = rv
            Return rv
        Else
            index_of_found = -1
            Return -1
        End If
    End Function
    Public Function InStrX(start As Integer, string1 As String, string2 As String, ByRef index_of_found As Integer) As Integer
        If start = 0 Then Stop
        InStrX = InStr(CInt(start), string1, string2)
        index_of_found = InStrX
    End Function

    Public Function describe_rx(rx As String) As String
        Dim t As String
        Dim ch As String
        Dim pch As String = ""
        Dim nch As String
        Dim in_sqb As Boolean

        Dim i As Integer
        Dim j As Integer
        For i = 1 To Len(rx)
            ch = Mid(rx, i, 1)
            If i <> Len(rx) Then nch = Mid(rx, i + 1, 1)
            If i = 1 And ch = "^" Then
                t &= "Na začátku řetězce je "
            ElseIf ch = "[" And ie(rx, i) = False Then
                t &= drx_get_bracket_area(rx, i, "[", i)
            ElseIf ch = "(" And ie(rx, i) = False Then
                t &= drx_get_bracket_area(rx, i, "(", i)
            ElseIf ch = "{" And ie(rx, i) = False Then
                t &= drx_get_bracket_area(rx, i, "{", i)
            ElseIf ch = "*" And ie(rx, i) = False Then
                t &= " kolikrátkoliv (0-nekonečno) "
            ElseIf ch = "+" And ie(rx, i) = False Then
                t &= " alespoň jednou "
            ElseIf ch = "?" And ie(rx, i) = False Then
                t &= " nulakrát nebo jednou "
            ElseIf pch = "\" And ch = "s" Then
                t &= " mezera "
            Else
                t &= ch
            End If

            pch = ch
        Next
    End Function
    Private Function drx_get_bracket_area(rx As String, start As Integer, b As String, ByRef ends As Integer) As String
        Dim i As Integer
        Dim ch As String
        Dim pch As String
        Dim ppch As String
        Dim nch As string_2
        Dim t As String
        If b = "[" Then
            Dim neg As Boolean
            For i = start To Len(rx)
                ch = Mid(rx, i, 1)
                If ch = "]" And cbs(rx, i - 1) Mod 2 = 1 Then
                    ends = i
                    If neg = True Then
                        t = " jeden z uvedených znaků (" & Mid(rx, start, (i - start) + 1) & ") "
                    Else
                        t = " libovolný jiný než uvedený znak (" & Mid(rx, start, (i - start) + 1) & ") "
                    End If
                    Return t
                ElseIf i = start + 1 And ch = " ^ " Then
                    neg = True
                End If
                pch = ch
            Next
        ElseIf b = "{" Then
            For i = start To Len(rx)
                ch = Mid(rx, i, 1)
                If ch = "}" Then
                    ends = i
                    Return Mid(rx, start, (i - start) + 1)
                End If
            Next
        ElseIf b = "(" Then
            Dim in_sqb As Boolean
            Dim n_opened As Long
            For i = start To Len(rx)
                ch = Mid(rx, i, 1)
                If ch = "[" And ie(rx, i) = False Then
                    in_sqb = True
                ElseIf ch = "]" And ie(rx, i) = False Then
                    in_sqb = False
                ElseIf ch = "(" And ie(rx, i) = False And in_sqb = False Then
                    n_opened += 1
                ElseIf ch = ")" And ie(rx, i) = False And in_sqb = False Then
                    n_opened -= 1
                    If n_opened = -1 Then 'zavřela se otvírací závorka!
                        ends = i
                        Return Mid(rx, start, (i - start) + 1)
                    End If
                End If
            Next
        End If
    End Function
    Private Function cbs(rx As String, pos As Integer) As Integer 'cbs=count_backslashes
        Dim i As Long
        cbs = 0
        For i = pos To 1 Step -1
            If Mid(rx, i, 1) = "\" Then
                cbs += 1
            Else
                Return cbs
            End If
        Next
    End Function
    Private Function ie(rx As String, pos As Integer) As Boolean 'ie=is_escaped
        Dim nbs As Integer = cbs(rx, pos - 1)
        If nbs Mod 2 = 0 Then
            Return False
        Else
            Return True
        End If
    End Function
    Public Function rgxt(str As String, pattern As String, Optional rgxOptions As RegexOptions = 0) As Boolean
        Dim rx As New Regex(pattern, rgxOptions)
        Return rx.IsMatch(str)

    End Function

    Public Function rgx(str As String, pattern As String, rv As String, Optional rewrite_rv_on_no_success As Boolean = True, Optional rgxOptions As RegexOptions = 0, Optional r_match As Match = Nothing) As String
        Dim rx As New Regex(pattern, rgxOptions)
        Dim m As Match
        m = rx.Match(str)
        If m.Success = True Then
            rv = m.Captures(0).Value
            If Not r_match Is Nothing Then
                r_match = m
            End If
            Return rv
        Else
            If rewrite_rv_on_no_success = True Then rv = ""
            Return ""
        End If
    End Function
    Public Function rgx(str As String, pattern As String, Optional rgxOptions As RegexOptions = 0, Optional ByRef r_match As Match = Nothing) As String
        Dim rx As New Regex(pattern, rgxOptions)
        Dim m As Match
        m = rx.Match(str)
        If m.Success = True Then

            r_match = m
            Return m.Captures(0).Value
        Else
            Return ""
        End If
    End Function
    Public Function rgx(str As String, pattern As String, startat As Integer, Optional rgxOptions As RegexOptions = 0, Optional ByRef r_match As Match = Nothing) As String
        Dim rx As New Regex(pattern, rgxOptions)
        Dim m As Match
        m = rx.Match(str, CInt(startat))
        If m.Success = True Then

            r_match = m
            Return m.Captures(0).Value
        Else
            r_match = m
            Return ""
        End If
    End Function
    Public Function rgx_g(str As String, pattern As String, Optional rgxOptions As RegexOptions = 0, Optional ByRef r_match As Match = Nothing) As String
        Dim rx As New Regex(pattern, rgxOptions)
        Dim m As Match
        If str Is Nothing Then Return ""
        m = rx.Match(str)
        If m.Success = True And m.Groups.Count > 1 Then

            r_match = m
            Return m.Groups(1).Value
        Else
            Return ""
        End If
    End Function
    Public Function rgxx(str As String, pattern As String, ByRef rv() As String,
                         Optional rewrite_rv_on_no_success As Boolean = True,
                         Optional rgxOptions As RegexOptions = 0, Optional ByRef r_matchc As MatchCollection = Nothing) As String
        Dim rx As New Regex(pattern, rgxOptions)
        Dim m As MatchCollection

        m = rx.Matches(str)
        r_matchc = m
        If m.Count > 0 Then
            Dim i As Integer
            If m.Item(0).Groups.Count > 1 Then
                ReDim rv(m.Item(0).Groups.Count - 2)
                For i = 0 To m.Item(0).Groups.Count - 2
                    rv(i) = m.Item(0).Groups(CInt(i + 1)).Value
                Next
                r_matchc = m
                Return rv(0)
            End If
        Else
            If rewrite_rv_on_no_success = True Then
                ReDim rv(0)
                rv(0) = ""
            End If
            Return ""
        End If
    End Function
    Public Function str_to_tags(str As String) As String()
        Dim si As Integer
        Dim si2 As Integer
        Dim ei As Integer
        Dim tname As String
        Dim t As String
        Dim tmp() As String
        Dim n As Integer = -1
        Dim sc As Boolean
        Do While InStrX(si + 1, str, "<", si) <> 0
            ei = InStr(si, str, ">")
            si2 = InStr(si + 1, str, "<")
            If ei > si2 And si2 <> 0 Then
                Return Nothing
            End If
            t = Mid(str, si + 1, ei - si - 1)
            If InStr(1, t, "/>") <> 0 Then
                sc = True
            Else
                sc = False
            End If
            tname = rgx_g(t, "<([^\s/>]+)")
            n += 1
            ReDim Preserve tmp(n)
            tmp(n) = t
            'tmp(n) = New cls_preXML_tag(tname, Nothing, New Point(0, 0), sc)
        Loop
        Return tmp
    End Function
    Public Function rgxr(str As String, pattern As String, replacement As String,
                         Optional rgxOptions As RegexOptions = 0, Optional ByRef r_matchc As MatchCollection = Nothing) As String
        Dim rx As New Regex(pattern, rgxOptions)
        Dim m As MatchCollection
        If r_matchc IsNot Nothing Then r_matchc = rx.Matches(str)
        If str IsNot Nothing And replacement IsNot Nothing Then
            Return rx.Replace(str, replacement)
        End If
    End Function
    Public Function check_RX_in_textbox(tb As TextBox)
        Dim e As Exception
        Dim v As Boolean
        v = text_rx_pattern_validity(tb.Text, e)
        highlight_textbox_rx_validity(tb, v, e)
    End Function
    Public Function text_rx_pattern_validity(rx_pattern As String, ByRef excp As Exception) As Boolean
        'otestujeme, je-li regulérní výraz validní
        Try
            Dim rx = New Regex(rx_pattern)
            rx.IsMatch("libovolný text")
        Catch ex As Exception
            excp = ex
            Return False
        End Try
        Return True
    End Function
    Public Function highlight_textbox_rx_validity(tb As TextBox, valid As Boolean, ex As Exception)
        'vybarví políčko podle toho, jestli regex v něm zapsaný je správně a pokud není, zobrazí tlačítko s nápovědou...
        Dim btn As Button
        If valid = False Then
            tb.BackColor = Color.LightPink
            Dim i As Long
            btn = tb.Controls.Item("btn_bad_regex_info")
            If btn IsNot Nothing Then 'tlačítko už existuje
                btn.Tag = ex
                AddHandler btn.Click, AddressOf btn_bad_regex_info_click
            Else
                btn = New Button
                btn.Parent = tb
                'btn.Left = tb.Width - 26
                'btn.Height = 25
                btn.Width = 25
                btn.Dock = DockStyle.Right
                'btn.Top = -1
                btn.Text = "?"
                btn.Tag = ex
                btn.BackColor = SystemColors.ButtonFace
                btn.Name = "btn_bad_regex_info"
                btn.Cursor = Cursors.Default
                AddHandler btn.Click, AddressOf btn_bad_regex_info_click
            End If
        Else
            tb.BackColor = Color.White
            tb.Controls.RemoveByKey("btn_bad_regex_info") 'odstraníme tlačítko s nápovědou
        End If
    End Function
    Public Sub btn_bad_regex_info_click(sender As Object, e As EventArgs)
        If sender.tag IsNot Nothing Then
            MsgBox(env.c("V regulérním výrazu v políčku je chyba: ") & vbNewLine & sender.tag.Message,, env.c("Chyba v regulérním výrazu!"))
        End If
    End Sub

    Public Function tag_on_position(ByRef text As String, pos As Integer) As String
        Dim psi As Integer
        Dim pei As Integer
        Dim nei As Integer
        Dim nsi As Integer
        psi = InStrRev(text, "<", pos)
        If psi = 0 Then Return ""
        pei = InStrRev(text, ">", pos)
        If pei > psi Then Return ""
        nei = InStr(pos, text, ">")
        If nei = 0 Then Return ""
        nsi = InStr(pos, text, "<")
        If nsi < nei And nsi <> 0 Then Return ""
        Return Mid(text, psi, 1 + nei - psi)
    End Function

    Public Function InStr_tag(text As String, start As Integer, ByRef ri As Integer, tag As String, ParamArray attributes() As String)
        Dim si As Integer
        Dim ei As Integer
        Dim nsi As Integer
        Dim t As String
        si = start

        Do While InStr_first(si, text, si, 0, "<" & tag & ">", "<" & tag & "/", "<" & tag & " ", "<" & tag & vbLf) <> -1
            ei = InStr(si, text, ">")
            nsi = si
            'nsi = InStr(si + 1, text, "<") 'abychom vyloučili případné chyby ve formátování - ??????
            If ei <> 0 And (ei < nsi Or nsi = 0) Then
                t = Mid(text, si, ei - si)
                Dim i As Long
                Dim j As Long
                Dim a() As String
                Dim atr As String
                Dim atr_v As String
                Dim atr_op As String
                For i = 0 To UBound(attributes)
                    a = Split(attributes(i), "=")
                Next
            End If
        Loop
    End Function
    Private Function Space2(n As Integer, ch As String) As String
        Dim i As Long
        For i = 1 To n
            Space2 &= ch
        Next
        Return Space2
    End Function
    Public Function arab_to_roman(ByVal a As Integer) As String
        On Error GoTo err
        Dim v As String
        Dim rl As String = "mdclxvi"
        Dim lvs() As Integer = {1000, 500, 100, 50, 10, 5, 1}
        If a < 4000 Then
            Dim m As Long
            Dim lV As Integer
            Dim l As String
            For i = 0 To Len(rl) - 1
                l = rl(i)
                lV = lvs(i)
                m = Int(a / lV)
                If m <> 0 Then
                    If m <> 4 Then
                        v = v & Space2(m, l)
                    Else
                        v = v & rl(i) & rl(i - 1)
                    End If
                End If
                a = a - (m * lV)
                If lV = 1000 Or lV = 100 Or lV = 10 Then
                    'xyz = a / (lV / 10)
                    If Int(a / (lV / 10)) = 9 Then
                        v = v & rl(i + 2) & l
                        a = a - (lV / 10) * 9
                    End If
                End If
                If a = 0 Then Return v
            Next i
            Return v
        Else
            Return "moc vysoké číslo..."
        End If
Err:
        Return ""
    End Function
    Public Function roman_to_arab(r As String) As Integer
        On Error GoTo err
        Dim v As Integer
        Dim i As Integer
        For i = 0 To Len(r) - 1
            If i <> Len(r) - 1 Then
                If rvalue(r(i)) >= rvalue(r(i + 1)) Then
                    v = v + rvalue(r(i))
                Else
                    v = v + (rvalue(r(i + 1)) - rvalue(r(i)))
                    i += 1
                End If
            Else
                v = v + rvalue(r(i))
            End If
        Next
        Return v
Err:
        Return 0
    End Function
    Private Function rvalue(r As String) As Integer
        r = LCase(r)
        Select Case r
            Case "i"
                Return 1
            Case "v"
                Return 5
            Case "x"
                Return 10
            Case "l"
                Return 50
            Case "c"
                Return 100
            Case "d"
                Return 500
            Case "m"
                Return 1000
        End Select

    End Function
End Module
Public Structure string_2
    Public str As String
    Public index As Integer
    Public param As Object
End Structure

'####################################################################################################################################################################################################################
Public Class cls_KeyStat
    Public Alt As Boolean
    Public Control As Boolean
    Public Shift As Boolean
    Public KeyCode As Integer
End Class
Public Class cls_color_tbl
    Public n_colors As Integer = -1
    Public colors() As String
    Private Structure xclr
        Public start_index As Integer
        Public name As String
        Public count As Integer
    End Structure

    Private clrs() As xclr

    Public Sub New()
        ReDim colors(0)
        n_colors = 0
        colors(0) = "\red0\green0\blue0"
    End Sub

    Public Sub create_default_clrtbl()
        n_colors = 29
        ReDim colors(n_colors)
        ReDim clrs(7)
        colors(0) = "\red255\green255\blue255"
        colors(1) = "\red0\green0\blue0"

        'šedá
        colors(2) = "\red230\green230\blue230"
        colors(3) = "\red210\green210\blue210"
        colors(4) = "\red190\green190\blue190"
        colors(5) = "\red170\green170\blue170"
        colors(6) = "\red150\green150\blue150"
        colors(7) = "\red130\green130\blue130"
        colors(8) = "\red110\green110\blue110"
        colors(9) = "\red90\green90\blue90"
        colors(10) = "\red70\green70\blue70"
        colors(11) = "\red50\green50\blue50"
        clrs(2).name = "gray" : clrs(2).start_index = 2 : clrs(2).count = 10

        'červená
        colors(12) = "\red200\green0\blue0"
        colors(13) = "\red255\green150\blue150"
        colors(14) = "\red255\green220\blue220"
        clrs(3).name = "red" : clrs(3).start_index = 12 : clrs(3).count = 3

        'zelená
        colors(15) = "\red0\green200\blue0"
        colors(16) = "\red150\green255\blue150"
        colors(17) = "\red220\green255\blue220"
        clrs(4).name = "green" : clrs(4).start_index = 15 : clrs(4).count = 3

        'modrá
        colors(18) = "\red0\green0\blue200"
        colors(19) = "\red150\green150\blue255"
        colors(20) = "\red220\green220\blue255"
        clrs(5).name = "yellow" : clrs(5).start_index = 18 : clrs(5).count = 3

        'žlutá
        colors(21) = "\red255\green230\blue0"
        colors(22) = "\red255\green235\blue100"
        colors(23) = "\red255\green245\blue200"
        clrs(6).name = "yellow" : clrs(5).start_index = 21 : clrs(5).count = 3

        'oranžová
        colors(24) = "\red255\green128\blue0"
        colors(25) = "\red255\green180\blue100"
        colors(26) = "\red255\green220\blue200"

        'světle modrá
        colors(27) = "\red180\green220\blue255"
        colors(28) = "\red220\green240\blue255"
        colors(29) = "\red240\green250\blue255"

        clrs(7).name = "orange" : clrs(7).start_index = 24 : clrs(7).count = 3
    End Sub
    Public Function def_clr(clrname As String) As Integer
        Select Case LCase(clrname)
            Case "white"
                Return 0
            Case "black"
                Return 1
            Case "grey1"
                Return 2
            Case "grey2"
                Return 3
            Case "grey3"
                Return 4
            Case "grey" 'def
                Return 4
            Case "grey4"
                Return 5
            Case "grey5"
                Return 6
            Case "grey6"
                Return 7
            Case "grey7"
                Return 8
            Case "grey8"
                Return 9
            Case "grey9"
                Return 10
            Case "grey10"
                Return 11
            Case "red" 'def
                Return 12
            Case "red1"
                Return 12
            Case "red2"
                Return 13
            Case "red3"
                Return 14
            Case "green" 'def
                Return 15
            Case "green1"
                Return 15
            Case "green2"
                Return 16
            Case "green3"
                Return 17
            Case "blue" 'def
                Return 18
            Case "blue1"
                Return 18
            Case "blue2"
                Return 19
            Case "blue3"
                Return 20
            Case "yellow" 'def
                Return 21
            Case "yelow1"
                Return 21
            Case "yellow2"
                Return 22
            Case "yellow3"
                Return 23
            Case "orange" 'def
                Return 24
            Case "orange1"
                Return 24
            Case "orange2"
                Return 25
            Case "orange3"
                Return 26
            Case "lblue1"
                Return 27
            Case "lblue2"
                Return 28
            Case "lblue3"
                Return 29
        End Select


    End Function
    Public Function clr(r As Integer, g As Integer, b As Integer) As Integer
        'tato funkce jednak přidá novou barvu do tabulky barev, jednak vrátí její hodnotu - tu vrátí i v případě, že už tam barva je
        Dim i As Integer
        Dim clrcode As String
        clrcode = "\red" & r & "\green" & g & "\blue" & b
        For i = 0 To n_colors
            If colors(i) = clrcode Then
                Return i
            End If
        Next i
        n_colors += 1
        ReDim Preserve colors(n_colors)
        colors(n_colors) = clrcode
        Return n_colors
    End Function

    Public Function create_clrtbl() As String
        Dim i As Integer
        Dim o As String
        o = "{\colortbl;"
        For i = 0 To n_colors
            o = o & colors(i) & ";"
        Next
        o = o & "}"
        Return o
    End Function
    Public Sub set_clr_tbl(ByRef rtf As String)
        Dim clrtbl As String
        clrtbl = create_clrtbl()
        rgxr(rtf, "({\colortbl;[^}]*})", clrtbl)
    End Sub
End Class

'####################################################################################################################################################################################################################

Public Class cls_highligh_rule
    Public Structure character_rtf_formating
        'tady máme uložené informace o předchozím znaku při vykreslování rtf - abchom pak u znaků, které mají stejné formátování, neměli u každého jednotlivě to formátování celé vypsané
        Dim cf As Integer
        Dim highlight As Integer
        Dim bold As Boolean
        Dim italics As Boolean
        Dim fs As Integer
        Dim f As Integer
        Dim ul As Boolean
        Dim ulth As Boolean
        Dim uld As Boolean
        Dim _ch As String
        Dim _form As String
        Dim utf As Boolean
    End Structure
    Private Structure character_rtf_formating2
        'tady máme uložené informace o předchozím znaku při vykreslování rtf - abchom pak u znaků, které mají stejné formátování, neměli u každého jednotlivě to formátování celé vypsané
        Dim cf_defclr As String
        Dim cf_index As Integer

        Dim highlight_defclr As String
        Dim highlight_index As String

        Dim bold As Boolean
        Dim italics As Boolean
        Dim fs As Integer
        Dim f As Integer
        Dim ul As Boolean
        Dim ulth As Boolean
        Dim uld As Boolean
        Dim _ch As String
        Dim _form As String
    End Structure
    Private char_rtf As character_rtf_formating2
    Private format As String

    Public Sub get_chrtf(ByRef chrtf As character_rtf_formating, clrtbl As cls_color_tbl)
        With chrtf
            .bold = char_rtf.bold
            .f = char_rtf.f
            If char_rtf.fs = 0 And chrtf.fs = 0 Then 'abychom nevynulovali velikost písma...
                .fs = char_rtf.fs
            End If
            .italics = char_rtf.italics
            .ul = char_rtf.ul
            .uld = char_rtf.uld
            .ulth = char_rtf.ulth
            Dim rv As String
            If char_rtf.cf_defclr <> "" Then
                .cf = clrtbl.def_clr(char_rtf.cf_defclr)
            Else
                .cf = char_rtf.cf_index
            End If
            If char_rtf.highlight_defclr <> "" Then
                .highlight = clrtbl.def_clr(char_rtf.highlight_defclr)
            Else
                .highlight = char_rtf.highlight_index
            End If
        End With
    End Sub
    Public Sub New(n As Xml.XmlNode)
        Dim f As String
        new_(get_singlenode_value(n, "format"))
    End Sub

    Public Sub New(format_ As String)
        new_(format_)
    End Sub
    Private Sub new_(format_ As String)
        Dim f() As String
        format = format_
        f = Split(format_, ";")
        Dim i As Integer
        Dim fv() As String
        For i = 0 To UBound(f)
            'rgxx("ahoj:pse", "(ahoj):pse", fv)
            rgxx(f(i), "([a-z0-9\-]+)\:?([a-z0-9\(\)]*)$", fv)
            If fv(0) = "b" Then
                If fv(1) = "1" Then
                    char_rtf.bold = True
                End If
            ElseIf fv(0) = "i" Then
                If fv(1) = "1" Then
                    char_rtf.italics = True
                End If
            ElseIf fv(0) = "ul" Then
                If fv(1) = "1" Then
                    char_rtf.ul = True
                End If
            ElseIf fv(0) = "uld" Then
                If fv(1) = "1" Then
                    char_rtf.uld = True
                End If
            ElseIf fv(0) = "ulth" Then
                If fv(1) = "1" Then
                    char_rtf.ulth = True
                End If
            ElseIf fv(0) = "i" Then
                If fv(1) = "1" Then
                    char_rtf.italics = True
                End If
            ElseIf fv(0) = "f" Then
                char_rtf.f = fv(1)
            ElseIf fv(0) = "fs" Then
                char_rtf.fs = fv(1)
            ElseIf fv(0) = "fc-index" Then
                char_rtf.cf_index = fv(1)
            ElseIf fv(0) = "fc" Then
                char_rtf.cf_index = env.wsp.def_clrtbl.def_clr(fv(1))
                'char_rtf.cf_defclr = fv(1)
            ElseIf fv(0) = "bc-index" Then
                char_rtf.highlight_index = fv(1)
            ElseIf fv(0) = "bc" Then
                char_rtf.highlight_index = env.wsp.def_clrtbl.def_clr(fv(1))
                'char_rtf.highlight_defclr = fv(1)
            End If

        Next
    End Sub
    Public Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Dim n As Xml.XmlNode
        n = x.CreateNode(Xml.XmlNodeType.Element, "highlight_rule", "")
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "format", "")).InnerText = format
        Return n
    End Function
End Class

'####################################################################################################################################################################