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
  let updateEndpoint = endpoint TOKEN "getUpdates"
  let sendMessageEndpoint = endpoint TOKEN "sendMessage"
  let editInlineKeyboardEndpoint = endpoint TOKEN "editMessageReplyMarkup"
  let editMessageEndpoint = endpoint TOKEN "editMessageText"

  let nextOffset (results:Json.Update.Result[]) = 
     let updateId (result: Json.Update.Result) = result.UpdateId    
     results |> Array.map updateId |> Array.tryLast |> Option.fold (fun _ i ->increment i) 0

  let getUpdatesBackground = fun() ->
      let rec getUpdatesBackground token offset =

        async {
            try 
                let url = updateEndpoint  
                let! responseString = Http.AsyncRequestString(url, query=["offset", offset.ToString()])
                let update = responseString |> Json.Update.Parse |> (fun root -> root.Result) 
                updateReceived.Trigger update

                sleep 100 

                let offset = nextOffset update
                do! getUpdatesBackground token offset       
            with
            | :? System.Net.WebException -> printfn "%s (%s.%s)" HTTPEXN __SOURCE_FILE__ __LINE__ ;  Async.Start (getUpdatesBackground token 0)  
            | _                          -> printfn "%s (%s.%s)" JSONEXN __SOURCE_FILE__ __LINE__ ; Async.Start (getUpdatesBackground token 0)  
        }   
      getUpdatesBackground TOKEN 0 

  let sendQuery url query = 
     try
          Http.RequestString (url, query = query) |> ignore 
     with
          | _ -> printfn "%s (%s.%s)" HTTPEXN __SOURCE_FILE__ __LINE__ |> ignore

  let sendMessage chatId text =      
        let query = ["chat_id", chatId.ToString(); "text", text; "parse_mode", "Markdown"]
        sendQuery sendMessageEndpoint query

  let editMessage chatId messageId newText = 
    let query = ["chat_id", chatId.ToString(); "message_id", messageId.ToString(); "text", newText]
    sendQuery editMessageEndpoint query

  let showReplyMarkupKeyboard chatId text buttons =
    let keyboard = TelegramMarkdown.toMarkupKeyboard buttons
    let query = ["chat_id", chatId.ToString(); "text", text; "reply_markup", keyboard]
    sendQuery sendMessageEndpoint query

  let hideReplyMarkupKeyboard chatId text = 
    let query = ["chat_id", chatId.ToString(); "text", text; "reply_markup", "{\"hide_keyboard\":true}"]
    sendQuery sendMessageEndpoint query
 
  let showConfigurableInlineKeyboard chatId (buttons: DataClasses.InlineButton[][]) = 
     let markup = TelegramMarkdown.toInlineLeyboard buttons
     let query = ["chat_id", chatId.ToString(); "text", "^_^";"reply_markup", markup]
     sendQuery sendMessageEndpoint query

  let editInlineKeyboard chatId messageId keyboard = 
    let query = ["chat_id", chatId.ToString(); "message_id", messageId.ToString(); "reply_markup", keyboard]
    sendQuery editInlineKeyboardEndpoint query

  let hideInlineKeyboard chatId messageId =
     editInlineKeyboard chatId messageId ""

  let overrideInlineKeyboard chatId messageId (buttons: DataClasses.InlineButton[][]) = 
    editInlineKeyboard chatId messageId (TelegramMarkdown.toInlineLeyboard buttons)

