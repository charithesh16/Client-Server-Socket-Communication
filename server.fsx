open System
open System.Net
open System.Net.Sockets
open System.IO;

let sendHelloToClient(stream: NetworkStream) = 
    let message = "Hello Client, How may I help you?\n"
    let bytes = System.Text.Encoding.ASCII.GetBytes(message)
    stream.Write(bytes)

let performAdd(arguments: int array) =
    Array.sum arguments
    |>string

let performSubtract(arguments: int array) =
    Array.reduce (-) arguments
    |>string

let performMultiplication(arguments: int array) =
    Array.reduce (*) arguments
    |>string

let serveClient (client: TcpClient) = async { 
    let stream = client.GetStream()
    sendHelloToClient stream
    let mutable connectionOpen = true
    while connectionOpen do
        let buffer = Array.zeroCreate 256
        let read = stream.Read(buffer,0,256);
        let input = System.Text.Encoding.ASCII.GetString(buffer)
        printfn "Received: %s" input
        let arguments = input.Trim().Split (' ') 
        let command = arguments[0]
        let values = arguments[1..]
        // TODO validate input only then process
        let cleanedValues = Array.map int values
        let mutable response = ""
        if command.Equals("add") then
            response <- performAdd(cleanedValues)
        elif command.Equals("subtract") then 
            response <- performSubtract(cleanedValues)
        elif command.Equals("multiply") then
            response <- performMultiplication (cleanedValues)
        elif command.Equals("bye") then
            printfn "bye bye client"
            response <- "bye"
            // connectionOpen <- false
            

        // printfn " command %s" command
        // printfn "values size %d" values.Length
        // printfn "Data received from client %s" input
        let serverResponse = "Responding to client "+(client.Client.Handle.ToInt32()|>string) + " with result: "+response
        printfn "%s" serverResponse
        stream.Write(System.Text.Encoding.ASCII.GetBytes(response))
    client.Close()
}


let ipAddress = IPAddress.Parse("127.0.0.1")
let port = 9001
let isServerUp = true
let listener = new TcpListener(IPAddress.Any,port)
listener.Start()
printfn "Server is running and listening on port %d" port
while isServerUp do
    let client = listener.AcceptTcpClient()
    Async.Start(serveClient (client))