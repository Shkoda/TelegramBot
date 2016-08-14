namespace Shkoda.Telegram.Bot

module Telegram =
  open FSharp.Data
  open System
  open System.Threading
  let TOKEN  = "231953668:AAHUQ8HEQr8Scnl_ViDZ6dWtH9JXDeMy5hw"

  let JSONEXN = "An issue was encountered parsing the JSON object."
  let HTTPEXN = "An issue was encountered contacting TELEGRAM server."

  let sleep (x: int) = System.Threading.Thread.Sleep x

  let updateReceived = new Event<Json.Update.Result[]>()
  let UpdateSubscribers = updateReceived.Publish

  let increment x = x + 1

  let endpoint token path = "https://api.telegram.org/bot" + token + "/" + path
  let updateEndpoint token =  endpoint token "getUpdates"
  let sendMessageEndpoint token =  endpoint token "sendMessage"

  let getUpdatesBackground = fun() ->
      let rec getUpdatesBackground token offset =

        async {
            try 
                let url = updateEndpoint token 
                let! responseString = Http.AsyncRequestString(url, query=["offset", offset.ToString()])
                let update =  responseString |> Json.Update.Parse 
                updateReceived.Trigger update.Result
            
      
                let nextOffset = match update.Result with
                                 | r when Seq.isEmpty r = false    
                                    -> r |> Array.map (fun elem -> elem.UpdateId) |> Array.last |> increment 
                                 | _ -> 0
                sleep 100 

                do! getUpdatesBackground token nextOffset          
            with
            | :? System.Net.WebException -> printfn "%s (%s)" HTTPEXN __LINE__;  Async.Start (getUpdatesBackground token 0)  
            | _                          -> printfn "%s (%s)" JSONEXN __LINE__; Async.Start (getUpdatesBackground token 0)  
        }   
      getUpdatesBackground TOKEN 0 
  
  let sendMessage chatId text =        
      let sendMessage token chatId body =
        let url  = sendMessageEndpoint token 
        try
          Http.RequestString (url, query=["chat_id", chatId.ToString(); "text", body; "parse_mode", "Markdown"]) |> ignore 
        with
          | :? System.Net.WebException -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore
          | _                          -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore
      sendMessage TOKEN chatId text

  let keyboard chatId text =
    let url  = sendMessageEndpoint TOKEN 

  //  let replyKeyboardMakeup = "{\"keyboard\": [[\"a\",\"b\"],[\"c\",\"d\"]], \"resize_keyboard\": False, \"one_time_keyboard\": True}"
    try
        Http.RequestString (url, 
            query=[
                "chat_id", chatId.ToString(); 
                "text", text; 
                "reply_markup", "{\"replyKeyboardMarkup\":{\"keyboard\":[[\"a\"], [\"b\"]]}}";
               // "{\"replyKeyboardMarkup\":{\"keyboard\":[[{\"text\":\"a\"}],[{\"text\":\"b\"}]],\"resize_keyboard\":\"true\",            \"one_time_keyboard\":\"true\"}}";
                 

            ])
        |> ignore 
    with
    | :? System.Net.WebException as ex-> printfn "%s Telegram.fs:(%s) %s" HTTPEXN __LINE__  ex.Message|> ignore
    | _                          -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore
