open System
open System.Net
open System.Net.Sockets
open System.IO
open System.Collections.Generic

// Define IP Address and port number
let ipAddress = IPAddress.Parse("127.0.0.1")
let port = 9001
let mutable isServerUp = true

// Initialize a TCP Listener and start
let listener = new TcpListener(IPAddress.Any,port)
listener.Start()
printfn "Server is running and listening on port %d" port

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

let connectedClients = new List<TcpClient>()

let sendResponse (client:TcpClient,response:string) = 
    let serverResponse = "Responding to client "+(client.Client.Handle.ToInt32()|>string) + " with result: "+response
    printfn "%s" serverResponse
    client.GetStream().Write(System.Text.Encoding.ASCII.GetBytes(response))

let readRequest (client:TcpClient) =
    let buffer = Array.zeroCreate 256
    let read = client.GetStream().Read(buffer,0,256);
    System.Text.Encoding.ASCII.GetString(buffer)

let terminateAllClients () = 
    for client in connectedClients do
        sendResponse (client,"-5")
        
        // client.Close()
    
let serveClient (client: TcpClient) = async { 
    
    connectedClients.Add(client)

    // Get client stream to read and write
    let stream = client.GetStream()

    // Send Hello message to client
    sendHelloToClient stream
    let mutable connectionOpen = true

    // Continue accepting the requests till the connection is Open
    while connectionOpen do

        // Read the input sent by the client 
        let input = readRequest client
        printfn "Received: %s" input
        let arguments = input.Trim().Split (' ') 
        let command = arguments[0]

        // If the command is terminate then send the code -5 and close the connection
        if command.Contains("terminate") then
            sendResponse (client,"-5")
            // terminateAllClients ()
            connectionOpen <- false
            isServerUp <-false

        // Else perform the command ( add,subtract,multiply and bye)
        else 
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
            elif command.Contains("bye") then
                connectedClients.Remove(client)|>ignore
                response <- "-5"
                connectionOpen <- false

            // Send response to the client.
            sendResponse (client,response)
    // client.Close()
}


// Accept clients 
while isServerUp do
    let client = listener.AcceptTcpClient()

    // After accepting client aynchronously start processing client
    Async.Start(serveClient (client))

listener.Stop()