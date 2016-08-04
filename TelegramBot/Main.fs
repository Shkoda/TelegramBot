namespace Shkoda.Telegram.Bot

module Main =
  open FSharp.Data
  open System

  let TOKEN  = "231953668:AAHUQ8HEQr8Scnl_ViDZ6dWtH9JXDeMy5hw"
  let PREFIX = "/"
  let (>>=) = Telegram.(>>=)
  let sleep (x: int) = System.Threading.Thread.Sleep x

  let handleJson (message : JsonParser.Update.Result) =

    let reply text = Telegram.sendMessage TOKEN  message.Message.Chat.Id text

    let match_entity (e: JsonParser.Update.Entity) =
        let value = message.Message.Text.Substring(e.Offset, e.Length)
        match e.Type with
        |"mention" ->  reply ("you've mentioned "+ value)
        |"bot_command" -> 
            let command = value
            match command with
             |"/hi"|"/hello" -> reply ("Nice to meet you, " + message.Message.From.FirstName)
             |_ -> reply (sprintf "I don't know %s command, %s" command message.Message.From.FirstName)
        |"hashtag" -> reply ("you've used hashtag "+value)
        |_ -> ignore()

    let rec match_entities (entities: JsonParser.Update.Entity[]) = 
        match entities with
        |[||] -> ()
        |_ as arr -> match_entity(arr.[0]); match_entities(Array.sub arr 1 (arr.Length-1))

    match_entities message.Message.Entities

  let rec mainLoop (offset: int) =
    let newUpdate = Telegram.getUpdatesJson0 TOKEN offset
    let newOffset = Telegram.getNewId  newUpdate

    sleep 100 

    match newUpdate with
    | Some upd-> Seq.iter (fun res -> handleJson res) upd.Result
    | None -> ignore()
     

    match newOffset with
      | Some nO -> mainLoop nO
      | None    -> mainLoop 0 

  [<EntryPoint>]
  let main args =

    let initial = Telegram.getUpdatesJson TOKEN 
    let nextId = Telegram.getNewId initial
    match initial with
    |Some updates ->       
        match nextId with
        |Some i -> mainLoop i
        |None -> mainLoop 0
    |None -> mainLoop 0

    0