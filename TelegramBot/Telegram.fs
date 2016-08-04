namespace Shkoda.Telegram.Bot

module Telegram =
  open FSharp.Data

  let JSONEXN = "An issue was encountered parsing the JSON object."
  let HTTPEXN = "An issue was encountered contacting the server."

  let succ x = x + 1

  let endpoint token path = "https://api.telegram.org/bot" + token + "/" + path
  let updateEndpoint token =  endpoint token "getUpdates"
  let sendMessageEndpoint token =  endpoint token "sendMessage"

  let getUpdates token offset =
    try 
      let url = updateEndpoint token 
      Http.RequestString (url, query=["offset", offset.ToString()]) |> JsonParser.Update.Parse |> Some
    with
      | :? System.Net.WebException -> printfn "%s (%s)" HTTPEXN __LINE__ ; None
      | _                          -> printfn "%s (%s)" JSONEXN __LINE__ ; None    
 
  let getNewId (responce: JsonParser.Update.Root option) = 
       let update_id (result:JsonParser.Update.Result) =  result.UpdateId
       match responce with
       |Some r -> 
            match Seq.isEmpty r.Result with
            | true -> 0
            | false -> r.Result |> Seq.map update_id |> Seq.last |> succ 
        |None -> 0
        
  let sendMessage token chatId body =
    let url  = sendMessageEndpoint token 
    try
      Http.RequestString (url, query=["chat_id", chatId.ToString(); "text", body]) |> ignore 
    with
      | :? System.Net.WebException -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore
      | _                          -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore

