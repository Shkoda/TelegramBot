namespace Shkoda.Telegram.Bot
module JsonParser=
    open FSharp.Data

    type Message = JsonProvider<"../data/message.json">
    let test = Message.Parse("\"{\r\n  \"update_id\": 736904239,\r\n  \"message\": {\r\n    \"message_id\": 50,\r\n    \"from\": {\r\n      \"id\": 168659798,\r\n      \"first_name\": \"Shkoda\",\r\n      \"last_name\": \"Aleceya\",\r\n      \"username\": \"freilin\"\r\n    },\r\n    \"chat\": {\r\n      \"id\": 168659798,\r\n      \"first_name\": \"Shkoda\",\r\n      \"last_name\": \"Aleceya\",\r\n      \"username\": \"freilin\",\r\n      \"type\": \"private\"\r\n    },\r\n    \"date\": 1470259539,\r\n    \"text\": \"das\"\r\n  }\r\n}\"")

    let n = test.Message.Chat.FirstName