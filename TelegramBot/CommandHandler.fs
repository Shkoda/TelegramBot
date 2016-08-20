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

    let getChatId (update : Json.Update.Result) = 
        match update with
        | Telegram.Message m -> m.Chat.Id
        | Telegram.InlineResponce i -> i.Message.Chat.Id
        | _ -> raise (System.ArgumentException("can't get chat id"))
    let getUsername (update : Json.Update.Result) = 
        match update with
        | Telegram.Message m -> m.From.Username
        | Telegram.InlineResponce i -> i.From.Username
        | _ -> raise (System.ArgumentException("can't get username"))

    let handleCommand (command, args:string[]) (update : Json.Update.Result) = 
        let chatId = getChatId update
        let reply text =  Telegram.sendMessage chatId text
        let sender =  getUsername update
        match command with
        |"/hi"|"/hello" -> reply ("Nice to meet you, " + sender)
        |"/git" -> reply (BitBucket.getCommitStatistics (sender))
        |"/k" -> Telegram.showKeyboard chatId ("i'm trying to show keyboard, " + sender)
        |"/h" -> Telegram.hideKeyboard chatId ("i'm trying to hide keyboard, " + sender)
        |"/ik" ->  Telegram.showInlineKeyboard chatId ("i'm trying to show inline keyboard, " + sender)
        |"/creds" -> reply (TelegramMarkdown.userAsString(UserConfigProvider.getUser(sender)))
        |"/setlogin" |"/setl" |"/setemail" -> reply (setLogin sender args)
        |"/setpass" |"/setpassword" |"/setp" -> reply (setPassword sender args)

        | _ -> reply (sprintf "I don't know %s command, %s" command sender)

    let handle (update : Json.Update.Result) =
        let reply text = Telegram.sendMessage (getChatId update) text
        match update with
        | Telegram.Message m -> let input = CommandParser.parse m
                                match input with 
                                | CommandParser.Invalid i -> reply (sprintf "Invalid input: %s" i)
                                | CommandParser.Text t -> reply (sprintf "Text received: %s" t)
                                | CommandParser.Command (c, args) -> handleCommand (c,args) update
        | Telegram.InlineResponce i ->  reply (sprintf "Inline responce %s" i.Data)
        | _ -> raise (System.ArgumentException("can't handle update"))

 
    let handleUpdates (updates: Json.Update.Result[]) = 
         Array.iter (fun res -> handle res) updates     