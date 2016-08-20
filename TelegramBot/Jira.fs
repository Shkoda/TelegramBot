namespace Shkoda.Telegram.Bot
module Jira = 
    open Option

    let userinfo (config:Json.UserConfig.Jira) = 
        let searchUserEndpoint = sprintf "/rest/api/2/user/search?username=%s" config.Email
        let url = config.Url + searchUserEndpoint
        let getUser (data:Json.JiraSearchUser.Root[]) = 
            match data.Length with
            |0 -> None
            |_ -> Some (data.[0])
        let toString (user: Json.JiraSearchUser.Root option) = 
            match user with
            |Some data -> TelegramMarkdown.jiraUserAsString data
            |None -> sprintf "jira user %s not found" config.Email
        Http.authorizedRequest url config.Email config.Password 
        |> bind (Json.JiraSearchUser.Parse >> getUser) 
        |> toString
