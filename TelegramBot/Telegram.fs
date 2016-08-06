namespace Shkoda.Telegram.Bot

module Telegram =
  open FSharp.Data

  let JSONEXN = "An issue was encountered parsing the JSON object."
  let HTTPEXN = "An issue was encountered contacting the server."

  let increment x = x + 1

  let endpoint token path = "https://api.telegram.org/bot" + token + "/" + path
  let updateEndpoint token =  endpoint token "getUpdates"
  let sendMessageEndpoint token =  endpoint token "sendMessage"

  let getUpdates token offset =
    try 
      let url = updateEndpoint token 
      let response = Http.RequestString (url, query=["offset", offset.ToString()]) |> Json.Update.Parse 
      
      let offset = match response.Result with
                    | r when Seq.isEmpty r = false    
                        -> r |> Array.map (fun elem -> elem.UpdateId) |> Array.last |> increment 
                    | _ -> 0

      response |> Some, offset
    with
      | :? System.Net.WebException -> printfn "%s (%s)" HTTPEXN __LINE__ ; None, 0
      | _                          -> printfn "%s (%s)" JSONEXN __LINE__ ; None, 0    
 
         
  let sendMessage token chatId body =
    let url  = sendMessageEndpoint token 
    try
      Http.RequestString (url, query=["chat_id", chatId.ToString(); "text", body]) |> ignore 
    with
      | :? System.Net.WebException -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore
      | _                          -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore

