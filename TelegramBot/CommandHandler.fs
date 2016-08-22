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
        |"/k" -> Telegram.showReplyMarkupKeyboard chatId ("i'm trying to show keyboard, " + firstname) ([|[|"alpha"; "beta"|];[|"omega"|]|])
        |"/h" -> Telegram.hideReplyMarkupKeyboard chatId ("i'm trying to hide keyboard, " + firstname)
        |"/creds" -> reply (TelegramMarkdown.credentialsAsString(UserConfigProvider.getUser(sender)))
        |"/setlogin" |"/setl" |"/setemail" -> reply (BitBucket.setLogin sender args)
        |"/setpass" |"/setpassword" |"/setp" -> reply (BitBucket.setPassword sender args)
        |"/jirauser" -> reply (Jira.userinfo (UserConfigProvider.getUser(sender).Jira))
        |"/issues" -> reply (TelegramMarkdown.issuesToString(Jira.getNotClosedIssuesCurrentSprint (UserConfigProvider.getUser(sender).Jira)))
        |"/ik" -> Telegram.showConfigurableInlineKeyboard chatId [| [|{text = "a"; callback="ca"} |]; [|{text = "b"; callback="cb"} |]|]
        | _ -> reply (sprintf "I don't know %s command, %s" command firstname)
    
    let handleInlineResponse (response: Json.Update.CallbackQuery) = 
        let randomString = fun () ->
            let r = System.Random()
            sprintf "%i"(r.Next())
        Telegram.editMessage response.Message.Chat.Id response.Message.MessageId (randomString())
        Telegram.overrideInlineKeyboard response.Message.Chat.Id response.Message.MessageId 
            [| [|{text = randomString(); callback="ca"} |]; [|{text = randomString(); callback="cb"} |]|] 

    let handle (update : Json.Update.Result) =
        let reply text = Telegram.sendMessage (Json.chatId update) text
        match update with
        | Json.UserMessage m -> let input = CommandParser.parse m
                                match input with 
                                | CommandParser.Invalid i -> reply (sprintf "Invalid input: %s" i)
                                | CommandParser.Text t -> reply (sprintf "Text received: %s" t)
                                | CommandParser.Command (c, args) -> handleCommand (c,args) update
        | Json.InlineResponce i ->  handleInlineResponse i//reply (sprintf "Inline responce %s" i.Data)
        | _ -> raise (System.ArgumentException("can't handle update"))

 
    let handleUpdates (updates: Json.Update.Result[]) = 
         Array.iter (fun res -> handle res) updates     