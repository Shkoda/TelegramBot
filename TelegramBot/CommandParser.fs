namespace Shkoda.Telegram.Bot

module CommandParser = 
    open System

    type UserInput = 
        | Command of string * string[]
        | Text of string
        | Invalid of string

    let parse (message: Json.Update.Message) = 
        try
        let commandEntities = message.Entities |> Array.where (fun e -> e.Type = "bot_command")
     
        let parseCommand (message: Json.Update.Message) (entity:Json.Update.Entity) = 
            let command = message.Text.Substring(entity.Offset, entity.Length) 
            let messageAfterCommand = (message.Text.Substring (entity.Offset + entity.Length)).Trim()

            Command (command,  messageAfterCommand.Split([|' '|], StringSplitOptions.RemoveEmptyEntries))

        match commandEntities.Length with
        | 0 -> Text (message.Text)
        | 1 -> parseCommand message commandEntities.[0]
        | _ -> Invalid ("Multiple commands in one message are not allowed")
        
        with 
        |ex->printf "EXCEPTION: %s\n" ex.Message; Invalid("error occured")
        
         

