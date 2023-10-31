Imports JsonParser.Parsing
Imports JsonParser

Public Module Program
    Public Sub Main(args As String())
        Dim p As New Parser(<json>
                                {
    "maxInt32": NaN
}

                            </json>)
        Dim obj As JToken = p.Parse
        obj.PrettyPrint()
        Console.WriteLine(obj)
        Dim fore As ConsoleColor = Console.ForegroundColor
        Console.ForegroundColor = ConsoleColor.Red
        For Each diag As String In p.Diagnostics
            Console.WriteLine(diag)
        Next
        Console.ForegroundColor = fore
        Console.ReadKey()
    End Sub
End Module