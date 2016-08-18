namespace Shkoda.Telegram.Bot
module CommandHandler = 

    let setLogin (sender:string)(args:string[]) = 
        match args.Length with
        | 0 -> "/setlogin should have one argument"
        | _ -> sprintf "Login was updated. Current credentials:\n\n%s" (TelegramMarkdown.userAsString(UserConfigProvider.saveEmail sender args.[0]))

    let setPassword (sender:string)(args:string[]) = 
        match args.Length with
        | 0 -> "/setpass should have one argument"
        | _ -> sprintf "Password was updated. Current credentials:\n\n%s" (TelegramMarkdown.userAsString(UserConfigProvider.savePassword sender args.[0]))

    let handleCommand (command, args:string[]) (update : Json.Update.Result) = 
        let reply text = Telegram.sendMessage update.Message.Chat.Id text
        let firstName = update.Message.From.FirstName
        let sender =  update.Message.From.Username
        match command with
        |"/hi"|"/hello" -> reply ("Nice to meet you, " + firstName)
        |"/git" -> reply (BitBucket.getCommitStatistics (sender))
        |"/k" -> Telegram.showKeyboard update.Message.Chat.Id ("i'm trying to show keyboard, " + firstName)
        |"/h" -> Telegram.hideKeyboard update.Message.Chat.Id ("i'm trying to hide keyboard, " + firstName)
        |"/ik" ->  Telegram.showInlineKeyboard update.Message.Chat.Id ("i'm trying to show inline keyboard, " + firstName)
        |"/creds" -> reply (TelegramMarkdown.userAsString(UserConfigProvider.getUser(sender)))
        |"/setlogin" |"/setl" |"/setemail" -> reply (setLogin sender args)
        |"/setpass" |"/setpassword" |"/setp" -> reply (setPassword sender args)

        | _ -> reply (sprintf "I don't know %s command, %s" command firstName)

    let handle (update : Json.Update.Result) =
        let reply text = Telegram.sendMessage update.Message.Chat.Id text
        let input = CommandParser.parse update
        match input with 
        | CommandParser.Invalid i -> reply (sprintf "Invalid input: %s" i)
        | CommandParser.Text t -> reply (sprintf "Text received: %s" t)
        | CommandParser.Command (c, args) -> handleCommand (c,args) update
 
    let handleUpdates (updates: Json.Update.Result[]) = 
         Array.iter (fun res -> handle res) updates     

 