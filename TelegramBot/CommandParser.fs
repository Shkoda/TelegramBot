namespace Shkoda.Telegram.Bot

module CommandParser = 
    open System
    open FSharp.Data

    type UserInput = 
        | NoArgCommand of string
        | ArgCommand of string * string[]
        | Text of string
        | Invalid of string

    let parse (update: Json.Update.Result) = 
        let commandEntities = update.Message.Entities |> Array.where (fun e -> e.Type = "bot_command")
        
        let parseCommand (update: Json.Update.Result) (entity:Json.Update.Entity) = 
            let command = update.Message.Text.Substring(entity.Offset, entity.Length) 
            let messageAfterCommand = (update.Message.Text.Substring (entity.Offset + entity.Length)).Trim()
 
            match messageAfterCommand.Length with
            | 0 -> NoArgCommand (command)
            | _ -> ArgCommand (command,  messageAfterCommand.Split([|' '|]))

        match commandEntities.Length with
        | 0 -> Text (update.Message.Text)
        | 1 -> parseCommand update commandEntities.[0]
        | _ -> Invalid ("Multiple commands in one message are not allowed")
        
        
         

