namespace Shkoda.Telegram.Bot
module Json =
    open FSharp.Data
 
    type Update = JsonProvider<"../data/update.json">
    type CommitsResponse = JsonProvider<"../data/commits_sample.json">
    type UserConfig = JsonProvider<"../data/user_config_sample.json">
    type InlineKeybardResponse = JsonProvider<"../data/inline_keyboard_response.json">

    let (|Message|InlineResponce|UnknownFormat|) (jsonString: string) = 
        try
            let update = Update.Parse jsonString 
            Message (update)        
        with
        |_ -> try 
                let inlineResponse = InlineKeybardResponse.Parse jsonString
                InlineResponce (inlineResponse)
                with 
                | _ -> UnknownFormat (jsonString)