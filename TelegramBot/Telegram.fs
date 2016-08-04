namespace Shkoda.Telegram.Bot

module Telegram =
  open FSharp.Data
  open FSharp.Data.JsonExtensions
  open System

  let JSONEXN = "An issue was encountered parsing the JSON object."
  let HTTPEXN = "An issue was encountered contacting the server."

  //  (?=) :: JsonValue -> a -> option JsonValue 
  let (?=) (value: JsonValue) prop =
    value.TryGetProperty prop

  let succ x = x + 1

  let (>>=) a f =
    Option.bind f a

  let getEndpoint token path = 
    "https://api.telegram.org/bot" + token + "/" + path

  let getUpdatesJson token =
    try 
      getEndpoint token "getUpdates" |> 
        Http.RequestString |> JsonParser.Update.Parse |> Some
    with
      | :? System.Net.WebException -> printfn "%s (%s)" HTTPEXN __LINE__ ; None
      | _                          -> printfn "%s (%s)" JSONEXN __LINE__ ; None

  let getUpdatesJson0 token offset=
    try 
      let url = getEndpoint token "getUpdates"
      let soffset = sprintf "%d" offset
      Http.RequestString (url, query=["offset", soffset]) |> JsonParser.Update.Parse |> Some
    with
      | :? System.Net.WebException -> printfn "%s (%s)" HTTPEXN __LINE__ ; None
      | _                          -> printfn "%s (%s)" JSONEXN __LINE__ ; None    


  let sendMessage (token: string) (chatId: int) (body: string) =
    let url  = getEndpoint token "sendMessage"
    let stringChatId = sprintf "%d" chatId
    try
      Http.RequestString (url, query=["chat_id", stringChatId; "text", body]) |> ignore 
    with
      | :? System.Net.WebException -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore
      | _                          -> printfn "%s (%s)" HTTPEXN __LINE__ |> ignore


  let getNewId (responce: JsonParser.Update.Root option) = 
       let update_id (result:JsonParser.Update.Result) =  result.UpdateId
       match responce with
       |Some r -> 
            match Seq.isEmpty r.Result with
            | true -> None
            | false -> r.Result |> Seq.map update_id |> Seq.last |> succ |> Some
        |None -> None
        

