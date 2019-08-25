Imports System
Imports Newtonsoft.Json
Imports System.Data
Imports System.Net.Http
Imports System.Text
Imports System.Linq
Imports MySql.Data.MySqlClient

Namespace integrationExampleUsingCsharp
    Class Program
        Private Const RequestUri As String = "http://139.59.151.199/api/sync_store_products_by_key" ' Endpoint URL
        Private Const Token As String = "JWT Token" ' JWT Token
        Private Const query As String = "select * from users" ' Query String

        Public Shared Sub Main(ByVal args As String())
            Dim connStr As String = "server=localhost;database=dotnet;uid=root;pwd=root" ' Connection String

            Using conn As MySqlConnection = New MySqlConnection(connStr)

                Try
                    Console.WriteLine("Connecting To MySql ...")

                    Using da As MySqlDataAdapter = New MySqlDataAdapter() ' Generate The MySql Adapter

                        Using dt As DataTable = New DataTable()

                            Using sqlCommand As MySqlCommand = conn.CreateCommand() ' Preparing MySql Command
                                sqlCommand.CommandType = CommandType.Text
                                sqlCommand.CommandText = query
                                da.SelectCommand = sqlCommand ' Execute MySql Command
                                da.Fill(dt) ' Dump Result into DataTable
                                sqlCommand.Dispose()
                                da.Dispose()
                                Console.WriteLine("Preparing Batches")
                                Dim tr = dt.Rows.Count ' Get Total Rows Count
                                tr = CInt(Math.Ceiling(CDbl(tr) / 10)) ' Divide total by the batches
                                Dim skipCount = 0

                                For i As Integer = 0 To tr - 1 ' Loop through the total table rows
                                    Console.WriteLine("Sending " & (i + 1) & " Batch")
                                    Dim dataBatch = dt.AsEnumerable().Skip(skipCount).Take(10) ' Take
                                    Dim copyTableData = dataBatch.CopyToDataTable() ' Copy Batches to New DataTable
                                    Dim jsonResult = JsonConvert.SerializeObject(copyTableData) ' Serialize Copied Data To JSON Object
                                    sendRequest(jsonResult)
                                    skipCount += 10
                                Next
                            End Using
                        End Using
                    End Using

                Catch e As Exception
                    Console.WriteLine(e.Message)
                End Try
            End Using

            Console.WriteLine("")
            Console.WriteLine("Done ...")
        End Sub

        Private Shared Sub sendRequest(ByVal data As String)
            Try

                Using client As HttpClient = New HttpClient()
                    client.DefaultRequestHeaders.Add("Authorization", Token) ' Add Authorization Header

                    Using content As HttpContent = New StringContent(data, Encoding.UTF8, "application/json") ' Prepare JSON Payload To Send

                        Using response As HttpResponseMessage = client.PostAsync(RequestUri, content).Result ' Send Http Request And Store Result In (response)
                            Console.WriteLine(response.Content.ReadAsStringAsync().Result) ' Print Out Response
                            Console.WriteLine("Batch Sent")
                        End Using
                    End Using
                End Using

            Catch e As Exception
                Console.WriteLine(e.Message)
            End Try
        End Sub
    End Class
End Namespace
