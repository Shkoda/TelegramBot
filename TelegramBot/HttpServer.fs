namespace Shkoda.Telegram.Bot
module HttpServer =
    open System
    open System.Net
    open System.Text
    open System.IO

    let host = "http://localhost:8080/"
 
    let listener (handler:(HttpListenerRequest->HttpListenerResponse->Async<unit>)) =
        let hl = new HttpListener()
        hl.Prefixes.Add host
        hl.Start()
        let task = Async.FromBeginEnd(hl.BeginGetContext, hl.EndGetContext)
        async {
            while true do
                let! context = task
                Async.Start(handler context.Request context.Response)
        } |> Async.Start
 
    let output (req:HttpListenerRequest) =
     "ololo" 

    let startIt = fun () ->
        listener (fun req resp ->
            async {
                Console.WriteLine (sprintf "request from %s" req.Url.AbsoluteUri)
                let txt = Encoding.ASCII.GetBytes(output req)
            
                resp.ContentType <- "text/html"
                resp.OutputStream.Write(txt, 0, txt.Length)
                resp.OutputStream.Close()
            })


