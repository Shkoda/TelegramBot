namespace Shkoda.Telegram.Bot

module Main =
  open FSharp.Data
  open System
  open System.Threading

  let TOKEN  = "231953668:AAHUQ8HEQr8Scnl_ViDZ6dWtH9JXDeMy5hw"
  let PREFIX = "/"

 
  let handle (update : Json.Update.Result) =
    let reply text = Telegram.sendMessage TOKEN  update.Message.Chat.Id text
    let sendCommits (commits:BitBucket.ShortCommitInfo[]) = 
       let s = commits |> Array.map (fun c -> sprintf "%s" c.message ) |> String.concat ("\n")
       reply s
   
    let match_entity (e: Json.Update.Entity) =
        let sender =  update.Message.From.FirstName
        let value = update.Message.Text.Substring(e.Offset, e.Length)
        match e.Type with
        |"mention" ->  reply ("you've mentioned "+ value)
        |"bot_command" -> 
            let command = value
            match command with
             |"/hi"|"/hello" -> reply ("Nice to meet you, " + sender)
             |"/git" -> sendCommits (BitBucket.getCommitList "email" "pass")
             |_ -> reply (sprintf "I don't know %s command, %s" command sender)
        |"hashtag" -> reply ("you've used hashtag " + value)
        |_ -> ignore()

    Console.WriteLine (sprintf "from %s: %s" update.Message.Chat.Username update.Message.Text)
    Array.iter (fun e -> match_entity e) update.Message.Entities

  let handleUpdates (updates: Json.Update.Result[]) = 
     Array.iter (fun res -> handle res) updates

  [<EntryPoint>]
  let main args =
  //  HttpServer.startIt()
    Telegram.UpdateSubscribers.Add(handleUpdates)
    Async.Start (Telegram.getUpdatesBackground TOKEN 0)
    while true do
        Console.ReadLine()
    0