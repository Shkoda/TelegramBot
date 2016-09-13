namespace Shkoda.Telegram.Bot
module Jira = 
    open System
    open Option
    open Operators
    open FSharp.Data.HttpRequestHeaders
    open FSharp.Data

            //active sprint https://reddotsquare.atlassian.net/rest/agile/1.0/board/28/sprint?state=active
        // https://reddotsquare.atlassian.net/rest/agile/1.0/board/28/sprint/274/issue?jql=(assignee=currentuser() OR assignee=null) AND status not in ("To review", "QA Ready", "Done") 

    let userinfo (config:Json.UserConfig.Jira) = 
        let searchUserEndpoint = sprintf "/rest/api/2/myself" 
        let url = config.Url + searchUserEndpoint

        Http.authorizedRequest url config.Email config.Password 
        |> bind (Json.JiraSearchUser.Parse >> Some) 
        |> TelegramMarkdown.jiraUserAsString

    let getActiveSprintId (config:Json.UserConfig.Jira) =      
        let url = sprintf "https://reddotsquare.atlassian.net/rest/agile/1.0/board/%i/sprint?state=active" config.BoardId
        Http.authorizedRequest url config.Email config.Password 
        |> bind (Json.ActiveSprintResponse.Parse >> Json.activeSprintId)

    let getNotClosedIssuesFromSprint (config:Json.UserConfig.Jira) (sprintId)= 
        let url = sprintf "https://reddotsquare.atlassian.net/rest/agile/1.0/board/%i/sprint/%i/issue?jql=(assignee=currentuser() OR assignee=null) AND status not in (\"To review\", \"QA Ready\", \"Done\") " config.BoardId sprintId
        Http.authorizedRequest url config.Email config.Password 
        |> bind (Json.ActiveIssuesResponse.Parse >> Some) 

    let getNotClosedIssuesCurrentSprint (config:Json.UserConfig.Jira) = 
     match (getActiveSprintId (config)) with
     | Some id -> getNotClosedIssuesFromSprint config id
     | None -> None

    let logWork (config:Json.UserConfig.Jira) (task:string) (hours:int) = 
        let workLogEntry startTime hours comment = 
            sprintf "{\"timeSpent\":\"%ih\",\"started\":\"%s\",\"comment\":\"%s\"}" hours startTime comment
        
        let workStartTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzzz")
        //"started": "2013-09-01T10:30:18.932+0530", works
      //  let workStartTime = DateTime.Now.ToString("r")
        Console.WriteLine workStartTime

        let requestBody = workLogEntry workStartTime hours "Logged by Celesta"          
        let url = sprintf "https://reddotsquare.atlassian.net/rest/api/2/issue/%s/worklog" task
        let response = Http.authorizedPostRequest url requestBody config.Email config.Password

        match response with
        | Some r -> r
        | None -> "sorry=("

   //     Http.RequestString(url, httpMethod = "POST", headers = [ContentType HttpContentTypes.Json], body = TextRequest requestBody)
        

//https://reddotsquare.atlassian.net/rest/api/2/issue/NG-13721/worklog
//{
//"timeSpent": "1h 30m",
//"started": "2013-09-01T10:30:18.932+0530",
//"comment": "logging via powershell"
//}

