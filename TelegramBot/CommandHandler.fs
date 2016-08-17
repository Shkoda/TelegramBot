namespace Shkoda.Telegram.Bot
module CommandHandler = 
    open System

    let handleNoArgCommand command (update : Json.Update.Result) = 
        let reply text = Telegram.sendMessage update.Message.Chat.Id text
        let firstName = update.Message.From.FirstName
        let sender =  update.Message.From.Username

        match command with
        |"/hi"|"/hello" -> reply ("Nice to meet you, " + firstName)
        |"/git" -> reply (BitBucket.getCommitStatistics (sender))
        |"/k" -> Telegram.keyboard update.Message.Chat.Id ("i'm trying to show keyboard, " + firstName)
        |"/creds" -> reply (TelegramMarkdown.userAsString(UserConfigProvider.getUser(sender)))
        |_ -> reply (sprintf "I don't know %s command, %s" command firstName)

    let handleArgCommand (command, args:string[]) (update : Json.Update.Result) = 
        let reply text = Telegram.sendMessage update.Message.Chat.Id text
        let firstName = update.Message.From.FirstName
        let sender =  update.Message.From.Username
        match command with
        |"/setlogin" -> reply (sprintf "Login updated. Current credentials:\n\n%s" (TelegramMarkdown.userAsString(UserConfigProvider.saveEmail sender args.[0])))
        |"/setpass" -> reply (sprintf "Login updated. Current credentials:\n\n%s" (TelegramMarkdown.userAsString(UserConfigProvider.savePassword sender args.[0])))

        | _ -> reply (sprintf "I don't know %s command, %s" command firstName)

    let handle (update : Json.Update.Result) =
        let reply text = Telegram.sendMessage update.Message.Chat.Id text
        let input = CommandParser.parse update
        match input with 
        | CommandParser.Invalid i -> reply (sprintf "Invalid input: %s" i)
        | CommandParser.Text t -> reply (sprintf "Text received: %s" t)
        | CommandParser.NoArgCommand c -> handleNoArgCommand c update
        | CommandParser.ArgCommand (c, args) -> handleArgCommand (c,args) update
 
    let handleUpdates (updates: Json.Update.Result[]) = 
         Array.iter (fun res -> handle res) updates     

 