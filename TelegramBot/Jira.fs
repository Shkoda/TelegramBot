namespace Shkoda.Telegram.Bot
module Jira = 
    open Option

    let userinfo (config:Json.UserConfig.Jira) = 
        let searchUserEndpoint = sprintf "/rest/api/2/myself" 
        let url = config.Url + searchUserEndpoint
        let toString (user: Json.JiraSearchUser.Root option) = 
            match user with
            |Some data -> TelegramMarkdown.jiraUserAsString data
            |None -> sprintf "jira user %s not found for %s" config.Email config.Url

        Http.authorizedRequest url config.Email config.Password 
        |> bind (Json.JiraSearchUser.Parse >> Some) 
        |> toString

        //active sprint https://reddotsquare.atlassian.net/rest/agile/1.0/board/28/sprint?state=active
        // https://reddotsquare.atlassian.net/rest/agile/1.0/board/28/sprint/274/issue?jql=(assignee=currentuser() OR assignee=null) AND status not in ("To review", "QA Ready", "Done") 