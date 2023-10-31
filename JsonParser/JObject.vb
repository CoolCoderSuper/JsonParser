Imports System.Collections.Immutable
Imports JsonParser.Parsing

Public Class JObject
    Inherits JValue

    Public ReadOnly Property Properties As IEnumerable(Of JProperty)
        Get
            Return Nodes.OfType(Of JProperty)
        End Get
    End Property

    Public Overrides Function WithNodes(nodes As IEnumerable(Of JToken)) As JToken
        Return New JObject With {._nodes = nodes.ToImmutableList}
    End Function

    Public Overrides Function AddNode(node As JToken) As JToken
        Return New JObject With {._nodes = _nodes.Add(node)}
    End Function

    Public Overrides Function AddNodes(nodes As IEnumerable(Of JToken)) As JToken
        Return New JObject With {._nodes = _nodes.AddRange(nodes)}
    End Function

    Public Overrides Function RemoveNode(node As JToken) As JToken
        Return New JObject With {._nodes = _nodes.Remove(node)}
    End Function

    Public Overrides Function ReplaceNode(oldNode As JToken, newNode As JToken) As JToken
        Return New JObject With {._nodes = _nodes.Replace(oldNode, newNode)}
    End Function
End Class