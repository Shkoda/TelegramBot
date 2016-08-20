namespace Shkoda.Telegram.Bot
module CommandHandler = 
  
    let handleCommand (command, args:string[]) (update : Json.Update.Result) = 
        let chatId = Json.chatId update
        let reply text =  Telegram.sendMessage chatId text
        let sender =  Json.username update
        let firstname = Json.firstname update
        match command with
        |"/hi"|"/hello" -> reply ("Nice to meet you, " + firstname)
        |"/git" -> reply (BitBucket.getCommitStatistics (sender))
        |"/k" -> Telegram.showKeyboard chatId ("i'm trying to show keyboard, " + firstname)
        |"/h" -> Telegram.hideKeyboard chatId ("i'm trying to hide keyboard, " + firstname)
        |"/ik" ->  Telegram.showInlineKeyboard chatId ("i'm trying to show inline keyboard, " + firstname)
        |"/creds" -> reply (TelegramMarkdown.userAsString(UserConfigProvider.getUser(sender)))
        |"/setlogin" |"/setl" |"/setemail" -> reply (BitBucket.setLogin sender args)
        |"/setpass" |"/setpassword" |"/setp" -> reply (BitBucket.setPassword sender args)

        | _ -> reply (sprintf "I don't know %s command, %s" command firstname)

    let handle (update : Json.Update.Result) =
        let reply text = Telegram.sendMessage (Json.chatId update) text
        match update with
        | Json.UserMessage m -> let input = CommandParser.parse m
                                match input with 
                                | CommandParser.Invalid i -> reply (sprintf "Invalid input: %s" i)
                                | CommandParser.Text t -> reply (sprintf "Text received: %s" t)
                                | CommandParser.Command (c, args) -> handleCommand (c,args) update
        | Json.InlineResponce i ->  reply (sprintf "Inline responce %s" i.Data)
        | _ -> raise (System.ArgumentException("can't handle update"))

 
    let handleUpdates (updates: Json.Update.Result[]) = 
         Array.iter (fun res -> handle res) updates     