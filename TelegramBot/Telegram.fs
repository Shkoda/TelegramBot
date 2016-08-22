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
  let editInlineKeyboardEndpoint token = endpoint token "editMessageReplyMarkup"
  let editMessageEndpoint token = endpoint token "editMessageText"


  let nextOffset (results:Json.Update.Result[]) = 
     let updateId (result: Json.Update.Result) = result.UpdateId    
     results |> Array.map updateId |> Array.tryLast |> Option.fold (fun _ i ->increment i) 0


  let getUpdatesBackground = fun() ->
      let rec getUpdatesBackground token offset =

        async {
            try 
                let url = updateEndpoint token 
                let! responseString = Http.AsyncRequestString(url, query=["offset", offset.ToString()])
                let update = responseString |> Json.Update.Parse |> (fun root -> root.Result) 
                updateReceived.Trigger update

                sleep 100 

                let offset = nextOffset update
                do! getUpdatesBackground token offset       
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

  let showReplyMarkupKeyboard chatId text =
    let url  = sendMessageEndpoint TOKEN 
    try
        let keyboard =  "{\"keyboard\":[[\"Hello\"], [\"Sun\", \"Moon\"], [\"11\",\"22\",\"33\",\"44\",\"55\"]], \"one_time_keyboard\": true,\"resize_keyboard\": true}"
        Http.RequestString (url, 
            query=[
                "chat_id", chatId.ToString(); 
                "text", text; 
                "reply_markup", keyboard;
            ])
        |> ignore 
    with
    | :? System.Net.WebException as ex-> printfn "%s Telegram.fs:(%s) %s" HTTPEXN __LINE__  ex.Message|> ignore
    | _-> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore

  let hideReplyMarkupKeyboard chatId text = 
    try
        Http.RequestString (sendMessageEndpoint TOKEN, 
             query=["chat_id", chatId.ToString(); "text", text; "reply_markup", "{\"hide_keyboard\":true}";])|> ignore 
    with
     | :? System.Net.WebException as ex-> printfn "%s Telegram.fs:(%s) %s" HTTPEXN __LINE__  ex.Message|> ignore
     | _  -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore


  let showInlineKeyboard chatId text =
    let url  = sendMessageEndpoint TOKEN 
    try
        let keyboard =  "{\"inline_keyboard\":[[{\"text\":\"alpha\",\"callback_data\":\"this is callback data\"}]]}"
        Http.RequestString (url, 
            query=[
                "chat_id", chatId.ToString(); 
                "callback_data", "this is callback";
                "text", text; 
                "reply_markup", keyboard;
            ])
        |> ignore 
    with
    | :? System.Net.WebException as ex-> printfn "%s Telegram.fs:(%s) %s" HTTPEXN __LINE__  ex.Message|> ignore
    | _-> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore

  let toInlineLeyboard (buttons: DataClasses.InlineButton[][]) = 
    let buttonMarkup (button: DataClasses.InlineButton) = 
       sprintf "{\"text\": \"%s\", \"callback_data\": \"%s\"}" button.text button.callback
    let rowMarkup (buttons:DataClasses.InlineButton[]) = 
       sprintf "[%s]" (String.concat "," (buttons |> Array.map buttonMarkup))
    let keyboardMarkup (buttons: DataClasses.InlineButton[][]) = 
       sprintf "[%s]" (String.concat "," (buttons |> Array.map rowMarkup))
    sprintf "{\"inline_keyboard\":%s}" (keyboardMarkup buttons)
    
  
  let showConfigurableInlineKeyboard chatId (buttons: DataClasses.InlineButton[][]) = 
     let url  = sendMessageEndpoint TOKEN     
     try
        let markup = toInlineLeyboard buttons
        Http.RequestString (url, 
            query=[
                "chat_id", chatId.ToString(); 
                "callback_data", "this is callback";
                "text", "^_^"; 
                "reply_markup", markup;
            ])
        |> ignore 
     with
        | :? System.Net.WebException as ex-> printfn "%s Telegram.fs:(%s) %s" HTTPEXN __LINE__  ex.Message|> ignore
        | _-> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore

  let editInlineKeyboard chatId messageId keyboard = 
    let url  = editInlineKeyboardEndpoint TOKEN 
    try
          Http.RequestString (url, query=["chat_id", chatId.ToString(); "message_id", messageId.ToString(); "reply_markup", keyboard]) |> ignore 
    with
          | :? System.Net.WebException -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore
          | _                          -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore

  let hideInlineKeyboard chatId messageId =
     editInlineKeyboard chatId messageId ""

  let overrideInlineKeyboard chatId messageId (buttons: DataClasses.InlineButton[][]) = 
    editInlineKeyboard chatId messageId (toInlineLeyboard buttons)

  let editMessage chatId messageId newText = 
    let url  = editMessageEndpoint TOKEN 
    try
          Http.RequestString (url, query=["chat_id", chatId.ToString(); "message_id", messageId.ToString(); "text", newText]) |> ignore 
    with
          | :? System.Net.WebException -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore
          | _                          -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore
    