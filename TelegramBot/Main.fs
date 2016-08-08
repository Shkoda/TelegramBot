namespace Shkoda.Telegram.Bot

module Main =
  open FSharp.Data
  open System

  let TOKEN  = "231953668:AAHUQ8HEQr8Scnl_ViDZ6dWtH9JXDeMy5hw"
  let PREFIX = "/"

  let sleep (x: int) = System.Threading.Thread.Sleep x



  let handle (update : Json.Update.Result) =
    let reply text = Telegram.sendMessage TOKEN  update.Message.Chat.Id text

    let match_entity (e: Json.Update.Entity) =
        let sender =  update.Message.From.FirstName
        let value = update.Message.Text.Substring(e.Offset, e.Length)
        match e.Type with
        |"mention" ->  reply ("you've mentioned "+ value)
        |"bot_command" -> 
            let command = value
            match command with
             |"/hi"|"/hello" -> reply ("Nice to meet you, " + sender)
             |_ -> reply (sprintf "I don't know %s command, %s" command sender)
        |"hashtag" -> reply ("you've used hashtag " + value)
        |_ -> ignore()
    
    Array.iter (fun e -> match_entity e) update.Message.Entities

  let handleUpdates (updates: Json.Update.Result[]) = 
     Array.iter (fun res -> handle res) updates

  let rec mainLoop (offset: int) =
    let (update, nextOffset) = Telegram.getUpdates TOKEN offset

    sleep 100 

    match update with
    | Some upd -> Array.iter (fun res -> handle res) upd.Result
    | None -> ignore()
     
    mainLoop nextOffset

  let anotherMain = 
    Telegram.UpdateReceived.Add(handleUpdates)
    Async.Start (Telegram.getUpdatesBackground TOKEN 0)

  [<EntryPoint>]
  let main args =
  //  mainLoop 0
    anotherMain
    Console.WriteLine "finished"
    Console.ReadLine()
    0