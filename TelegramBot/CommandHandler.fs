namespace Shkoda.Telegram.Bot
module CommandHandler = 
    open System

    let handle (update : Json.Update.Result) =
        let reply text = Telegram.sendMessage update.Message.Chat.Id text
  
        let match_entity (e: Json.Update.Entity) =
            let sender =  update.Message.From.Username
            let firstName = update.Message.From.FirstName
            let value = update.Message.Text.Substring(e.Offset, e.Length)
            match e.Type with
            |"mention" ->  reply ("you've mentioned "+ value)
            |"bot_command" -> 
                let command = value
                match command with
                 |"/hi"|"/hello" -> reply ("Nice to meet you, " + firstName)
                 |"/git" -> reply (BitBucket.getCommitStatistics (sender))
                 |"/k" -> Telegram.keyboard update.Message.Chat.Id ("i'm trying to show keyboard, " + firstName)
                 |"/creds" -> reply (TelegramMarkdown.userAsString(UserConfigProvider.getUser(sender)))
                 |_ -> reply (sprintf "I don't know %s command, %s" command firstName)
            |"hashtag" -> reply ("you've used hashtag " + value)
            |_ -> ignore()

        Console.WriteLine (sprintf "from %s: %s" update.Message.Chat.Username update.Message.Text)
        Array.iter (fun e -> match_entity e) update.Message.Entities

    let handleUpdates (updates: Json.Update.Result[]) = 
         Array.iter (fun res -> handle res) updates

