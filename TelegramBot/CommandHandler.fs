﻿namespace Shkoda.Telegram.Bot
module CommandHandler = 
    open System

    let TOKEN  = "231953668:AAHUQ8HEQr8Scnl_ViDZ6dWtH9JXDeMy5hw"

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
                 |"/git" -> reply (BitBucket.getCommitStatistics (update.Message.From.Username))
                 |_ -> reply (sprintf "I don't know %s command, %s" command sender)
            |"hashtag" -> reply ("you've used hashtag " + value)
            |_ -> ignore()

        Console.WriteLine (sprintf "from %s: %s" update.Message.Chat.Username update.Message.Text)
        Array.iter (fun e -> match_entity e) update.Message.Entities

    let handleUpdates (updates: Json.Update.Result[]) = 
         Array.iter (fun res -> handle res) updates

