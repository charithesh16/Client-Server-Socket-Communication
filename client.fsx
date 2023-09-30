open System
open System.Net
open System.Net.Sockets
open System.IO;

let sendCommandsToServer (stream: NetworkStream,buffer:byte array) = 
    printf "Sending command: " 
    let inputFromUser = Console.ReadLine()
    stream.Write(System.Text.Encoding.ASCII.GetBytes(inputFromUser))
    let buff = Array.zeroCreate 256
    stream.Read(buff,0,256) |> ignore
    System.Text.Encoding.ASCII.GetString(buff)

let serverAddress = "127.0.0.1"
let serverPort = 9001

let client = new TcpClient(serverAddress,serverPort)
let mutable isClientUp = true
let buffer = Array.zeroCreate 256
let stream = client.GetStream()
let data = stream.Read(buffer,0,256)
let message = System.Text.Encoding.ASCII.GetString(buffer)
printf "%s" message
while isClientUp do
    let response = sendCommandsToServer(stream,buffer)
    if response.Equals("bye") then
        isClientUp <- false
    printfn "Server response: %s" response

client.Close()