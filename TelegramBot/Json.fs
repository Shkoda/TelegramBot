namespace Shkoda.Telegram.Bot
module Json =
    open FSharp.Data
    open System

    type Update = JsonProvider<"../data/update.json">
    type CommitsResponse = JsonProvider<"../data/commits_sample.json">
    type UserConfig = JsonProvider<"../data/user_config_sample.json">

    let (|UserMessage|InlineResponce|UnknownFormat|) (update:Update.Result) = 
        match update with
        |u when u.Message.IsSome -> UserMessage (u.Message.Value)
        |u when u.CallbackQuery.IsSome -> InlineResponce (u.CallbackQuery.Value)
        |_ -> UnknownFormat (update)

    let chatId (update : Update.Result) = 
        match update with
        | UserMessage m -> m.Chat.Id
        | InlineResponce i -> i.Message.Chat.Id
        | _ -> raise (System.ArgumentException("can't get chat id"))
    let username (update : Update.Result) = 
        match update with
        | UserMessage m -> m.From.Username
        | InlineResponce i -> i.From.Username
        | _ -> raise (System.ArgumentException("can't get username"))

    let firstname (update : Update.Result) = 
        match update with
        | UserMessage m -> m.From.FirstName
        | InlineResponce i -> i.From.FirstName
        | _ -> raise (System.ArgumentException("can't get firstname"))
    