Imports System.Text.RegularExpressions

Public Class rules_collection
    'různé kolekce pravidel (třeba pro určitou sekci pro událost vložení textu nebo pro ruční spuštění...)
    'idea je taková, že bude jedna kolekce "fyzickcýh" pravidel, kde budou všechna pravidla, výchozí i uživatelsky nastavená
    'a poté jednotlivé sekce a události budou mít vlastní kolekce, které ale budou pouze odkazovat do té hlavní
    Public n_rules As Long
    Public r() As cls_rule
    Public r_enabled() As Boolean
    Public r_run() As Long
    Public am_i_base_collection As Boolean

    Public description As String
    Public name As String
    Public run_on_insert As Boolean

    Public Event rule_triggered(rulecolection As rules_collection, rule As cls_rule, reaction As Long, rulerun As Long)

    Public Sub rule_triggered_handler(rule As cls_rule, reaction As Long, rulerun As Long)
        'vpodstatě jen přepošleme dál událost, kterou jsme zachytili od nějakého našeho pravidla a akorát přidáme info, z jaké to je kolekce, kdyby to někoho zajímalo
        RaiseEvent rule_triggered(Me, rule, reaction, rulerun)
    End Sub

    Public Function run_all(ppart As cls_preXML_page_part, reaction_mode As Long)
        'spustí všechna zapnutá pravidla v kolekci
        Dim i As Long
        For i = 0 To n_rules
            If r_enabled(i) = True Then
                r(i).eval(ppart, reaction_mode, r_run(i))
            End If
        Next
    End Function
    Public Function add_rule(Optional referenced_rule As cls_rule = Nothing, Optional clone As Boolean = True) As cls_rule
        If am_i_base_collection = False And referenced_rule Is Nothing Then
            Exit Function
        End If
        n_rules += 1
        ReDim Preserve r(n_rules)
        ReDim Preserve r_enabled(n_rules)
        ReDim Preserve r_run(n_rules) 'informace o tom, pokolikáté bylo toto pravidlo spuštěno (abychom měli přehled...)
        If Not referenced_rule Is Nothing Then
            r_enabled(n_rules) = True
            If clone = True Then 'zklonujeme si ji...r(n_rules)
                r(n_rules) = New cls_rule()
                referenced_rule.clone(r(n_rules))
                AddHandler r(n_rules).rule_triggered, AddressOf rule_triggered_handler
            Else
                r(n_rules) = referenced_rule
            End If
        Else
            r_enabled(n_rules) = True
            r(n_rules) = New cls_rule
            add_rule = r(n_rules)
            AddHandler r(n_rules).rule_triggered, AddressOf rule_triggered_handler
        End If
    End Function

    Public Sub New(basecollection As Boolean)
        n_rules = -1
        am_i_base_collection = basecollection
        If am_i_base_collection = True Then 'vytvoříme základní sady - nebo je později načteme ze souborů?
            add_rule(New cls_rule(New cls_condition("rgxc ^\s*([0-9]*[05])(?:[^0-9]|$)", "", 0), New cls_reaction("highlight-defclr:red3;", "~D $1"),
                                  Nothing, "Odstranění čísel řádků na začátku", "Odstraní čísla řádků (na začátku řádků), která zůstala v textu"))
            add_rule(New cls_rule(New cls_condition("rgxc [^0-9]([0-9]*[05]\s*$)", "", 0),
                                  New cls_reaction("highlight-defclr:red3;", "rgxr [^0-9]([0-9]*[05]\s*$)", ""),
                                  Nothing, "Odstranění čísel řádků na konci", "Odstraní čísla řádků (na konci řádků), která zůstala v textu"))
            add_rule(New cls_rule(New cls_condition("rgxc ([<>\(\)\|/_])", "", 0), New cls_reaction("highlight-defclr:yellow3;", "~D $1"),
                                  Nothing, "Vyznačení podezřelých znaků", "Vyznačí znaky, které se v textech běžně nevyskytují, a jde tedy možná o chybu OCR"))
            add_rule(New cls_rule(New cls_condition("rgxc ([A-Za-z][0-9?!.,:]+[A-Za-z])", "", 0), New cls_reaction("highlight-defclr:yellow3;", "~D $1"),
                                  Nothing, "Vyznačení podezřelých kombinací znaků",
                                  "Vyznačí běžně se nevyskytující kombinace znaků (např. číslo mezi písmeny apod.),
                                  které mohou být chybou v OCR"))
            add_rule(New cls_rule(New cls_condition("rgxt .*", "", 0), New cls_reaction("highlight-defclr:green3;",, "~^+ <lb/>"),
                                  Nothing, "Přidej na začátek řádky tag zlomu řádku",
                                  "Přidá na začátek každé řádky tag <lb/>"))

        End If
    End Sub
End Class
'####################################################################################################################################################################################################################

Public Class cls_rule
    Public act As cls_condition
    Public react As cls_reaction
    Public name As String
    Public desc As String
    Public pp As cls_preXML_page_part

    Public Event rule_triggered(rule As cls_rule, reaction_mode As Long, rulerun As Long)

    Protected Friend _am_i_clone As Boolean
    Public ReadOnly Property am_i_clone As Boolean
        Get
            Return _am_i_clone
        End Get
    End Property

    Public Function clone(obj_to_clone_into As cls_rule)
        With obj_to_clone_into
            .name = name
            ._am_i_clone = True
            .desc = desc
            .pp = pp 'tohle bude jen reference
            .act = New cls_condition()
            .react = New cls_reaction()
            act.clone(.act)
            react.clone(.react)
        End With
    End Function

    Public Sub New(activator As cls_condition, reaction As cls_reaction, p_p As cls_preXML_page_part, rname As String, rdesc As String)
        act = activator
        react = reaction
        pp = p_p
        name = rname
        desc = rdesc
    End Sub


    Public Sub New()

    End Sub

    Public Function eval(where As String, reaction_mode As Long, ByRef rulerun As Long) As Boolean
        rulerun = rulerun + 1
        If act.eval(where) = True Then

        End If
        RaiseEvent rule_triggered(Me, reaction_mode, rulerun)
    End Function
    Public Function eval(where() As String, reaction_mode As Long, ByRef rulerun As Long) As Boolean
        rulerun = rulerun + 1
        Dim mc() As MatchCollection
        Dim mc_lines() As Long 'tady se bude uchovávat informace o tom, na kterém řádku se která MatchCollection nachází
        Dim i As Long
        For i = 0 To UBound(where)
            If act.eval(where, i, mc, mc_lines) = True Then
                If reaction_mode = 0 Then
                    highlight(mc, mc_lines, rulerun)
                End If
            End If
        Next
        RaiseEvent rule_triggered(Me, reaction_mode, rulerun)
    End Function
    Public Function eval(ppart As cls_preXML_page_part, reaction_mode As Long, ByRef rulerun As Long) As Boolean
        'provedeme vyhodnocení všech řádků sekce
        rulerun = rulerun + 1
        Dim mc() As MatchCollection
        Dim mc_lines() As Long 'tady se bude uchovávat informace o tom, na kterém řádku se která MatchCollection nachází

        Dim reaction_mc(0) As MatchCollection
        Dim reaction_mc_lines(0) As Long
        Dim i As Long
        pp = ppart

        For i = 0 To ppart.n_lines
            If act.eval(ppart.lines, i, mc, mc_lines) = True Then
                If reaction_mode = 0 Then 'jen zvýraznění...
                    highlight(mc, mc_lines, rulerun)
                ElseIf reaction_mode = 1 Then
                    ppart.line(i) = react.react(ppart.line(i), reaction_mc(0))
                    reaction_mc_lines(0) = i
                    highlight(reaction_mc, reaction_mc_lines, rulerun)
                End If
            End If
        Next
        RaiseEvent rule_triggered(Me, reaction_mode, rulerun)
    End Function
    Private Sub highlight(mc() As MatchCollection, ByRef mc_lines() As Long, run As Long)
        Dim i As Long, j As Long, k As Long, l As Long
        For i = 0 To UBound(mc)
            If Not mc(i) Is Nothing Then
                For j = 0 To mc(i).Count - 1
                    For k = 1 To mc(i).Item(j).Groups.Count - 1
                        For l = mc(i).Item(j).Groups(CInt(k)).Index To mc(i).Item(j).Groups(CInt(k)).Length - 1 + mc(i).Item(j).Groups(CInt(k)).Index
                            pp.add_char_metadata_value(name & " {" & run & "}", mc_lines(i), l)
                            'pp.add_char_metadata_value(react.hgl_r, mc_lines(i), l)
                        Next l
                    Next k
                Next j
            End If
        Next i
    End Sub
End Class

'####################################################################################################################################################################################################################

Public Class cls_condition
    Public n_expr As Long = -1
    Public e() As cls_expression
    Protected Friend e_operators_() As Long
    Protected Friend e_relative_line_index() As Long 'při vyhodnocování se obdrží index řádky (pokud se bude vyhodnocování provádět nad polem řádků)
    'a každý další výraz bude vyhodnocování provádět na řádce relativní k této zadané (může být i 0, tj. bude se provádět stále na stejné řádce)


    Public Property expr_operators(index As Long) As String
        Get
            If index > -1 And index <= n_expr Then
                If e_operators_(index) = 0 Then
                    Return "---"
                ElseIf e_operators_(index) = 1 Then
                    Return "AND"
                ElseIf e_operators_(index) = 2 Then
                    Return "OR"
                End If
            End If
        End Get
        Set(value As String)
            If index > -1 And index <= n_expr Then
                If UCase(value) = "AND" Then
                    e_operators_(index) = 1
                ElseIf ucase(value) = "OR" Then
                    e_operators_(index) = 2
                End If
            End If
        End Set
    End Property

    Public Sub clone(obj_to_clone_into As cls_condition)
        With obj_to_clone_into
            .n_expr = n_expr
            ReDim .e_operators_(UBound(e_operators_))
            For i = 0 To UBound(.e_operators_)
                .e_operators_(i) = e_operators_(i)
            Next
            ReDim .e_relative_line_index(UBound(e_relative_line_index))
            For i = 0 To UBound(.e_relative_line_index)
                .e_relative_line_index(i) = e_relative_line_index(i)
            Next
            ReDim .e(UBound(e))
            For i = 0 To UBound(.e)
                .e(i) = New cls_expression
                e(i).clone(.e(i))
            Next
        End With
    End Sub

    Public Function add_expression(Optional expression As String = "", Optional op_before As String = "", Optional relative_index As Long = 0) As cls_expression
        'op before=operátor určující, v jakém vztahu he tato podmínka k podmínce předchozí
        'pokud toto nebude zadáno a zároveň nepůjde o první podmínku v pravidle, je to chyb
        If n_expr > 0 And op_before = "" Then
            Return Nothing
        Else
            n_expr = n_expr + 1
            ReDim Preserve e(n_expr)
            e(n_expr) = New cls_expression(expression)
            ReDim Preserve e_relative_line_index(n_expr)
            e_relative_line_index(n_expr) = relative_index
            ReDim Preserve e_operators_(n_expr)
            expr_operators(n_expr) = op_before
            Return e(n_expr)
        End If

    End Function
    Public Sub New(Optional expression As String = "", Optional op_before As String = "", Optional relative_index As Long = 0)
        add_expression(expression, op_before, relative_index)
    End Sub


    Public Sub New()
        n_expr = -1
    End Sub

    Public Function eval(where As String, Optional ByRef mc As MatchCollection = Nothing) As Boolean
        'vyhodnocení se provede nad jedním řetězcem
        Dim result As Boolean
        Dim sub_result As Boolean
        result = e(0).eval(where)
        For i = 1 To n_expr
            sub_result = e(i).eval(where,, mc)
            If e_operators_(i) = 1 Then
                result = result And sub_result
            Else
                result = result Or sub_result
            End If
        Next
        Return result
    End Function
    Public Function eval(where() As String, start_index As Long,
                         Optional ByRef mc() As MatchCollection = Nothing, Optional ByRef mc_lines() As Long = Nothing) As Boolean
        'vyhodnocení po řádcích
        Dim result As Boolean
        Dim i As Long
        Dim sub_result As Boolean

        ReDim mc(n_expr)
        ReDim mc_lines(n_expr)

        If start_index + e_relative_line_index(0) <= UBound(where) And start_index + e_relative_line_index(0) >= 0 Then
            result = e(0).eval(where(start_index + e_relative_line_index(0)),, mc(0))
            mc_lines(0) = start_index + e_relative_line_index(0)
        Else
            result = False
        End If
        For i = 1 To n_expr
            If start_index + e_relative_line_index(i) <= UBound(where) And start_index + e_relative_line_index(i) >= 0 Then
                sub_result = e(i).eval(where(start_index + e_relative_line_index(i)),, mc(i))
                mc_lines(i) = start_index + e_relative_line_index(i)
            Else
                sub_result = False
            End If
            If e_operators_(i) = 1 Then
                result = result And sub_result
            Else
                result = result Or sub_result
            End If
            'If result = True Then Stop
        Next
        Return result
    End Function
End Class
'####################################################################################################################################################################################################################

Public Class cls_expression
    Protected Friend expression As String
    Protected Friend expr_rgx As Boolean
    Protected Friend negate As Boolean

    Public Sub clone(obj_to_clone_to As cls_expression)
        With obj_to_clone_to
            .expression = expression
            .expr_rgx = expr_rgx
            .negate = negate
        End With
    End Sub
    Public Sub New()
        'pro potřeby klonování
    End Sub

    Public Sub New(ByVal pattern As String)
        expression = Trim(pattern)
    End Sub
    Public Property pattern As String
        Get
            pattern = expression
        End Get
        Set(value As String)
            expression = Trim(value)
        End Set
    End Property

    Public Function eval(element_to_evaluate As String, Optional rv As String = "", Optional ByRef mc As MatchCollection = Nothing) As Boolean

        Dim ex As String = expression
        Dim tmp As String = ""
        Dim p_lng As Long
        If ex(0) = "!" Then
            negate = True
            ex = Trim(Mid(ex, 2))
        End If
        If Left(ex, 4) = "left" Or Left(ex, 5) = "right" Then
            If rgx(ex, "left\s*\(\s*([0-9]+)\s*\).*", tmp) <> "" Then
                p_lng = -1
            End If
        ElseIf Left(ex, 5) = "rgxc " Then
            tmp = Mid(ex, 6)
            Dim rv_arr() As String

            If rgxx(element_to_evaluate, tmp, rv_arr,,, mc) <> "" Then
                eval = True
            Else
                eval = False
            End If
        ElseIf Left(ex, 5) = "rgxt " Then
            tmp = Mid(ex, 6)
            'Dim rv_arr() As String

            If rgxt(element_to_evaluate, tmp) = True Then
                eval = True
            Else
                eval = False
            End If
        End If
        If negate = True Then
            Return Not eval
        Else
            Return eval
        End If
    End Function
End Class
'####################################################################################################################################################################################################################

Public Class cls_reaction
    Public hgl_r As String
    Public rgx_replacement As String
    Public rgx_to_replace As String
    Public Sub New(highlight_rules As String, Optional rgxtoreplace As String = "", Optional rgxreplacement As String = "")
        hgl_r = highlight_rules
        rgx_replacement = rgxreplacement
        rgx_to_replace = rgxtoreplace
    End Sub
    Public Sub New()
        'pro případ klonování...
    End Sub
    Public Sub clone(obj_to_clone_into As cls_reaction)
        With obj_to_clone_into
            .hgl_r = hgl_r
            .rgx_replacement = rgx_replacement
            .rgx_to_replace = rgx_to_replace
        End With
    End Sub

    Public Function react(ByVal text As String, ByRef r_matches As MatchCollection) As String
        If Left(rgx_to_replace, 4) = "rgxr " Then
            Dim rgx_pattern As String
            rgx_pattern = Mid(rgx_to_replace, 5)
            Return rgxr(text, rgx_to_replace, rgx_replacement,, r_matches)
        Else
            Dim t As String
            If rgx_replacement = "" Then
                Return text
            ElseIf Left(rgx_replacement, 3) = "~D " Then 'smazání řádku
                Return ""
            ElseIf Left(rgx_replacement, 3) = "~Ab" Then 'přidá řádku před aktuální
                t = Mid(rgx_replacement, 5)
                Return t & Chr(10) & text
            ElseIf Left(rgx_replacement, 3) = "~Aa" Then 'přidá řádku za aktuální
                t = Mid(rgx_replacement, 5)
                Return text & Chr(10) & t
            ElseIf Left(rgx_replacement, 3) = "~^+" Then 'přidá text na začátek řádky
                t = Mid(rgx_replacement, 5)
                Return t & text
            ElseIf Left(rgx_replacement, 3) = "~$+" Then 'přidá text na konec řádky
                t = Mid(rgx_replacement, 5)
                If Right(text, 1) = Chr(10) Then
                Else
                    Return text & t
                End If
            Else
                Return text
            End If
        End If
    End Function
End Class

Public Class cls_executed_rule
    Public rule As cls_rule
    Public index As Long
    Public hgl As cls_highligh_rule
    Public Sub New(r As cls_rule, ind As Long, hglr As String)
        hgl = New cls_highligh_rule(hglr)
        index = ind
        rule = r
    End Sub
End Class

'####################################################################################################################################################################################################################

Public Class cls_executed_rules_collection
    Public er() As cls_executed_rule
    Public n As Long
    Public Sub New()
        n = -1
    End Sub
    Public Sub add(execr As cls_executed_rule)
        n = n + 1
        ReDim Preserve er(n)
        er(n) = execr
    End Sub
    Public Function get_all() As String()
        If n > -1 Then
            Dim i As Long
            Dim tmp() As String
            ReDim tmp(n)
            For i = 0 To n
                tmp(i) = er(i).rule.name & " {" & er(i).index & "}"
            Next
            Return tmp
        End If
    End Function
    Public Function get_from_name(name As String) As cls_executed_rule
        Dim i As Long
        For i = 0 To n
            If er(i).rule.name & " {" & er(i).index & "}" = name Then
                Return er(i)
            End If
        Next
    End Function
End Class