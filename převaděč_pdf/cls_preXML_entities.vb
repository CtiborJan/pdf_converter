Imports System.Text.RegularExpressions
Imports System.Xml


Public Class cls_preXML_document
    'někdy se natvrdo přistupuje k vlastonstem section(0): tato sekce ale musí vždy existovat, pokud není sekce 0, není dokument. Ostatní sekce mají stejný počet
    'stran
    Public name As String
    Public path As String

    Public pdf_path As String

    Public n_sections As Integer

    Public Event page_added(inserted_after As Integer, id As String)
    Private pages_() As cls_preXML_section_page
    Public n_pages As Integer
    Public pdf_first_page As Integer
    Public pdf_pages_per_page As Integer

    Public Function new_page(Optional insert_on As Integer = -1, Optional id As String = "", Optional page_nr As String = "") As cls_preXML_section_page

        n_pages += 1

        If (n_pages Mod 10) = 0 Then
            create_backup()
        End If
        Dim new_page_index As Long
        ReDim Preserve pages_(n_pages)
        If insert_on = -1 Then 'přidání na konec
            pages_(n_pages) = New cls_preXML_section_page(Me)
            new_page_index = n_pages
        Else
            Dim i As Long
            For i = n_pages To insert_on + 1 Step -1
                pages_(i) = pages_(i - 1)
                pages_(i).m_index = i
                save_page(i)
            Next
            new_page_index = insert_on
            pages_(new_page_index) = New cls_preXML_section_page(Me, new_page_index)
        End If
        If page_nr = "" Then
            If new_page_index > 0 Then
                Dim last_used_page_nr As String
                Dim pos() As Point
                pos = pages_(new_page_index - 1).search(True, "<pb\s+n\s*=\s*['""]([0-9IVXLCDMivxlcdm]*)",,,, True)
                Dim prev_pos As Long
                ''Do While pos > 0
                'prev_pos = pos
                'pos = pages_(new_page_index - 1).search(True, "<pb\s+n\s*=\s*['""]([0-9IVXLCDMivxlcdm]*)", pos + 1,,, True)
                'Loop
                If pos IsNot Nothing Then
                    If pos.Length <> 0 Then
                        xyz = Mid(pages_(new_page_index - 1).plain_text, 8, 100)
                        last_used_page_nr = Mid(pages_(new_page_index - 1).plain_text, pos.Last.X + 1, (pos.Last.Y - pos.Last.X) + 1)
                        If last_used_page_nr <> "" Then
                            If rgxt(last_used_page_nr, "^[0-9]+$") = True Then
                                pages_(new_page_index).text_changed("<pb n='" & CInt(last_used_page_nr) + 1 & "'/>" & vbLf, 1)
                            ElseIf rgxt(last_used_page_nr, "^[IVXLCDMivxlcdm]+$") = True Then 'římské číslice
                                pages_(new_page_index).text_changed("<pb n='" & UCase(arab_to_roman(roman_to_arab(last_used_page_nr) + 1)) & "'/>" & vbLf, 1)
                            End If
                        End If
                    Else
                        pages_(new_page_index).text_changed("<pb n='?'/>" & vbLf, 1)
                    End If
                Else
                    pages_(new_page_index).text_changed("<pb n='?'/>" & vbLf, 1)
                End If
            Else
                pages_(new_page_index).text_changed("<pb n='?'/>" & vbLf, 1)
            End If
        Else
            pages_(new_page_index).text_changed("<pb n='" & page_nr & "'/>" & vbLf, 1)

        End If
        If id = "" Then id = new_page_index
        pages_(new_page_index).ID = id
        Return pages_(new_page_index)
        RaiseEvent page_added(insert_on, new_page_index)

    End Function
    Public Function page_get(Optional ByRef index As Integer = -1, Optional ByRef ID As String = "", Optional get_by_id As Boolean = False) As cls_preXML_section_page
        If get_by_id = False And index <> -1 Then
            If index > -1 And index <= n_pages Then
                ID = pages_(index).ID
                Return pages_(index)
            End If
        ElseIf get_by_id = True And ID <> "" Then
            Dim i As Integer
            For i = 0 To n_pages
                If pages_(i).ID = ID Then
                    index = i
                    Return pages_(i)
                End If
            Next
        End If
        ID = "" 'pokud jsme došli až sem, je něco špatně
        index = -1
    End Function
    Public Property page(index As Object) As cls_preXML_section_page
        Set(value As cls_preXML_section_page)
            If TypeOf index Is Long Or TypeOf index Is Integer Then
                If index > -1 And index <= n_pages Then
                    pages_(index) = value
                End If
            ElseIf TypeOf (index) Is String Then
                Dim i As Integer
                For i = 0 To n_pages
                    If pages_(i).ID = index Then
                        pages_(i) = value
                        Exit For
                    End If
                Next
            End If
        End Set
        Get
            If TypeOf index Is Long Or TypeOf index Is Integer Then
                If index > -1 And index <= n_pages Then
                    Return pages_(index)
                End If
            ElseIf TypeOf (index) Is String Then
                Dim i As Integer
                For i = 0 To n_pages
                    If pages_(i).ID = index Then
                        Return pages_(i)
                    End If
                Next
            End If
        End Get
    End Property
    Public Function delete_page(p_index As Long, move As Long) As Long
        'smažeme stránku
        If p_index > -1 And p_index <= Me.n_pages Then
            Dim i As Long
            For i = p_index To n_pages - 1
                pages_(i) = pages_(i + 1)
                pages_(i).m_index = i
                pages_(i).check_xml(True)
                Me.save_page(i)
            Next

            If My.Computer.FileSystem.FileExists(Me.path & "\strana_" & n_pages & ".preXML") Then
                My.Computer.FileSystem.DeleteFile(Me.path & "\strana_" & n_pages & ".preXML")
            End If
            n_pages -= 1
            If n_pages > -1 Then
                ReDim Preserve pages_(n_pages)
                If p_index <= n_pages Then Return p_index Else Return n_pages 'vrátíme následující stránku
            Else
                Erase pages_
                Return -1
            End If
        End If
    End Function
    Public Sub New()
        n_pages = -1
    End Sub
    Public Sub create_backup()
        'Exit Sub 'vypnuto, zatím to k ničemu nebylo, jenom to zdržuje
        If My.Computer.FileSystem.DirectoryExists(Me.path) = True Then
            If My.Computer.FileSystem.DirectoryExists(Me.path & "\zaloha") = False Then
                My.Computer.FileSystem.CreateDirectory(Me.path & "\zaloha")
            End If
            Dim d() As IO.DirectoryInfo
            d = My.Computer.FileSystem.GetDirectoryInfo(Me.path & "\zaloha").GetDirectories
            Dim i As Long
            Dim m As Long = 0
            Dim tmp As String
            If d IsNot Nothing Then
                For i = 0 To UBound(d)
                    tmp = rgx_g(d(i).Name, "z_([0-9]+)")
                    If tmp <> "" Then
                        If CInt(tmp) > m Then m = CInt(tmp)
                    End If
                Next
            End If
            My.Computer.FileSystem.CreateDirectory(Me.path & "\zaloha\z_" & m + 1)
            Dim f() As IO.FileInfo
            f = My.Computer.FileSystem.GetDirectoryInfo(Me.path).GetFiles
            If f IsNot Nothing Then
                For i = 0 To UBound(f)
                    If f(i).Extension = ".preXML" Or f(i).Name = "dokument.xml" Then
                        f(i).CopyTo(Me.path & "\zaloha\z_" & m + 1 & "\" & f(i).Name)
                    End If
                Next
            End If
        End If
    End Sub
    Public Sub load_pages()
        Dim i As Integer
        Dim j As Integer = -1
        Dim files() As String
        If My.Computer.FileSystem.DirectoryExists(Me.path) = True Then
            files = My.Computer.FileSystem.GetFiles(Me.path).ToArray
            If files IsNot Nothing Then
                For i = 0 To UBound(files)
                    If My.Computer.FileSystem.FileExists(Me.path & "\strana_" & i & ".preXML") And (i <= Me.n_pages Or Me.n_pages = -1) Then
                        Dim f As IO.StreamReader
                        f = My.Computer.FileSystem.OpenTextFileReader(Me.path & "\strana_" & i & ".preXML")
                        j += 1
                        Dim n As Integer = -1
                        Dim l As String
                        Do Until f.EndOfStream
                            l = f.ReadLine()
                            n += 1
                        Loop
                        Dim lns() As String
                        If n <> -1 Then
                            ReDim lns(n)
                            f.Close()
                            f = My.Computer.FileSystem.OpenTextFileReader(Me.path & "\strana_" & i & ".preXML")
                            n = 0
                            Do Until f.EndOfStream
                                lns(n) = f.ReadLine
                                n = n + 1
                            Loop

                            'n_pages += 1
                            ReDim Preserve pages_(i)
                            pages_(i) = New cls_preXML_section_page(Me, i)
                            pages_(i).load_from_preXML(lns)
                        End If

                        f.Close()

                        'pages_(n_pages).check_xml(True)
                    End If
                Next
                If n_pages > j Then
                    n_pages = j
                End If
                If n_pages = -1 Then n_pages = j
                For i = 0 To n_pages
                    pages_(i).check_xml(True)
                Next
            End If
        End If
    End Sub
    Public Sub save(Optional all As Boolean = True)
        If Me.path = "" Then
            Dim d As New FolderBrowserDialog
            d.ShowDialog()
            If d.SelectedPath <> "" Then
                Me.path = d.SelectedPath
            End If
        End If
        If Me.path <> "" And My.Computer.FileSystem.DirectoryExists(Me.path) Then
            Dim out As String
            out = "<doc>" & vbNewLine
            out &= "<name>" & Me.name & "</name>" & vbNewLine
            out &= "<path>" & Me.path & "</path>" & vbNewLine
            out &= "<pdf>" & Me.pdf_path & "</pdf>" & vbNewLine
            out &= "<n_pages>" & Me.n_pages & "</n_pages>" & vbNewLine
            out &= "<rtb_zoom>" & env.wsp.rtb.ZoomFactor & "</rtb_zoom>" & vbNewLine
            out &= "<workspace>" & Me.path & "\workspace.xml" & "</workspace>" & vbNewLine
            If env.opened_document IsNot Nothing Then
                If env.opened_document.name = Me.name Then
                    If env._p IsNot Nothing Then
                        out &= "<last_opened_page>" & env._p.m_index & "</last_opened_page>" & vbNewLine
                    End If
                End If
            End If
            out &= "<pdf_first_page>" & pdf_first_page & "</pdf_first_page>" & vbNewLine
            out &= "<pdf_pages_per_page>" & pdf_pages_per_page & "</pdf_pages_per_page>" & vbNewLine
            Dim pdf As AxAcroPDFLib.AxAcroPDF
            'out &= "<pdf_last_p>" & env.frm.get_pdf_control & "</pdf_last_p>" & vbNewLine
            out &= "</doc>"
            My.Computer.FileSystem.WriteAllText(Me.path & "\dokument.xml", out, False)

            Dim i As Long
            For i = 0 To Me.n_pages
                save_page(i)
            Next

            env.wsp.tm.export_to_xml.Save(Me.path & "\workspace.xml")
        End If
    End Sub
    Public Function export_to_xml() As String
        Dim i As Long


        If page(n_pages).XML_error <> 0 Then
            export_to_xml &= "<prexml-xmlerror_pg>" & page(n_pages).XML_error_on_page & "</prexml-xmlerror_pg>" & vbLf
        End If
        For i = 0 To n_pages
            export_to_xml &= vbNewLine & pages_(i).plain_text
        Next
        If page(n_pages).elements_left_opened IsNot Nothing Then
            For i = 0 To UBound(page(n_pages).elements_left_opened)
                export_to_xml &= vbLf & "</" & page(n_pages).elements_left_opened(i).name & ">"
            Next
        End If
        Return export_to_xml
    End Function
    Public Sub add_to_workspace(wpath As String)
        If My.Computer.FileSystem.FileExists(wpath) = True Then
            Dim xmldoc As Xml.XmlDocument
            xmldoc = New XmlDocument()
            Try
                xmldoc.Load(wpath)
                If xmldoc IsNot Nothing Then
                    env.wsp.tm.add_tools(xmldoc)
                End If
            Catch
                MsgBox(env.c("Nepodařilo se nahrát soubor. Jeho XML je zřejmě chybné."))
            End Try
        End If
    End Sub
    Public Sub load_my_workspace(Optional wpath As String = "")
        If wpath = "" Then
            If My.Computer.FileSystem.DirectoryExists(Me.path) = True Then
                If My.Computer.FileSystem.FileExists(Me.path & "\workspace.xml") Then
                    Dim xmldoc As Xml.XmlDocument
                    xmldoc = New XmlDocument()
                    xmldoc.Load(Me.path & "\workspace.xml")
                    If xmldoc IsNot Nothing Then
                        env.wsp.tm.dispose()
                        env.wsp.tm.destroy_controls()
                        env.wsp.tm = Nothing
                        env.wsp.tm = New cls_tools_manager(xmldoc, env.wsp)
                    End If
                End If
            End If
        Else
            If My.Computer.FileSystem.FileExists(wpath) Then
                Dim xmldoc As Xml.XmlDocument
                xmldoc = New XmlDocument()
                xmldoc.Load(wpath)
                If xmldoc IsNot Nothing Then
                    env.wsp.tm.dispose()
                    env.wsp.tm.destroy_controls()
                    env.wsp.tm = Nothing
                    env.wsp.tm = New cls_tools_manager(xmldoc, env.wsp)
                End If
            End If
        End If
    End Sub
    Public Function save_page(nr As Integer) As Boolean
        If Me.path <> "" And My.Computer.FileSystem.DirectoryExists(Me.path) Then
            Dim t As String
            If Me.pages_(nr) IsNot Nothing Then
                t = page(nr).preXML_to_save()
                My.Computer.FileSystem.WriteAllText(Me.path & "\strana_" & nr & ".preXML", t, False)
                Return True
            End If
        Else
                'MsgBox("Cesta pro uložení je neplatná.")
                Return False
        End If
    End Function

    Public Function join_pages(first_page As Integer, second_page As Integer)

        If first_page > -1 And second_page <= n_pages Then
            Dim i As Long
            pages_(first_page).text_changed(pages_(second_page))
            n_pages -= 1
            For i = second_page To n_pages
                pages_(i) = pages_(i + 1)
                pages_(i).m_index = i
                pages_(i).check_xml(True)
            Next
            ReDim Preserve pages_(n_pages)
        End If
    End Function

    Public Sub search_and_highlight(search As String, regexp As Boolean)
        Dim i As Long
        For i = 0 To Me.n_pages
            Me.page(i).search_and_highlight(regexp, search)
        Next
    End Sub
    Public Sub search_and_replace(search As String, replacement As String, regexp As Boolean)
        Dim i As Long
        For i = 0 To Me.n_pages
            Me.page(i).search_and_replace(regexp, search, replacement)
        Next
    End Sub
End Class
Public Class cls_preXML_context_evaluator

End Class
Public Structure cls_preXML_attribute
    Public name As String
    Public value As String
    Public tag As cls_preXML_tag
    Public Sub New(name_ As String, value_ As String, tag_ As cls_preXML_tag)
        name = name_
        value = value_
        tag = tag_
    End Sub
    Public Overrides Function ToString() As String
        Return name & "=""" & value & ""
    End Function
End Structure
Public Class cls_preXML_tag
    Public name As String
    Public attributes() As cls_preXML_attribute
    Public n_attributes As Long
    Public self_closing As Boolean
    Public position As Point
    Public parent As cls_preXML_tag
    Public second_to_pair As cls_preXML_tag
    Public Sub New()
        n_attributes = -1
    End Sub
    Public Sub New(name_ As String, parent_ As cls_preXML_tag, position_ As Point, self_closing_ As Boolean)
        parent = parent_
        name = name_
        position = position_
        self_closing = self_closing_
        n_attributes = -1
    End Sub
    Shared Operator <>(v1 As String, v2 As cls_preXML_tag)

    End Operator
    Shared Operator <>(v1 As cls_preXML_tag, v2 As String)

    End Operator
    Public Shared Operator =(v1 As cls_preXML_tag, v2 As String)
        Return op_Equality(v2, v1)
    End Operator
    Public Shared Operator =(v1 As String, v2 As cls_preXML_tag)
        If Left(v1, 1) = "@" Then
            Dim attr_name As String
            Dim attr_value As String
            attr_name = rgx_g(v1, "^@([^\s=!><]+)")
            attr_value = rgx_g(v1, "[""']([^""']*)[""']")
            For i = 0 To v2.n_attributes
                If v2.attributes(i).name = attr_name Then
                    If attr_value = v2.attributes(i).value Then Return True
                End If
            Next
        End If
    End Operator

    Public Sub add_attribute(attr_name As String, attr_value As String)
        Dim i As Long
        For i = 0 To Me.n_attributes
            If Me.attributes(i).name = attr_name Then
                Me.attributes(i).value = attr_value
                Exit Sub
            End If
        Next
        n_attributes += 1
        ReDim Preserve attributes(n_attributes)
        attributes(n_attributes) = New cls_preXML_attribute(attr_name, attr_value, Me)
    End Sub
    Public Function has_attribute_with_value(attr_name As String, attr_value As String) As Boolean
        Dim i As Long
        For i = 0 To n_attributes
            If attributes(i).name = attr_name Then
                If attributes(i).value = attr_value Then
                    Return True
                End If
            End If
        Next
    End Function
    Public Function has_attribute(attr_name As String) As Boolean
        Dim i As Long
        For i = 0 To n_attributes
            If attributes(i).name = attr_name Then
                Return True
            End If
        Next
    End Function
    Public Function get_attribute_value(attr_name As String) As String
        Dim i As Long
        For i = 0 To n_attributes
            If attributes(i).name = attr_name Then
                Return attributes(i).value
            End If
        Next
    End Function
    Public Sub write_out(p As cls_preXML_section_page)
        If p IsNot Nothing Then
            Dim ei As Integer
            If Me.position.Y > 0 Then
                ei = InStr(Me.position.Y, p.plain_text, ">")
                If ei <> 0 Then
                    p.delete_text_on_position(Me.position.Y, ei)
                    p.insert_on_position(Me.position.Y, Me.ToString, "")
                    p.context.get_context()
                End If
            End If
        End If
    End Sub
    Public Overrides Function ToString() As String
        Dim i As Long
        Dim str As String
        str = "<" & Me.name
        For i = 0 To Me.n_attributes
            str = str & " " & Me.attributes(i).name & "=" & Chr(34) & Me.attributes(i).value & Chr(34)
        Next
        If Me.self_closing = True Then
            str &= "/>"
        Else
            str &= ">"
        End If
        Return str
    End Function

End Class
Public Class cls_preXML_context
    Private marks_() As String
    Private opened_tags() As cls_preXML_tag
    Private opened_tags_indices() As Point
    Private n_opened As Integer
    Private inside_tag As String
    Private inside_tag_name As String
    Private n_marks As Integer
    Private p As cls_preXML_section_page
    Public flt As cls_flyingtool
    Public word As String
    Public word_boundaries1b As Point
    Public ReadOnly Property tags_opened(index As Integer) As cls_preXML_tag
        Get
            If opened_tags IsNot Nothing Then
                If index > -1 And index <= n_opened Then Return opened_tags(index)
            End If
        End Get
    End Property
    Public ReadOnly Property n_tags_opened() As Integer
        Get
            Return n_opened
        End Get
    End Property

    Public Function inside_of_tag() As String
        Return inside_tag
    End Function
    Public Function inside_of_tag_name() As String
        Return inside_tag_name
    End Function
    Public Function opened_element(t As String, Optional ByRef pagenr As Integer = -1, Optional ByRef index As Integer = -1) As Boolean
        'zjistí, je-li otevřený element t
        If opened_tags IsNot Nothing Then

            Dim i As Long
            For i = 0 To UBound(opened_tags)
                If opened_tags(i).name = t Then

                    pagenr = opened_tags(i).position.X
                    index = opened_tags(i).position.Y

                    Return True
                End If
            Next
        End If
    End Function
    Public Function last_opened_element(Optional ByRef pagenr As Integer = -1, Optional ByRef index As Integer = -1, Optional ByRef attr() As cls_preXML_attribute = Nothing) As String
        If opened_tags IsNot Nothing And n_opened <> -1 Then

            pagenr = opened_tags(n_opened).position.X
            index = opened_tags(n_opened).position.Y

            attr = opened_tags(n_tags_opened).attributes
            Return opened_tags(n_opened).name
        End If
    End Function

    Public Function mark(m As String) As Boolean
        If marks_ IsNot Nothing Then
            Return marks_.Contains(m)
        End If
    End Function
    Public Function marks() As String()
        If marks_ IsNot Nothing Then
            Return marks_
        End If
    End Function
    Public Sub New(p_ As cls_preXML_section_page)
        p = p_
        n_opened = -1
    End Sub
    Public Sub reset_flying_tool(parent As Object, description As String, value As Object, value2 As Object)
        flt.description = description
        If value IsNot Nothing Then flt.value = value
        If value2 IsNot Nothing Then flt.value2 = value2
        If parent IsNot Nothing Then flt.tool = parent
        If env.wsp.flti IsNot Nothing Then
            env.wsp.flti.Text = description

        End If
    End Sub

    Public Sub set_flying_tool(flt_ As cls_flyingtool)
        flt = flt_
        If env.wsp.flti Is Nothing Then
            env.wsp.flti = New Label
            env.wsp.flti.Parent = env.wsp.rtb.Parent
        End If
        If flt Is Nothing Then
            env.wsp.flti.Visible = False
        Else
            env.wsp.flti.Parent = env.wsp.rtb.Parent
            env.wsp.flti.Visible = True
            env.wsp.flti.Text = flt.description
            env.wsp.flti.BackColor = Color.LightBlue
            env.wsp.flti.MaximumSize = New Size(500, 250)
            env.wsp.flti.AutoSize = True
            'If env.wsp.flti.Width > 250 Then

            'End If
            env.wsp.flti.BringToFront()
        End If
    End Sub
    Public Sub get_context()
        Dim t As Long = Environment.TickCount()
        n_opened = -1
        If p.SelStart0b >= 0 And p.SelStart0b <= Len(p.plain_text) Then
            If p.SelStart0b < Len(p.plain_text) Then marks_ = p.meta_data(p.SelStart0b)
            Dim si As Integer
            Dim ei As Integer
            Dim ts As Integer
            Dim prev_te As Integer
            Dim te As Integer
            Dim inside_of_closing_tag As Boolean
            xyz = Mid(p.plain_text, p.SelStart1b, 100)
            If p.SelStart1b > 1 Then
                ts = InStrRev(p.plain_text, "<", p.SelStart1b - 1) 'začátek posledního předcházejícího tagu
                prev_te = InStrRev(p.plain_text, ">", p.SelStart1b - 1) 'poslední předcházející konec 
                te = InStr(p.SelStart1b - 1, p.plain_text, ">")
            End If
            If ts <> 0 Then
                If prev_te <> 0 And prev_te > ts Then 'nejsme uvnitř tagu (aspoň podle pravidel...)
                    inside_tag = ""
                    inside_tag_name = ""
                    inside_of_closing_tag = False
                Else
                    ei = InStr_first(ts + 1, p.plain_text, 0, 0, ">")  'pokud jsme v neuzavřeném tagu na konci stránky, nastala by chyba
                    If ei <> -1 Then
                        xyz = Mid(p.plain_text, ei, 100)
                        If ei > 0 Then
                            inside_tag = (Mid(p.plain_text, ts + 1, (ei - ts - 1)))
                            If InStr(1, inside_tag, "/") = 1 Then inside_of_closing_tag = True
                            inside_tag_name = rgx_g(inside_tag, "/?([^/ >]+)")
                        End If
                    Else
                        inside_tag = "?"
                        inside_tag_name = "?"
                        Me.p.XML_error = Me.p.XML_ERRORS.NONCLOSED_TAG
                    End If
                End If
            Else
                inside_tag = ""
                inside_tag_name = ""
            End If

            Dim tag As String
            Dim tname As String
            n_opened = -1
            Erase opened_tags
            opened_tags = Me.p.elements_found_opened
            If opened_tags IsNot Nothing Then 'musíme počítat i s elementy otevřenými na předchozí straně!
                n_opened = UBound(opened_tags)
            End If
            Do While InStrX(si + 1, p.plain_text, "<", si) <> 0 And si <= p.SelStart0b
                ei = InStr(CInt(si), p.plain_text, ">")
                If ei <> 0 Then
                    tag = Mid(p.plain_text, si, ei + 1 - si)
                    If Left(tag, 2) = "</" = True And ei <= p.SelStart0b Then 'zavírací tag
                        tname = rgx_g(tag, "</([^/> ]+)")
                        If n_opened > -1 Then
                            If tname = opened_tags(n_opened).name Then 'poslední otevřený tag se shoduje s právě zavíraným, je to v pořádku
                                n_opened -= 1
                                If n_opened > -1 Then
                                    ReDim Preserve opened_tags(n_opened)
                                    'opened_tags(n_opened) = New cls_preXML_tag
                                Else
                                    Erase opened_tags
                                End If
                            End If
                        End If
                    ElseIf Right(tag, 2) <> "/>" And Left(tag, 2) <> "</" Then 'předchozí podmínka může být vyhodnocena jako false i v případě, že jsme právě v zavíracím tagu 
                        'tady teda nesmíme otvírat další, prázdný tag
                        Dim x As Xml.XmlDocument = New XmlDocument
                        tname = rgx_g(tag, "<([^/> ]+)")
                        'On Error Resume Next
                        Try
                            x.InnerXml = tag & "</" & tname & ">"
                        Catch
                        End Try
                        n_opened += 1
                        ReDim Preserve opened_tags(n_opened)
                        opened_tags(n_opened) = New cls_preXML_tag
                        opened_tags(n_opened).name = tname
                        opened_tags(n_opened).position.X = Me.p.m_index
                        opened_tags(n_opened).position.Y = si
                        Dim i As Long
                        If x IsNot Nothing And x.InnerXml <> "" Then
                            If x.FirstChild.Attributes IsNot Nothing Then
                                For i = 0 To x.FirstChild.Attributes.Count - 1
                                    opened_tags(n_opened).add_attribute(x.FirstChild.Attributes.Item(i).Name, x.FirstChild.Attributes.Item(i).Value)
                                Next i
                            End If
                        End If
                    End If
                End If
            Loop
            'teď musíme ještě najít zavírací tag posledního otevřeného elementu...
            Dim lo_el_name_opened() As cls_preXML_tag
            Dim n_loeno As Integer = -1
            Dim tags_to_close As Integer = 1
            Dim pi As Integer
            pi = Me.p.m_index_
            Dim li As Integer
            Dim fi As Integer
            li = p.SelStart1b
            fi = li
            Do While tags_to_close <> 0 And pi <= p.parent_d.n_pages
                '    Exit Do
                If fi = -1 Then 'na aktuální stránce už dál nic není
                    pi += 1
                    li = 0
                    fi = 0
                    Continue Do 'skočíme na další
                End If
                'najdeme, další otvírací a další zavírací tag
                If opened_tags IsNot Nothing Then
                    si = InStr_first(fi + 1, p.parent_d.page(pi).plain_text, si, 0, "<" & opened_tags.Last.name & " ", "<" & opened_tags.Last.name & ">")
                    ei = InStr_first(fi + 1, p.parent_d.page(pi).plain_text, ei, 0, "</" & opened_tags.Last.name & " ", "</" & opened_tags.Last.name & ">")
                Else
                    si = -1
                    ei = -1
                End If

                If ei = -1 And si = -1 Then 'pokud na stránce není ani jeden, zase se posuneme na další
                    pi += 1
                    li = 0
                    fi = 0
                    Continue Do
                End If
                If si > 0 Then xyz = Mid(p.parent_d.page(pi).plain_text, si, 100)
                If ei > 0 Then xyz = Mid(p.parent_d.page(pi).plain_text, ei, 100)
                'pokud na stránce něco je, 
                fi = si
                If ei < fi Or fi = -1 Then fi = ei 'uložíme si index, na kterém to další něco začíná

                If si < ei And si <> -1 Or (ei = -1 And si <> -1) Then 'pokud je dřív otvírací tag
                    tags_to_close += 1 'musíme pak zavřít o jeden víc
                ElseIf ei < si And ei <> -1 Or (si = -1 And ei <> -1) Then 'dřív je zavírací tag...
                    tags_to_close -= 1
                End If

                If tags_to_close = 0 And opened_tags IsNot Nothing Then 'zavřeli jsme poslední, který jsme potřebovali... jsem s hledáním u konce
                    opened_tags.Last.second_to_pair = New cls_preXML_tag(opened_tags.Last.name, opened_tags.Last.parent, New Point(pi, ei), False)
                    Exit Do 'našli jsme zavírací tag, vyskočíme
                End If

                'si = InStr_first(li + 1, p.parent_d.page(pi).plain_text, si, 0, "<" & opened_tags.Last.name & " ", "<" & opened_tags.Last.name & ">")
                'ei = InStr_first(li + 1, p.parent_d.page(pi).plain_text, ei, 0, "</" & opened_tags.Last.name & " ", "</" & opened_tags.Last.name & ">")

            Loop

            'slovo, v němž nebo u nějž se nachází kurzor
            If p.SelLength > 0 And p.SelLength < 30 Then
                word = Trim(Mid(p.plain_text, p.SelStart1b, p.SelLength))
                word_boundaries1b.X = p.SelStart1b
                word_boundaries1b.Y = p.SelStart1b + p.SelLength
            ElseIf p.SelLength = 0 Then
                ei = InStr_first(p.SelStart1b, p.plain_text, 0, 0, " ", vbLf, "<", ">")
                If ei = -1 Then ei = Len(p.plain_text) + 1
                si = InStrRev_first(p.SelStart1b, p.plain_text, 0, 0, " ", vbLf, "<", ">") + 1
                If si < 1 Then si = 1
                word = ""
                If ei > si Then
                    word = Trim(Mid(p.plain_text, si, ei - si))
                    Dim m As Match

                    word = rgx(word, "\b(\w)*\b",, m)
                    If m IsNot Nothing Then
                        word_boundaries1b.X = m.Index + si
                        word_boundaries1b.Y = m.Index + m.Length + si
                    Else
                        word_boundaries1b.X = 0
                        word_boundaries1b.Y = 0
                    End If
                End If
            Else
                word = ""
            End If
        End If
        'env.wsp.highlight_tags()
        t = Environment.TickCount - t
        frm_main.Text = "get_context: " & t
    End Sub
End Class
'#########################################################################################################################################################
'#########################################################################################################################################################

Public Class cls_preXML_section_page 'sekce
    Public Event xml_checked(no_rendering As Boolean)

    Public elements_found_opened() As cls_preXML_tag
    Public elements_left_opened() As cls_preXML_tag

    Public force_SelStart As Integer
    Public force_SelLength As Integer

    Private Class subcls_prev_state
        Public max As Integer = 1
        Public previous_state() As cls_preXML_section_page
        Public n As Long
        Private p As cls_preXML_section_page
        Public Sub New(max_1b As Long, p_ As cls_preXML_section_page)
            max = max_1b
            p = p_
            ReDim previous_state(max - 1)
        End Sub
        Public Sub add()
            Dim tmp As cls_preXML_section_page
            tmp = New cls_preXML_section_page(p)
            Dim i As Long
            For i = max - 2 To 0 Step -1
                previous_state(i + 1) = previous_state(i)
            Next
            previous_state(0) = tmp
        End Sub
        Public Sub restore_last()
            If previous_state(0) IsNot Nothing Then
                p.restore(previous_state(0))
            End If
            Dim i As Long
            For i = 0 To max - 2
                previous_state(i) = previous_state(i + 1)
            Next
            previous_state(max - 1) = Nothing
        End Sub
    End Class
    Private p_state As subcls_prev_state
    Public Sub one_step_back()
        p_state.restore_last()
    End Sub
    Public Sub save_state()
        p_state.add()
    End Sub
    Friend Function restore(from_p As cls_preXML_section_page)
        Me.plain_text_ = from_p.plain_text_
        Dim li As Integer
        If from_p.meta_data IsNot Nothing Then
            Me.meta_data = from_p.copy_metadata_section(0, UBound(from_p.meta_data))
            Me.pt_selstart = from_p.pt_selstart
            Me.pt_sellength = from_p.pt_sellength
            Me.pt_prev_sellength = from_p.pt_prev_sellength
            Me.pt_prev_selstart = from_p.pt_prev_selstart
            Me.calculate_lines()
        End If
        env.wsp.display_page(Nothing, Nothing,,, 10)
    End Function
    Public Enum XML_ERRORS
            NO_ERROR
            BAD_CLOSING_TAG
            NONCLOSED_TAG
            NONCLOSED_ELEMENT
            ERROR_ON_PREVIOUS_PAGE
        End Enum
        Public XML_error As Integer
    Public XML_error_on_page As Integer

    Public Sub insert_text_from_clipboard(mode As Integer)
        Dim txt As String
        Dim rtf As String
        Dim md()() As String
        If Clipboard.ContainsText(TextDataFormat.Rtf) = True And env.wsp.textformat = TextDataFormat.Rtf Then
            rtf = env.wsp.rtf_to_prepreXML(Clipboard.GetText(TextDataFormat.Rtf), md)

            Me.text_changed(rtf, md, mode)

        Else
            txt = Clipboard.GetText(TextDataFormat.Text)
            txt = Replace(txt, "<", "&lt;")
            txt = Replace(txt, ">", "&gt;")

            Me.text_changed(txt, mode)
        End If
    End Sub

    Public Sub get_Opened_elements()
            If Me.m_index <> 0 Then
                Me.elements_found_opened = Me.parent_d.page(Me.m_index - 1).elements_left_opened
            End If
        End Sub
        Public Function check_xml(Optional no_rendering As Boolean = False)
            Dim i As Integer
            If Me.m_index <> 0 Then
                If Me.parent_d.page(Me.m_index - 1).XML_error <> 0 Then
                    Me.XML_error = XML_ERRORS.ERROR_ON_PREVIOUS_PAGE
                    Dim error_found As Boolean = False
                    For i = Me.m_index To 0 Step -1
                        If Me.parent_d.page(i).XML_error <> 0 And Me.parent_d.page(i).XML_error <> XML_ERRORS.ERROR_ON_PREVIOUS_PAGE Then
                            XML_error_on_page = i
                            error_found = True
                            Exit For
                        End If
                    Next
                    If error_found = False Then
                        For i = 0 To Me.m_index
                            Me.parent_d.page(i).check_xml(False)
                        Next
                        GoTo check_xml
                    End If
                    RaiseEvent xml_checked(no_rendering)
                    Exit Function
                End If
            End If
check_xml:
            XML_error = 0
            remove_mark_on_position("~BAD_CLOSING_TAG", 0, Len(plain_text_) - 1)
            remove_mark_on_position("~TAG_NOT_CLOSED", 0, Len(plain_text_) - 1)

            Dim opened_tags() As cls_preXML_tag
            Dim n_opened As Integer = -1
            If Me.m_index <> 0 Then
                elements_found_opened = Me.parent_d.page(Me.m_index - 1).elements_left_opened
            End If
            If elements_found_opened IsNot Nothing Then
                n_opened = UBound(elements_found_opened)
                ReDim opened_tags(n_opened)
                For i = 0 To n_opened
                    opened_tags(i) = elements_found_opened(i)
                Next
            End If

            Dim si As Integer
            Dim ei As Integer
            Dim nsi As Integer
            Dim tag As String
            Dim tname As String
            Do While InStrX(si + 1, plain_text_, "<", si) <> 0
                ei = InStr(CInt(si), plain_text_, ">")
                nsi = InStr(CInt(si + 1), plain_text_, "<")
                If ei <> 0 And (ei < nsi Or nsi = 0) Then 'pokud dřív než konec tagu nenásleduje začátek nějakého nového
                    tag = Mid(plain_text_, si, ei + 1 - si)
                    If rgxt(tag, "<[^>]+/>") = True Then 'samozavírací tag: ten nás dál zajímat nebude

                    ElseIf rgxt(tag, "^</") = True Then 'zavírací tag
                        tname = rgx_g(tag, "</([^/> ]+)")
                        If opened_tags IsNot Nothing Then
                            If tname = opened_tags(n_opened).name Then 'poslední otevřený tag se shoduje s právě zavíraným, je to v pořádku
                                n_opened -= 1
                                If n_opened > -1 Then
                                    ReDim Preserve opened_tags(n_opened)
                                Else
                                    Erase opened_tags
                                End If
                            Else
                                add_metadata_to_section("~BAD_CLOSING_TAG", si - 1, ei - 1, True)
                                If opened_tags(n_opened).position.Y > -1 Then '-1 znamená, že je to někde na předchozích stránkách
                                    add_metadata_to_section("~BAD_CLOSING_TAG", opened_tags(n_opened).position.Y - 1, opened_tags(n_opened).position.Y + Len(opened_tags(n_opened).name), True)
                                End If
                                XML_error = XML_ERRORS.BAD_CLOSING_TAG
                                Exit Do
                            End If
                        Else 'tagy se neshodují - něco je špatně
                            add_metadata_to_section("~BAD_CLOSING_TAG", si - 1, ei - 1, True)
                            XML_error = XML_ERRORS.BAD_CLOSING_TAG
                            Exit Do
                        End If

                    Else 'otvírací tag
                        Dim x As Xml.XmlDocument = New XmlDocument
                        tname = rgx_g(tag, "<([^/> ]+)")
                        On Error Resume Next
                        x.InnerXml = tag & "</" & tname & ">"

                        n_opened += 1
                        ReDim Preserve opened_tags(n_opened)
                        opened_tags(n_opened) = New cls_preXML_tag
                        opened_tags(n_opened).name = tname
                        opened_tags(n_opened).position.X = Me.m_index
                        opened_tags(n_opened).position.Y = si
                        If x IsNot Nothing And x.InnerXml <> "" Then
                            For i = 0 To x.FirstChild.Attributes.Count - 1
                                opened_tags(n_opened).add_attribute(x.FirstChild.Attributes.Item(i).Name, x.FirstChild.Attributes.Item(i).Value)
                            Next i
                        End If
                    End If
                Else 'nenašli jsme koncovou závorku tagu ->chyba (neuzavřený tag)
                    If nsi = 0 Then nsi = Len(plain_text_) - 1
                    add_metadata_to_section("~TAG_NOT_CLOSED", si - 1, nsi - 1, True)
                    XML_error = XML_ERRORS.NONCLOSED_TAG
                    Exit Do
                End If
            Loop
            elements_left_opened = opened_tags
            RaiseEvent xml_checked(no_rendering)
        End Function

    Private plain_text_ As String
    Private pt_prev_selstart As Integer
    Private pt_prev_sellength As Integer
    Private pt_selstart As Integer
    Private pt_sellength As Integer

    Public page_rtf As String
    Private lines_starting_indices() As Integer
    Private lines_() As String

    Public n_lines As Integer
    Public meta_data()() As String
    Public parent_d As cls_preXML_document

    Public Event PlainText_changed(ByRef pp As cls_preXML_section_page)

    Public m_index_ As Integer 'index této sekce v nadřazeném objektu

    Public ID As String
    Public context As cls_preXML_context

    Private marks_() As String
    Private n_marks
    Public ReadOnly Property marks() As String()
        Get
            Return marks_
        End Get
    End Property

    Private type_ As String
    Public ReadOnly Property type As String
        Get
            Return type_
        End Get
    End Property
    Public Property m_index As Integer
        Get
            Return m_index_
        End Get
        Set(value As Integer)
            Me.m_index_ = value
            Me.parent_d.save_page(m_index)
        End Set

    End Property
    Public Sub New(p As cls_preXML_section_page)
        parent_d = Nothing
        type_ = "PREVSTATE"
        pt_selstart = p.SelStart0b
        pt_sellength = p.SelLength
        pt_prev_sellength = p.SelStart_prev
        pt_prev_selstart = p.SelStart_prev
        Me.plain_text_ = p.plain_text
        Dim li As Integer = -1
        If p.meta_data IsNot Nothing Then
            li = UBound(p.meta_data)
            Me.meta_data = p.copy_metadata_section(0, li)
        End If
    End Sub
    Public Sub New(parent As cls_preXML_document, Optional mindex As Long = -1)
            'n_elements = -1
            parent_d = parent

            n_lines = -1
            plain_text_ = ""

        context = New cls_preXML_context(Me)
        p_state = New subcls_prev_state(10, Me)
        If mindex = -1 Then
            m_index_ = parent.n_pages
        Else
            m_index_ = mindex
        End If

    End Sub

    Public Function get_all_marks() As String()
        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim add As Boolean
        n_marks = -1
        Erase marks_
        If meta_data IsNot Nothing Then
            For i = 0 To Len(plain_text_) - 1
                If meta_data(i) IsNot Nothing Then
                    For j = 0 To UBound(meta_data(i))
                        If marks_ IsNot Nothing Then
                            If marks_.Contains(meta_data(i)(j)) Then
                                Continue For
                            Else
                                n_marks += 1
                                ReDim Preserve marks_(n_marks)
                                marks_(n_marks) = meta_data(i)(j)
                            End If
                        Else
                            n_marks += 1
                            ReDim Preserve marks_(n_marks)
                            marks_(n_marks) = meta_data(i)(j)
                        End If

                    Next j

                End If
            Next i
        End If
        Return marks_
    End Function
    Public Function safe_metadata(index) As String()
        If Me.meta_data(index) Is Nothing Then
            Dim tmp(0) As String
            Return tmp
        Else
            Return meta_data(index)
        End If
    End Function
    Public Function search(rgx As Boolean, searchtxt As String, Optional ByVal start_at As Integer = 0, Optional ByVal end_at As Integer = 0,
                           Optional ignore_case As Boolean = False, Optional rgx_get_capture As Boolean = False) As Point()
        Dim i As Long = start_at
        Dim n As Long
        Dim real_n As Long = -1
        Dim tmp() As Point
        If end_at = 0 Then end_at = Len(Me.plain_text_)

        If rgx = False Then
            n = 100
            ReDim tmp(n)
            Do While InStrX(i + 1, Me.plain_text_, searchtxt, i)
                real_n += 1
                If real_n > n Then
                    n += 100
                    ReDim Preserve tmp(n)
                End If
                tmp(real_n).X = i
                i = i + Len(searchtxt) - 1
                tmp(real_n).Y = i
                If i > end_at Then Exit Do
            Loop
        Else
            Dim mc As MatchCollection
            Dim rx As Regex
            On Error GoTo Err
            rx = New Regex(searchtxt)
            mc = rx.Matches(plain_text_, start_at)
            If end_at <> 0 Then
                n = 100
                ReDim tmp(n)
                Dim index As Integer
                Dim length As Integer
                For i = 0 To mc.Count - 1
                    If rgx_get_capture = False Then
                        index = mc.Item(i).Index
                        length = mc.Item(i).Length
                    ElseIf mc.Item(i).Groups IsNot Nothing Then
                        If mc.Item(i).Groups(1).Length <> 0 Then
                            index = mc.Item(i).Groups(1).Index
                            length = mc.Item(i).Groups(1).Length
                        Else
                            Continue For
                        End If
                    Else
                        Continue For
                    End If
                    If index + length < end_at Then
                        real_n += 1
                        If real_n > n Then
                            n += 100
                            ReDim Preserve tmp(n)
                        End If
                        tmp(real_n).X = index
                        tmp(real_n).Y = index + length - 1
                    End If
                Next
            Else
                If mc.Count > 0 Then
                    ReDim tmp(mc.Count - 1)
                    real_n = mc.Count - 1
                    For i = 0 To mc.Count - 1
                        If rgx_get_capture = False Then
                            tmp(i).X = mc.Item(i).Index
                            tmp(i).Y = mc.Item(i).Index + mc.Item(i).Length - 1
                        ElseIf mc.Item(i).Groups IsNot Nothing Then
                            tmp(i).X = mc.Item(i).Groups(1).Index
                            tmp(i).Y = mc.Item(i).Groups(1).Index + mc.Item(i).Groups(1).Length - 1
                        Else
                            Return Nothing
                        End If
                    Next
                Else
                    Return Nothing

                End If
            End If
        End If
        ReDim Preserve tmp(real_n)
Err:
        Return tmp
    End Function
    Public Function search_first_marked_section0b(mark As String, Optional s_index0b As Integer = 0, Optional e_index0b As Integer = -1) As Point
        'najde první úsek za zadaným s_index0b který obsahuje zadanou značku
        Dim i As Long, j As Long
        Dim tmp As Point = New Point(-1, -1)
        If Me.meta_data IsNot Nothing Then
            If e_index0b > UBound(Me.meta_data) Or e_index0b = -1 Then e_index0b = UBound(Me.meta_data)

            If s_index0b < UBound(Me.meta_data) Then
                For i = s_index0b To e_index0b
                    If meta_data(i) IsNot Nothing Then
                        If meta_data(i).Contains(mark) = True Then
                            tmp.X = i
                            tmp.Y = e_index0b
                            For j = i To e_index0b
                                If meta_data(j) IsNot Nothing Then
                                    If meta_data(j).Contains(mark) = False Then
                                        tmp.Y = j - 1
                                        Return tmp
                                    End If
                                Else
                                    tmp.Y = j - 1
                                    Return tmp
                                End If
                            Next
                        End If
                    End If
                Next
            End If

        End If
    End Function

    Public Sub search_and_highlight(rgx As Boolean, searchtxt As String, Optional mark As String = "~search", Optional remove_marks As Boolean = True)
            Dim i As Integer, j As Integer, k As Integer

            Dim new_size As Integer
            Dim arr_size As Integer
            If remove_marks = True And meta_data IsNot Nothing Then 'odstraníme všechny předchozí označení zadanou značkou
                For i = 0 To UBound(meta_data)
                    If Not meta_data(i) Is Nothing Then
                        new_size = UBound(meta_data(i))
                        j = 0
                        Do While j <= new_size
                            If Left(meta_data(i)(j), Len(mark) + 1) = mark & " " Or meta_data(i)(j) = mark Then
                                For k = j To meta_data(i).Count - 2
                                    meta_data(i)(k) = meta_data(i)(k + 1)
                                Next
                                new_size = meta_data(i).Count - 2
                                If new_size > -1 Then
                                    ReDim Preserve meta_data(i)(new_size)
                                Else
                                    Erase meta_data(i)
                                End If
                            End If
                            j = j + 1
                        Loop
                    End If
                Next
            End If

            i = 0
            If rgx = False Then
                'vyhledávání prostého textu
                Do
                    If InStrX(i + 1, plain_text_, searchtxt, i) <> 0 Then
                        For j = i To i + Len(searchtxt) - 1
                            add_char_metadata_value(mark, j - 1, True)
                        Next
                        i = j
                    End If
                Loop While i <> 0
            ElseIf rgx = True Then
                'vyhledávání podle regulérních výrazů
                Dim rv() As String
                Dim rmc As MatchCollection
                Dim l As Integer
                rgxx(plain_text_, searchtxt, rv, True,, rmc)
                If Not rmc Is Nothing Then
                    For i = 1 To rmc.Count
                        For j = rmc.Item(i - 1).Index To rmc.Item(i - 1).Index + rmc.Item(i - 1).Length - 1
                            add_char_metadata_value(mark, j, True)

                        Next
                        For j = 1 To rmc.Item(i - 1).Groups.Count - 1 'tady jsou zachycené sekvence - pod indexem 0 je ale celý zachycený výraz
                            For k = rmc.Item(i - 1).Groups.Item(CInt(j)).Index To rmc.Item(i - 1).Groups.Item(CInt(j)).Index + rmc.Item(i - 1).Groups.Item(CInt(j)).Length - 1
                                add_char_metadata_value(mark & "_capture", k, True)
                            Next
                        Next
                    Next i
                End If
            End If
        End Sub

    Public Sub search_and_replace(rgx As Boolean, searchtxt As String, replacementtxt As String,
                                      Optional mark As String = "~replaced", Optional remove_marks As Boolean = True,
                                      Optional start_at As Integer = 0, Optional n_replacements As Integer = -1,
                                      Optional lenght As Long = -1, Optional exclude_tags As Boolean = False)

        Dim i As Integer, j As Integer, k As Integer, l As Integer
        Dim n As Integer
        Dim searchtxt_len As Integer
        Dim replacement_len As Integer
        'Dim tmp As String
        'Dim tmp_md() As String
        Dim tmp As String
        Dim tmp2 As String
        Dim tmp_md()() As String
        Dim tmp_md2() As String

        If rgx = False Then
            searchtxt_len = Len(searchtxt)
            replacement_len = Len(replacementtxt)

            j = 1
            i = start_at
            l = 0
            tmp = ""
            Do While InStrX(i + 1, plain_text_, searchtxt, i) <> 0
                If lenght <> -1 And i - start_at > lenght Or (i - j) < 0 Then
                    Exit Do
                End If
                If exclude_tags = True Then
                    Dim prev_opening As Integer
                    Dim prev_closing As Integer
                    prev_opening = InStrRev(Me.plain_text_, "<", i)
                    prev_closing = InStrRev(Me.plain_text_, ">", i)
                    If prev_opening > prev_closing Or (prev_closing = 0 And prev_opening <> 0) Then
                        'jsme uvnitř tagu, takže toto kolo přeskočíme
                        GoTo next_loop
                    End If
                End If
                tmp2 = Mid(plain_text_, j, i - j)
                tmp = tmp & Mid(plain_text_, j, i - j) & replacementtxt
                'překoprujeme nezměněné části pole
                ReDim Preserve tmp_md(Len(tmp) - 1)
                For k = j To i - 1
                    tmp_md(l) = meta_data(k - 1)
                    l += 1
                Next
                'a přidáme změněné (pokud jseou delší než nic)
                For k = 1 To Len(replacementtxt)
                    ReDim tmp_md2(0)
                    tmp_md2(0) = mark
                    tmp_md(l) = tmp_md2
                    l += 1
                Next
                If Len(replacementtxt) = 0 Then
                    'nějak označíme místa okolo, že se v jejich sousedství něco ztratilo...
                    If l > 0 Then
                        ReDim tmp_md2(1)
                        tmp_md2(0) = "~removed_after" 'tj. že po tomto znaku bylo něco odstraněno
                        tmp_md2(1) = "!" & mark 'a s jakou značkou to bylo odstraněno
                        tmp_md(l - 1) = tmp_md2
                    End If
                End If
                j = i + Len(searchtxt)
                n = n + 1
                If (n_replacements <> -1 And n >= n_replacements) Then Exit Do 'po dosažení maximálního zadaného počtu nahrazení
                'text_changed(Replace(plain_text_, searchtxt, replacementtxt, i, 1),, mark)
                'i = i + replacement_len

next_loop:
            Loop
            tmp = tmp & Mid(plain_text_, j)
            ReDim Preserve tmp_md(Len(tmp) - 1)
            For k = j To Len(plain_text_) 'dokopírujeme metadata pro zbytek řetězce
                tmp_md(l) = meta_data(k - 1)
                l += 1
            Next
        Else
            'frm_main.Text = "repl"
            tmp = plain_text_
            Dim spl() As String
            Dim mc As MatchCollection
            Dim m As Match
            Dim rgxopt As RegexOptions

            Dim reg As Regex
            reg = New Regex(searchtxt, RegexOptions.Multiline)
            mc = reg.Matches(tmp, start_at)

            If mc.Count = 0 Then Exit Sub

            Dim fragment As String
            Dim frg_start_at As Integer = 1
            Dim frgms() As String
            Dim frgms_md()()() As String
            ReDim frgms(mc.Count * 2)
            ReDim frgms_md(mc.Count * 2)
            j = 0
            k = 0
            l = 0
            'řetězec si rozdělíme podle nalezených sekcí odpovídajících reg. výr. (fukce regex.split to udělá špatně, tak to musíme udělat takto)
            For i = 0 To mc.Count - 1
                frgms(j) = Mid(tmp, frg_start_at, (mc.Item(i).Index) - (frg_start_at - 1))
                If Len(frgms(j)) > 0 Then
                    ReDim Preserve frgms_md(j)(Len(frgms(j)) - 1)
                End If
                l = 0
                For k = frg_start_at - 1 To mc.Item(i).Index - 1
                    frgms_md(j)(l) = meta_data(k)
                    l += 1
                Next
                j += 1
                frgms(j) = Mid(tmp, mc.Item(i).Index + 1, mc.Item(i).Length)
                If Len(frgms(j)) > 0 Then
                    ReDim Preserve frgms_md(j)(Len(frgms(j)) - 1)
                End If
                l = 0
                For k = mc.Item(i).Index To mc.Item(i).Index + mc.Item(i).Length - 1
                    frgms_md(j)(l) = meta_data(k)
                    l += 1
                Next
                frg_start_at = mc.Item(i).Index + mc.Item(i).Length + 1
                j += 1
            Next
            frgms(j) = Mid(tmp, frg_start_at)
            If Len(frgms(j)) > 0 Then
                ReDim Preserve frgms_md(j)(Len(frgms(j)) - 1)
            End If
            l = 0
            For k = frg_start_at - 1 To Len(tmp) - 1
                frgms_md(j)(l) = meta_data(k)
                l += 1
            Next

            replacementtxt = Replace(replacementtxt, "\n", Chr(10))
            If replacementtxt Is Nothing Then replacementtxt = "" 'pokud provedeme předchozí příkaz nad prázdnm řetězcem, výsledkem není "", ale nothing. Nechápu proč, ale je to tak

            'v částech odpovídajících reg. výrazu provedeme jednotlivě nahrazení...
            ReDim tmp_md2(0)
            tmp_md2(0) = mark
            For i = 1 To UBound(frgms) Step 2
                frgms(i) = reg.Replace(frgms(i), replacementtxt)
                If Len(frgms(i)) > 0 Then
                    ReDim frgms_md(i)(Len(frgms(i)) - 1)
                    For j = 0 To Len(frgms(i)) - 1
                        frgms_md(i)(j) = tmp_md2
                    Next
                Else
                    Erase frgms_md(i)
                End If
            Next

            j = 0
            tmp = ""
            For i = 0 To UBound(frgms) 'spojíme fragmenty zase zpátky dohromady
                tmp = tmp & frgms(i)
            Next
            If Len(tmp) > 0 Then 'a spojíme dohromady i rozdělená metadata
                ReDim tmp_md(Len(tmp) - 1)
                k = 0
                For i = 0 To UBound(frgms)
                    If frgms_md(i) IsNot Nothing Then
                        For j = 0 To UBound(frgms_md(i))
                            tmp_md(k) = frgms_md(i)(j)
                            k += 1
                        Next
                    ElseIf i Mod 2 = 1 Then 'tj. jde o část, v níž bylo provedeno nahrazení, a to tak, že z ní nic nezbylo: označíme předchozí znak značkou
                        '~removed_after
                        If k > 0 Then
                            If tmp_md(k - 1) IsNot Nothing Then
                                ReDim Preserve tmp_md(k - 1)(UBound(tmp_md(k - 1)) + 1)
                            Else
                                ReDim tmp_md(k - 1)(0)
                            End If
                            tmp_md(k - 1)(UBound(tmp_md(k - 1))) = "~removed_after"
                        End If
                    End If
                Next
            Else
                Erase tmp_md
            End If
        End If
        meta_data = tmp_md
        plain_text_ = tmp
        calculate_lines()
        get_all_marks()
        RaiseEvent PlainText_changed(Me)
    End Sub

    Public Sub replace_marked(mark_to_find As String, rgx As Boolean, replacement_string As String, sel_start As Integer,
                                  sel_lenght As Integer, Optional all_chars_together As Boolean = True, Optional mark_with As String = "",
                                  Optional remove_old_mark As Boolean = True)
            'nahradí části řetězce označené nějakou značkou
            'rgx: zda se bude nahrazovat pomocí regulérního výrazu, nebo ne
            'all_chars_together: zda se bude v sekvenci po sobě jdoucích znaků s vybranou značkou jednat s každým zvlášť, nebo se všemi dohromady...
            Dim i As Integer, j As Integer
            Dim sekvence As String
            Dim seq_start As Integer
            Dim seq_end As Integer
        For i = sel_start To sel_start + sel_lenght
            If meta_data(i) IsNot Nothing Then
                If meta_data(i).Contains(mark_to_find) Then
                    If all_chars_together = True Then
                        seq_start = i
                        seq_end = -1
                        'pokud jednáme s celou sekvencí naráz...
                        For j = i + 1 To sel_start + sel_lenght
                            If meta_data(i) IsNot Nothing Then
                                If meta_data(i).Contains(mark_to_find) = False Then
                                    seq_end = j - 1
                                    Exit For
                                End If
                            End If
                        Next
                        If seq_end = -1 Then seq_end = j 'pokud jsme do konce vybraného textu nenašli konec sekvence, nastavíme konec sekvence na konec vybr. textu

                        If rgx = False Then
                            'a jdeme nahrazovat... - jednoduše
                            If Left(replacement_string, 1) <> "~" Then 'žádná specialita...
                            Else
                                Dim spec_command As String
                                Dim sc_end As Integer = InStr(1, replacement_string, " ")
                                If sc_end = 0 Then sc_end = Len(replacement_string)
                                spec_command = Left(replacement_string, sc_end)


                            End If

                        ElseIf rgx = True Then

                        End If


                    Else
                        'pokud chceme nahrazovat každý znak zvlášť...
                    End If
                End If
            End If
        Next
        get_all_marks()
        RaiseEvent PlainText_changed(Me)
        End Sub


    Public Sub insert_on_position(position_1_based As Integer, added_str As String, mark As String, Optional ByRef first_index_after0b As Integer = 0)
        'xyz = Len(plain_text_)
        'xyz = AscW(added_str)
        Dim l As Integer = Len(plain_text_)
        Dim i As Integer
        Dim added_md()() As String
        Dim added_mark() As String
        If position_1_based > 0 And position_1_based <= Len(plain_text_) Then
            Dim prev_str As String = Mid(plain_text_, 1, position_1_based - 1)
            Dim post_str As String = Mid(plain_text_, position_1_based)
            Dim prev_md()() As String
            Dim post_md()() As String

            added_mark = Split(mark, "|")
            If added_mark IsNot Nothing Then
                Dim md_to_merge() As String
                For i = 0 To UBound(added_mark)
                    If added_mark(i) = ">>>" Then
                        If position_1_based > 1 Then
                            md_to_merge = Me.meta_data(position_1_based - 2)
                        Else
                            added_mark(i) = ""
                        End If
                    ElseIf added_mark(i) = "<<<" Then
                        If position_1_based < Len(plain_text_) Then
                            md_to_merge = Me.meta_data(position_1_based)
                        Else
                            added_mark(i) = ""
                        End If
                    End If
                    If md_to_merge IsNot Nothing Then
                        If md_to_merge.Length <> 0 Then
                            For j = 0 To md_to_merge.Length - 1
                                If added_mark.Contains(md_to_merge(j)) = False Then
                                    ReDim Preserve added_mark(added_mark.Count)
                                    added_mark(added_mark.Count - 1) = md_to_merge(j)
                                End If
                            Next
                            md_to_merge = Nothing
                        End If
                    End If
                Next
            End If
            prev_md = copy_metadata_section(0, position_1_based - 2)
            post_md = copy_metadata_section(position_1_based - 1, Len(plain_text_) - 1)
            ReDim added_md(Len(added_str) - 1)

                For i = 0 To Len(added_str) - 1
                added_md(i) = added_mark
            Next

            plain_text_ = prev_str & added_str & post_str
            join_metadata_sections(prev_md, added_md, post_md)
            Dim p1_l As Integer, p2_l As Integer
            If prev_md IsNot Nothing Then p1_l = UBound(prev_md) + 1
            If added_md IsNot Nothing Then p2_l = UBound(added_md) + 1
            first_index_after0b = p1_l + p2_l
        ElseIf position_1_based > Len(plain_text_) Then
            plain_text_ &= added_str
            first_index_after0b = -1

            added_mark = Split(mark, "|")

            ReDim Preserve meta_data(Len(plain_text_) - 1)
            For i = l To Len(plain_text_) - 1
                meta_data(i) = added_mark
            Next


        End If
        calculate_lines()
        RaiseEvent PlainText_changed(Me)
        If plain_text_ <> "" Then
            xyz = Len(plain_text_)
            If Len(plain_text_) <> meta_data.Count Then
                Stop
            End If
        End If
        If added_mark IsNot Nothing Then
            For i = 0 To UBound(added_mark)
                If marks_ IsNot Nothing Then
                    If marks_.Contains(added_mark(i)) = False Then
                        add_mark(added_mark(i))
                    End If
                Else
                    add_mark(added_mark(i))
                End If
            Next i
        End If
    End Sub
    Private Sub add_mark(mark As String)
        n_marks += 1
        If n_marks < 0 Then n_marks = 0
        ReDim Preserve marks_(n_marks)
        marks_(n_marks) = mark
    End Sub
    Private Sub join_metadata_sections(ParamArray md_sections()()() As String) 'spojení více zadaných polí string()() do jednoho
            If md_sections IsNot Nothing Then
                Dim i As Integer, j As Integer, k As Integer
                Dim total As Integer
                For i = 0 To UBound(md_sections)
                    If md_sections(i) IsNot Nothing Then
                        total = total + UBound(md_sections(i)) + 1 'nejprve si zjistíme celkovou délku spojovaných polí
                    End If
                Next
                Dim tmp()() As String
                ReDim tmp(total - 1)
                For i = 0 To UBound(md_sections)
                    If md_sections(i) IsNot Nothing Then
                        For j = 0 To UBound(md_sections(i))
                            tmp(k) = md_sections(i)(j)
                            k += 1
                        Next
                    End If
                Next
                meta_data = tmp
            End If
        End Sub
        Private Function copy_metadata_section(first_index As Integer, last_index As Integer) As String()()
            If last_index = -100 Then last_index = Len(plain_text_) - 1
            Dim i As Integer, j As Integer
            Dim tmp()() As String
            If first_index > -1 And first_index <= UBound(meta_data) And last_index >= first_index Then
                If last_index > UBound(meta_data) Then last_index = UBound(meta_data)
                ReDim tmp(last_index - first_index)
                For i = first_index To last_index
                    'If i > UBound(meta_data) Then Stop
                    tmp(j) = meta_data(i)
                    j = j + 1
                Next
                Return tmp
            End If
        End Function

    Public Function line_from_char_index(char_index As Integer) As Integer
        Dim i As Integer
        For i = 0 To n_lines
            If lines_starting_indices(i) <= char_index And lines_starting_indices(i + 1) >= char_index Then
                Return i
            End If
        Next
        Return -1
    End Function


    Public Property line_metadata(line_index As Integer) As Integer()()

            Get
                Dim tmp()() As String
                Dim i As Integer, j As Integer
                If line_index > -1 And line_index <= n_lines Then
                    ReDim tmp(lines_starting_indices(line_index + 1) - lines_starting_indices(line_index) - 1)
                    For i = lines_starting_indices(line_index) To lines_starting_indices(line_index + 1)
                        'nemusíme se bát, že když budeme chtít poslední řádku, dojde k chybě (index mimo meze), protože 
                        'lines_starting_indices obsahují o jeden prvek víc, a v něm je první neexistující prvek (mimo meze lines_)
                        tmp(j) = meta_data(i)
                        j = j + 1
                    Next
                End If
            End Get
            Set(value As Integer()())

            End Set
        End Property

        Public Property line_metadata(line_index As Integer, char_index As Integer) As String()
            'získání a nastavení metadat pro jednotlivé znaky na jednotlivém řádku
            Get
                If line_index > -1 And line_index < n_lines Then
                    If lines_starting_indices(line_index) + char_index < lines_starting_indices(line_index + 1) Then
                        Return meta_data(lines_starting_indices(line_index) + char_index)
                    End If
                End If
            End Get
            Set(value As String())
                If line_index > -1 And line_index < n_lines Then
                    If lines_starting_indices(line_index) + char_index < lines_starting_indices(line_index + 1) Then
                        meta_data(lines_starting_indices(line_index) + char_index) = value
                    End If
                End If
            End Set
        End Property
        Public Sub add_char_metadata_value(value As String, line_index As Integer, char_index As Integer, Optional overwrite As Boolean = True)
            'přidá jednu hodnotu do pole metadat určitého znaku
            If line_index > -1 And line_index < n_lines Then
                Dim mdata_index As Integer = lines_starting_indices(line_index) + char_index
                If mdata_index < lines_starting_indices(line_index + 1) Then
                    If meta_data(mdata_index) IsNot Nothing Then

                        If overwrite = False Or (meta_data(mdata_index).Contains(value) = False And overwrite = True) Then
                            Dim n As Integer
                            ReDim Preserve meta_data(mdata_index)(UBound(meta_data(mdata_index)) + 1)
                            ReDim meta_data(mdata_index)(0)

                            meta_data(mdata_index)(UBound(meta_data(mdata_index))) = value
                        End If
                    Else
                        ReDim meta_data(mdata_index)(0)

                        meta_data(mdata_index)(UBound(meta_data(mdata_index))) = value
                    End If
                End If
            End If

        End Sub
        Public Sub add_metadata_to_section(value As String, si As Integer, ei As Integer, overwrite As Boolean)
            Dim i As Integer
            For i = si To ei
                add_char_metadata_value(value, i, overwrite)
            Next
        End Sub
        Public Sub add_char_metadata_value(value As String, char_index As Integer, Optional overwrite As Boolean = True)
            'přidá jednu hodnotu do pole metadat určitého znaku - zadaného v absoutní pozici
            If char_index > -1 And char_index < Len(plain_text_) Then

                Dim n As Integer
                If meta_data(char_index) Is Nothing Then
                    ReDim meta_data(char_index)(0)

                ElseIf overwrite = False Or (meta_data(char_index).Contains(value) = False And overwrite = True) Then
                    If meta_data(char_index) IsNot Nothing Then
                        ReDim Preserve meta_data(char_index)(UBound(meta_data(char_index)) + 1)
                    Else
                        ReDim meta_data(char_index)(0)
                    End If
                End If
                meta_data(char_index)(UBound(meta_data(char_index))) = value
            End If
        End Sub

        Public ReadOnly Property lines() As String()
            Get
                lines = lines_
            End Get
        End Property
        Public Property line(index As Integer, Optional add As Boolean = False) As String
            Get
                If index > -1 And index <= n_lines Then
                    Return lines_(index)
                End If
            End Get

            Set(ByVal value As String)
                Dim txt As String
                If index = -1 Then 'přidání řádky na začátek
                    txt = value & Chr(10) & plain_text_
                    ' plain_text = txt
                ElseIf index = -10 Or index > n_lines Then 'přidáme řádku na konec
                    txt = plain_text_ & Chr(10) & value
                    'plain_text = txt
                ElseIf index > -1 And index < n_lines Then
                    Dim i As Integer
                    For i = 0 To index - 1
                        txt = txt & lines_(i) & Chr(10)
                    Next
                    txt = txt & value & Chr(10)
                    If add = True Then 'řádku přidáváme
                        For i = index To n_lines
                            txt = txt & Chr(10) & lines_(i)
                        Next
                    Else 'pokud cheme změnit již existující řádku...
                        Dim md_before()() As String
                        Dim md_after()() As String
                        Dim md_new()() As String

                        If index <> 0 Then
                            md_before = copy_metadata_section(0, lines_starting_indices(index) - 1)
                        End If
                        ReDim md_new(Len(value) - 1 + 1) '-1 protože len počítá od 1 a +1 protože musíme přidat ještě znak zlomu řádku, který ve value není
                        If index <> n_lines Then
                            md_after = copy_metadata_section(lines_starting_indices(index + 1), Len(plain_text_) - 1)
                        End If

                        For i = index + 1 To n_lines - 1
                            txt = txt & lines_(i) & Chr(10)
                        Next
                        txt = txt & lines_(n_lines)

                        xyz = Len(txt)
                        xyz = UBound(meta_data)
                        join_metadata_sections(md_before, md_new, md_after)
                        plain_text_ = txt
                        calculate_lines()
                    End If

                End If
            End Set
        End Property
        Public ReadOnly Property line_start_index(lineindex As Integer)
            Get
                If lineindex > -1 And lineindex <= n_lines Then
                    Return CInt(lines_starting_indices(lineindex))
                End If
            End Get
        End Property
        Public ReadOnly Property line_end_index(lineindex As Integer)
            Get
                If lineindex > -1 And lineindex <= n_lines Then
                    Return CInt(lines_starting_indices(lineindex) + Len(lines_(lineindex)))
                End If
            End Get
        End Property
        Public ReadOnly Property last_line_start() As Integer
            Get
                If Me.n_lines > -1 Then
                    Return lines_starting_indices(Me.n_lines)
                End If
            End Get
        End Property
        Public ReadOnly Property plain_text() As String
            Get
                Return plain_text_
            End Get

        End Property
    Public Sub text_changed(text As String, md()() As String, mode As Long)

        If mode = 1 Or Me.plain_text_ = "" Then 'nahrazení celého textu
            If Len(text) = md.Count Then
                plain_text_ = text
                meta_data = md
                md = Nothing
                get_all_marks()
            End If
        ElseIf mode = 0 Then

        ElseIf mode = 2 Then 'přidání na konec
            plain_text_ &= text
            Dim tmpmd()() As String
            tmpmd = Me.copy_metadata_section(0, UBound(meta_data))
            join_metadata_sections(tmpmd, md)
            get_all_marks()
        End If
    End Sub
    Public Sub text_changed(value As cls_preXML_section_page, Optional mode As Integer = 2)
            'mode 2=přidání
            If value IsNot Nothing Then
                If mode = 2 Then
                    Me.plain_text_ &= vbLf
                    ReDim Preserve Me.meta_data(Len(Me.plain_text_) - 1)
                    Me.meta_data(Len(Me.plain_text_) - 1) = {"~spojení stran"}
                    Me.plain_text_ &= value.plain_text_
                    Dim tmp_md1()() As String = Me.copy_metadata_section(0, UBound(Me.meta_data))
                    Dim tmp_md2()() As String = value.copy_metadata_section(0, UBound(value.meta_data))
                    join_metadata_sections(tmp_md1, tmp_md2)
                End If
            End If
        End Sub
    Public Sub text_changed(Value As String, Optional mode As Integer = 0, Optional mark_newly_added As String = "")
        p_state.add()
        Dim i As Integer, j As Integer
        Dim tmp_md()() As String
        Dim tmp() As String
        xyz = 1
        xyz = InStr(xyz + 1, plain_text_, vbLf)
        Value = Replace(Value, vbCr, "")

        Dim nvLen As Integer = Len(Value)
        Dim ovLen As Integer = Len(plain_text_)

        Dim pocet_stejnych_na_zacatku As Integer
        Dim pocet_stejnych_na_konci As Integer
        Dim index_zacatku_nezmenene_casti_na_konce_ve_starem As Integer
        Dim index_zacatku_nezmenene_casti_na_konci_v_novem As Integer

        Dim ch1 As String
        Dim ch2 As String

        Dim rozdil As Integer
        If ovLen = 0 Or mode = 1 Then 'úplně nový text (nebo původně žádný text nebyl)
            plain_text_ = Value
            If nvLen <> 0 Then
                ReDim meta_data(nvLen - 1)
            Else
                Erase meta_data
            End If
        ElseIf mode = 0 Then
            If nvLen > ovLen Then
                rozdil = nvLen - ovLen
                index_zacatku_nezmenene_casti_na_konci_v_novem = pt_selstart + 1
                pocet_stejnych_na_konci = nvLen - (index_zacatku_nezmenene_casti_na_konci_v_novem - 1)
                index_zacatku_nezmenene_casti_na_konce_ve_starem = ovLen - (pocet_stejnych_na_konci - 1)

                pocet_stejnych_na_zacatku = -1
                For i = 1 To pt_prev_selstart
                    ch1 = Value(i - 1)
                    ch2 = plain_text_(i - 1)
                    If ch1 <> ch2 Then
                        pocet_stejnych_na_zacatku = i - 1
                        Exit For
                    End If
                Next

                If pocet_stejnych_na_zacatku = -1 Then pocet_stejnych_na_zacatku = pt_prev_selstart
            ElseIf ovLen > nvLen Then
                pocet_stejnych_na_zacatku = pt_selstart
                If pocet_stejnych_na_zacatku = -1 Then pocet_stejnych_na_zacatku = 0
                pocet_stejnych_na_konci = -1

                For i = 0 To nvLen - pt_selstart - 1
                    If nvLen - 1 > 0 Then
                        ch1 = Value(nvLen - i - 1)
                    Else
                        ch1 = ""
                    End If
                    ch2 = plain_text_(ovLen - i - 1)
                    If ch1 <> ch2 Then
                        pocet_stejnych_na_konci = i
                        Exit For
                    End If
                Next
                If pocet_stejnych_na_konci = -1 Then pocet_stejnych_na_konci = nvLen - pt_prev_selstart

                index_zacatku_nezmenene_casti_na_konci_v_novem = nvLen - i + 1
                index_zacatku_nezmenene_casti_na_konce_ve_starem = ovLen - i + 1
            Else
                'změna někde v řetězci, ale při zachování délky
                pocet_stejnych_na_zacatku = pt_prev_selstart
                index_zacatku_nezmenene_casti_na_konci_v_novem = pt_selstart + 1
                index_zacatku_nezmenene_casti_na_konce_ve_starem = index_zacatku_nezmenene_casti_na_konci_v_novem
            End If

            If nvLen <> 0 Then
                ReDim tmp_md(nvLen - 1)
                j = 0
                For i = 1 To pocet_stejnych_na_zacatku
                    tmp_md(j) = meta_data(i - 1)
                    j += 1
                Next
                If pocet_stejnych_na_zacatku = -1 Then pocet_stejnych_na_zacatku = 0

                Dim stejne_znacky As String 'nově vloženému znaku přidáme všechny značky, které mají předcházející i následující znak společné.
                If index_zacatku_nezmenene_casti_na_konci_v_novem < nvLen Then 'pokud tedy nedochází k mazání textu odzadu
                    If meta_data(pocet_stejnych_na_zacatku) IsNot Nothing And meta_data(index_zacatku_nezmenene_casti_na_konce_ve_starem) IsNot Nothing Then
                        For i = 0 To UBound(meta_data(pocet_stejnych_na_zacatku))
                            If meta_data(index_zacatku_nezmenene_casti_na_konce_ve_starem).Contains(meta_data(pocet_stejnych_na_zacatku)(i)) Then
                                stejne_znacky &= meta_data(pocet_stejnych_na_zacatku)(i) & "|"
                            End If
                        Next i
                    End If
                    stejne_znacky = Trim(stejne_znacky)
                End If
                mark_newly_added &= stejne_znacky
                For i = pocet_stejnych_na_zacatku + 1 To index_zacatku_nezmenene_casti_na_konci_v_novem - 1
                    If mark_newly_added <> "" Then
                        tmp = Split(mark_newly_added, "|")
                    End If
                    tmp_md(j) = tmp
                    j += 1
                Next

                For i = index_zacatku_nezmenene_casti_na_konce_ve_starem To ovLen
                    tmp_md(j) = meta_data(i - 1)
                    j += 1
                Next
            Else
                Erase tmp_md
            End If
            plain_text_ = Value
            meta_data = tmp_md
        ElseIf mode = 2 Then 'přidání 
            Me.plain_text_ &= Value
            ReDim Preserve meta_data(Len(Me.plain_text_) - 1)
        End If
        calculate_lines()
        If tmp IsNot Nothing Then
            For i = 0 To UBound(tmp)
                If marks_ IsNot Nothing Then
                    If marks_.Contains(tmp(i)) = False Then
                        add_mark(tmp(i))
                    End If
                Else
                    add_mark(tmp(i))
                End If
            Next i
        End If
        RaiseEvent PlainText_changed(Me)
        xyz = Len(env.wsp.rtb.Text)
        xyz = Len(Me.plain_text_)
        'If Len(env.wsp.rtb.Text) <> Len(Me.plain_text_) Then Stop

        If plain_text_ <> "" Then
            xyz = Len(plain_text_)
            If Len(plain_text_) <> meta_data.Count Then
                Stop
            End If
        End If

    End Sub
    Private Sub calculate_lines()
            If plain_text_ <> "" Then

                lines_ = Split(plain_text_, Chr(10))
                n_lines = UBound(lines_)
                ReDim lines_starting_indices(n_lines + 1) 'bude jich o jednu víc, než je řádek, protože poslední bude označovat první index za polem
                lines_starting_indices(0) = 0
                Dim subtotal As Integer
                For i = 1 To n_lines
                    subtotal += Len(lines_(i - 1)) + 1
                    lines_starting_indices(i) = subtotal
                    '+1 protože funkce split, když rozděluje podle chr(10), tento znak do rozdělených řetězců nezahrne
                Next
                subtotal += Len(lines(n_lines)) + 1
                lines_starting_indices(n_lines + 1) = subtotal 'tento prvek pole označuje konec - jde o první index, který je mimo meze
            Else
                Erase lines_
                Erase lines_starting_indices
                n_lines = -1
            End If
        End Sub
        Public Function delete_marked_on_position(position As Integer, mark As String) As Point
            'funkce smaže text, který je označen zadanou značkou a leží okolo zadané pozice (ALT+Delete v rtb)
            Dim i As Integer
            Dim j As Integer
            For i = position To 0 Step -1
                If meta_data(i) IsNot Nothing Then
                    If meta_data(i).Contains(mark) = False Then
                        Exit For
                    End If
                Else
                    Exit For
                End If
            Next
            For j = position To UBound(meta_data)
                If meta_data(j) IsNot Nothing Then
                    If meta_data(j).Contains(mark) = False Then
                        Exit For
                    End If
                Else
                    Exit For
                End If
            Next
            'i+1=první index[0b] mazaného textu, j-1=poslední index[0b] mazaného textu
            delete_text_on_position(i + 1, j - 1)
            delete_marked_on_position.X = i + 1
            delete_marked_on_position.Y = j - 1
        get_all_marks()
    End Function
        Public Sub delete_text_on_position(p1_1_based As Integer, p2_1_based As Integer)
            'p1 je první index mazané části a p2 je poslední index. 
            'xyz = Mid(plain_text_, p1_1_based, 100)
            If (p1_1_based > 0 And p2_1_based <= Len(plain_text_) And p1_1_based <= p2_1_based) Then
                Dim md1()() As String = copy_metadata_section(0, p1_1_based - 2) 'protože indexy pole metadat začínají nulou (=-1) a my potřebujeme skončit před zadaným
                'znakem (=podruhé -1)
                Dim md2()() As String = copy_metadata_section(p2_1_based, UBound(meta_data))
                'opět jednou -1 kvůli base 0 u metadata, ale pak +1, protože potřebujeme začít až o znak za zadaným indexem
                Dim tmp As String '=Left(plain_text_, p1)
            'xyz = Left(plain_text_, p1_1_based - 1)
            'xyz = Mid(plain_text_, p2_1_based + 1)
            tmp = Left(plain_text_, p1_1_based - 1) & Mid(plain_text_, p2_1_based + 1)
                Dim x As Integer = Len(tmp)
                plain_text_ = tmp
                'poslední znak mazaného řetězce
                join_metadata_sections(md1, md2)
                calculate_lines()
            End If
        If plain_text_ <> "" Then
            xyz = Len(plain_text_)
            If Len(plain_text_) <> meta_data.Count Then
                Stop
            End If
        End If
        get_all_marks()
    End Sub

        Public Sub plain_text_selection_changed(sel_start As Integer, sel_length As Integer)
            pt_prev_selstart = pt_selstart
            pt_prev_sellength = pt_sellength
            pt_sellength = sel_length
            pt_selstart = sel_start
            context.get_context()
        End Sub
        Public ReadOnly Property SelStart_prev()
            Get
                Return pt_prev_selstart
            End Get
        End Property
        Public ReadOnly Property SelLength_prev()
            Get
                Return pt_prev_sellength
            End Get
        End Property
        Public Function prev_selection() As String
            Return Mid(plain_text_, pt_prev_selstart, pt_prev_sellength)
        End Function
        Public Function selection() As String
            'Dim tmp As String = Mid(plain_text_, pt_selstart, pt_sellength)
            If pt_selstart > 0 And pt_prev_selstart + pt_sellength <= Len(plain_text_) Then
                Return Mid(plain_text_, pt_selstart + 1, pt_sellength)
            End If

        End Function
        Public ReadOnly Property SelStart0b()
            Get
                Return pt_selstart
            End Get
        End Property
        Public ReadOnly Property SelStart1b()
            Get
                Return pt_selstart + 1
            End Get
        End Property
        Public ReadOnly Property SelLength()
            Get
                Return pt_sellength
            End Get
        End Property

        Public Function get_position_context(p As Integer) As String
            If p <> 0 Then
                Dim marks() As String
                marks = Me.meta_data(p - 1)

                Dim tag As String
                Dim i As Integer
                Dim pos As Integer
                i = -1
                InStrRev_first(p, Me.plain_text_, pos, i, "<", ">")
                If pos <> 0 And i = 0 Then
                    Dim tagname_end As Integer
                    tagname_end = InStr_first(pos, Me.plain_text_, 0, 0, " ", ">")
                    If tagname_end = -1 Then tagname_end = Len(Me.plain_text_)
                    If tagname_end - pos - 1 > 0 Then
                        tag = Mid(Me.plain_text_, pos + 1, tagname_end - pos - 1)
                    End If
                    Return tag
                End If
            End If
        End Function
        Public Function is_marked(mark As String, si As Integer, ei As Integer, Optional perc As Integer = 100) As Boolean
            Dim i As Integer
            Dim y As Integer
            Dim n As Integer
            Dim p As Single
            If meta_data IsNot Nothing Then
                If si > -1 And si <= UBound(meta_data) And ei <= UBound(meta_data) Then

                    For i = si To ei
                        If meta_data(i) IsNot Nothing Then
                            If meta_data(i).Contains(mark) Then
                                y += 1
                            Else
                                n += 1
                                'xyz = Mid(plain_text_, i, 100)
                            End If
                        End If
                    Next
                    If (y + n) <> 0 Then
                        p = y / (y + n) * 100
                    End If
                    If p >= perc Then
                        Return True
                    Else
                        Return False
                    End If
                End If
            End If
        End Function
        Public Sub remove_mark_on_position(mark As String, p1 As Integer, p2 As Integer)
            Dim i As Integer, j As Integer, k As Integer
            Dim nn As Integer
            If Not meta_data Is Nothing Then
                If p1 <= UBound(meta_data) Then
                    If p2 > UBound(meta_data) Then p2 = UBound(meta_data)
                    For i = p1 To p2
                        If meta_data(i) IsNot Nothing Then
                            If meta_data(i).Contains(mark) = True Then
                                nn = UBound(meta_data(i)) - 1
                                If nn <> -1 Then
                                    For j = 0 To UBound(meta_data(i))
                                        If meta_data(i)(j) = mark Then
                                            For k = j To UBound(meta_data(i)) - 1
                                                meta_data(i)(k) = meta_data(i)(k + 1)
                                            Next
                                            ReDim Preserve meta_data(i)(nn)
                                            Exit For
                                        End If
                                    Next
                                Else
                                    Erase meta_data(i)
                                End If
                            End If
                        End If
                    Next
                End If
            End If
        End Sub
        Public Function load_from_preXML(preXML_lines() As String) As Boolean
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer
            Dim n As Integer
            Dim l As String
            Dim md() As String
            If preXML_lines IsNot Nothing Then
                n = UBound(preXML_lines)
                For i = 0 To n

                If preXML_lines(i) = "<!preXML_text>" Then
                    i += 1
                    plain_text_ = ""
                    Do While InStrX(1, preXML_lines(i), "</!preXML_text>", k) = 0
                        plain_text_ &= preXML_lines(i) & vbLf
                        i += 1
                        If i > UBound(preXML_lines) Then 'dostali jsme se mimo pole, v souboru je nějaká chyba
                            ReDim Preserve meta_data(Len(plain_text_) - 1)
                            Me.calculate_lines()
                            Return False
                        End If
                    Loop
                    plain_text_ &= Left(preXML_lines(i), k - 1)
                    End If
                    If preXML_lines(i) = "<!preXML_metadata>" Then
                        i += 1
                        ReDim Preserve meta_data(Len(plain_text_) - 1)

                        Dim md_index As Integer = 0

                        Do While preXML_lines(i) <> "</!preXML_metadata>"
                            l = preXML_lines(i)
                            j = InStr(1, l, "[")
                            If j <> 0 Then
                                k = InStr(CInt(j), l, "]")
                                If k <> 0 Then
                                    l = Mid(l, j + 1, k - j - 1)
                                    Erase md
                                    If l <> "" Then
                                        md = Split(l, "|")
                                    End If
                                    If md IsNot Nothing Then
                                        j = 0
                                        Do While j <= UBound(md)
                                            If md(j) = "" Then
                                                For k = j To UBound(md) - 1
                                                    md(k) = md(k + 1)
                                                Next
                                                If UBound(md) > 0 Then
                                                    ReDim Preserve md(UBound(md) - 1)
                                                Else
                                                    Erase md
                                                    Exit Do
                                                End If
                                                j = j - 1
                                            End If
                                            j += 1
                                        Loop
                                    End If
                                If md IsNot Nothing And md_index <= UBound(meta_data) Then
                                    meta_data(md_index) = md
                                End If
                                xyz = Len(plain_text_)
                            End If
                            End If
                            i += 1
                            md_index += 1
                            If i > UBound(preXML_lines) Then 'dostali jsme se mimo pole, v souboru je nějaká chyba
                                Return False
                            End If
                        Loop
                    End If
                Next
                Me.calculate_lines()
                'Me.check_xml(True)
            End If
        End Function
        Public Function preXML_to_save() As String
            Dim o As String
            o = "<!preXML_text>" & vbNewLine
            o &= plain_text_ & "</!preXML_text>" & vbNewLine
            Dim n As Integer
            If meta_data IsNot Nothing Then
                n = UBound(meta_data)
            Else
                n = -1
            End If
            o &= "<!preXML_metadata>" & vbNewLine
            Dim i As Integer
            Dim j As Integer
        Dim mdo As String = ""
        Dim buffer As String = ""
        If meta_data IsNot Nothing Then
            For i = 0 To UBound(meta_data)
                mdo &= "["
                If meta_data(i) IsNot Nothing Then
                    For j = 0 To UBound(meta_data(i))
                        mdo &= meta_data(i)(j) & "|"
                    Next
                End If
                mdo &= "]" & vbNewLine
                If Len(mdo) > 5000 Then
                    buffer = buffer & mdo
                    mdo = ""
                End If
            Next

        End If
        buffer = buffer & mdo

        o &= buffer

        o &= "</!preXML_metadata>"

            Return o
        End Function

    End Class
    Public Class cls_preXML_section_page_Workspace
    Public pp As cls_preXML_section_page
    Public my_saved_tools As cls_tools_collection
    Public on_insert As cls_tools_collection

End Class
