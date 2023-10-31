Imports System.Text

Namespace Parsing
    Public Class Lexer
        Private ReadOnly _text As String
        Private _position As String
        Private ReadOnly _diagnostics As New List(Of String)

        Public Sub New(text As String)
            _text = text
        End Sub

        Public ReadOnly Property Diagnostics As IEnumerable(Of String)
            Get
                Return _diagnostics.AsEnumerable
            End Get
        End Property

        Private ReadOnly Property Current As Char
            Get
                If _position >= _text.Length Then Return vbNullChar
                Return _text(_position)
            End Get
        End Property

        Private Function Peek(num As Integer) As Char
            If _position + num >= _text.Length Then Return vbNullChar
            Return _text(_position + num)
        End Function

        Private Sub [Next]()
            _position += 1
        End Sub

        Private Sub Skip(num As Integer)
            _position += num
        End Sub

        Public Function Lex() As SyntaxToken
            If _position >= _text.Length Then Return New SyntaxToken(SyntaxKind.EndOfFileToken, _position, vbNullChar, Nothing)
            Dim leadingTrivia As SyntaxToken() = CollectTrivia()
            Dim token As SyntaxToken
            'operators
            Select Case Current
                Case "["
                    [Next]()
                    token = New SyntaxToken(SyntaxKind.OpenBracketToken, _position, "[", Nothing)
                Case "]"
                    [Next]()
                    token = New SyntaxToken(SyntaxKind.CloseBracketToken, _position, "]", Nothing)
                Case "{"
                    [Next]()
                    token = New SyntaxToken(SyntaxKind.OpenBraceToken, _position, "{", Nothing)
                Case "}"
                    [Next]()
                    token = New SyntaxToken(SyntaxKind.CloseBraceToken, _position, "}", Nothing)
                Case ","
                    [Next]()
                    token = New SyntaxToken(SyntaxKind.CommaToken, _position, ",", Nothing)
                Case ":"
                    [Next]()
                    token = New SyntaxToken(SyntaxKind.ColonToken, _position, ":", Nothing)
            End Select
            If token IsNot Nothing Then
                'skip all other scenarios
                'numbers
            ElseIf MatchesInfinity() Then
                If Current = "+" Then [Next]()
                Skip(8)
                token = New SyntaxToken(SyntaxKind.InfinityLiteralToken, _position, "Infinity", Double.PositiveInfinity)
            ElseIf MatchesNegativeInfinity() Then
                Skip(9)
                token = New SyntaxToken(SyntaxKind.NegativeInfinityLiteralToken, _position, "-Infinity", Double.NegativeInfinity)
            ElseIf MatchesNaN() Then
                Skip(3)
                Return New SyntaxToken(SyntaxKind.NaNLiteralToken, _position, "NaN", Double.NaN)
            ElseIf Char.IsDigit(Current) OrElse Current = "." OrElse Current = "-" OrElse Current = "+" Then
                Dim start As Integer = _position
                While Char.IsDigit(Current) OrElse Current = "." OrElse Current = "-" OrElse Current = "+" OrElse Current = "e" OrElse Current = "E"
                    [Next]()
                End While
                Dim length As Integer = _position - start
                Dim text As String = _text.Substring(start, length)
                Dim value As Object
                If text.Contains(".") Then
                    Dim numberText As String = text
                    Dim exponent As Integer = 0
                    If text.ToLower.Contains("e") Then
                        Dim parts As String() = text.Split("e")
                        numberText = parts(0)
                        exponent = Integer.Parse(parts(1))
                    End If
                    If Not Decimal.TryParse(numberText, value) Then
                        _diagnostics.Add($"The number {_text} isn't valid Decimal.")
                    Else
                        If exponent <> 0 Then value *= 10 ^ exponent
                    End If
                Else
                    If Not Integer.TryParse(text, value) AndAlso Not Long.TryParse(text, value) Then _diagnostics.Add($"The number {text} isn't valid Int32 or Int64.")
                End If
                token = New SyntaxToken(SyntaxKind.NumberLiteralToken, start, text, value)
                'string literal
            ElseIf Current = """" OrElse Current = "'" Then
                Dim start As Integer = _position
                Dim startChar As Char = Current
                [Next]()
                While (Current = startChar AndAlso Peek(-1) = "\") OrElse Current <> startChar
                    [Next]()
                End While
                [Next]()
                Dim length As Integer = _position - start
                Dim text As String = _text.Substring(start, length)
                Dim rx As New RegularExpressions.Regex("\\[uU]([0-9A-Fa-f]{4})")
                Dim valueText As String = rx.Replace(text.Remove(0, 1).Remove(text.Length - 2, 1), Function(match) ChrW(Integer.Parse(match.Value.Substring(2), Globalization.NumberStyles.HexNumber)).ToString()).Replace("\", "")
                token = New SyntaxToken(SyntaxKind.StringLiteralToken, start, text, valueText)
                'literals
            ElseIf MatchesTrueKeyword() Then
                Dim start As Integer = _position
                Skip(4)
                token = New SyntaxToken(SyntaxKind.TrueLiteralToken, start, "true", True)
            ElseIf MatchesFalseKeyword() Then
                Dim start As Integer = _position
                Skip(5)
                token = New SyntaxToken(SyntaxKind.FalseLiteralToken, start, "false", False)
            ElseIf MatchesNullKeyword() Then
                Dim start As Integer = _position
                Skip(4)
                token = New SyntaxToken(SyntaxKind.NullLiteralToken, start, "null", Nothing)
            Else
                'identifiers
                Dim identifierStart As Integer = _position
                While Current <> ":" AndAlso Not MatchesTrivia()
                    [Next]()
                End While
                Dim identifierLength As Integer = _position - identifierStart
                If identifierLength > 0 Then
                    Dim identifierText As String = _text.Substring(identifierStart, identifierLength)
                    token = New SyntaxToken(SyntaxKind.IdentifierToken, identifierStart, identifierText, Nothing)
                Else
                    _diagnostics.Add($"ERROR: Bad character input: '{Current}'")
                    [Next]()
                    Return New SyntaxToken(SyntaxKind.BadToken, _position, _text.Substring(_position - 1, 1), Nothing)
                End If
            End If
            Return token.WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(CollectTrivia())
        End Function

#Region "Trivia"
        Private Function MatchesTrivia() As Boolean
            Return MatchesSingleLineComment() OrElse MatchesMultiLineComment() OrElse MatchesWhitespace()
        End Function
        Private Function MatchesSingleLineComment() As Boolean
            Return Current = "/" AndAlso Peek(1) = "/"
        End Function
        Private Function MatchesMultiLineComment() As Boolean
            Return Current = "/" AndAlso Peek(1) = "*"
        End Function
        Private Function MatchesWhitespace() As Boolean
            Return Char.IsWhiteSpace(Current)
        End Function
        Private Function CollectTrivia() As SyntaxToken()
            Dim trivia As New List(Of SyntaxToken)
            While MatchesTrivia()
                'single line comments
                If MatchesSingleLineComment() Then
                    Dim start As Integer = _position
                    [Next]()
                    [Next]()
                    While Current <> vbNullChar AndAlso Current <> vbLf
                        [Next]()
                    End While
                    Dim length As Integer = _position - start
                    Dim text As String = _text.Substring(start, length)
                    trivia.Add(New SyntaxToken(SyntaxKind.SingleLineCommentToken, start, text, Nothing))
                End If
                'block comments
                If MatchesMultiLineComment() Then
                    Dim start As Integer = _position
                    [Next]()
                    [Next]()
                    While Current <> vbNullChar AndAlso Not (Current = "*" AndAlso Peek(1) = "/")
                        [Next]()
                    End While
                    [Next]()
                    [Next]()
                    Dim length As Integer = _position - start
                    Dim text As String = _text.Substring(start, length)
                    trivia.Add(New SyntaxToken(SyntaxKind.BlockCommentToken, start, text, Nothing))
                End If
                'whitespace
                If MatchesWhitespace() Then
                    Dim start As Integer = _position
                    While Char.IsWhiteSpace(Current)
                        [Next]()
                    End While
                    Dim length As Integer = _position - start
                    Dim text As String = _text.Substring(start, length)
                    trivia.Add(New SyntaxToken(SyntaxKind.WhitespaceToken, start, text, Nothing))
                End If
            End While
            Return trivia.ToArray()
        End Function
#End Region

#Region "Numbers"
        Private Function MatchesNaN() As Boolean
            Return MatchWord("NaN")
        End Function

        Private Function MatchesInfinity() As Boolean
            Dim start As Integer = 0
            If Current = "+" Then
                start += 1
            End If
            Return MatchWord("Infinity", start)
        End Function

        Private Function MatchesNegativeInfinity() As Boolean
            Return MatchWord("-Infinity")
        End Function
#End Region

#Region "Keywords"
        Private Function MatchesKeyword() As Boolean
            Return MatchesNullKeyword() OrElse MatchesTrueKeyword() OrElse MatchesFalseKeyword()
        End Function

        Private Function MatchesNullKeyword() As Boolean
            Return MatchWord("null")
        End Function

        Private Function MatchesTrueKeyword() As Boolean
            Return MatchWord("true")
        End Function

        Private Function MatchesFalseKeyword() As Boolean
            Return MatchWord("false")
        End Function
#End Region

        Private Function MatchWord(word As String, Optional i As Integer = 0) As Boolean
            While i < word.Length AndAlso Peek(i) = word(i)
                i += 1
            End While
            Return i = word.Length
        End Function
    End Class
End Namespace