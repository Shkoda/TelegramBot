namespace Shkoda.Telegram.Bot

module Main =
  open FSharp.Data
  open System

  let TOKEN  = "231953668:AAHUQ8HEQr8Scnl_ViDZ6dWtH9JXDeMy5hw"
  let PREFIX = "/"

  let sleep (x: int) = System.Threading.Thread.Sleep x

  let handle (update : JsonParser.Update.Result) =
    let reply text = Telegram.sendMessage TOKEN  update.Message.Chat.Id text

    let match_entity (e: JsonParser.Update.Entity) =
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

    for e in update.Message.Entities do match_entity(e)

  let rec mainLoop (offset: int) =
    let update = Telegram.getUpdates TOKEN offset
    let newOffset = Telegram.getNewId  update

    sleep 100 

    match update with
    | Some upd-> Seq.iter (fun res -> handle res) upd.Result
    | None -> ignore()
     
    mainLoop newOffset

  [<EntryPoint>]
  let main args =
    mainLoop 0
    0