namespace Shkoda.Telegram.Bot

module Main =
  open FSharp.Data
  open System

  let TOKEN  = "231953668:AAHUQ8HEQr8Scnl_ViDZ6dWtH9JXDeMy5hw"
  let PREFIX = "/"

  let (<@>) = Telegram.(<@>)
  let (>>=) = Telegram.(>>=)
  let sleep (x: int) = System.Threading.Thread.Sleep x

  let rand () = 
    let rng = new System.Random ()
    rng.Next ()

  //  handler :: JsonValue -> unit
  let handler (message: JsonValue) =
    let cid = Telegram.chat_id message
    let body = Telegram.message_text message
    Console.WriteLine( message.ToString())

    let parsed = JsonParser.Message.Parse(message.ToString())
    Console.WriteLine("from "+parsed.Message.From.Username)


    let match_entity (e: JsonParser.Message.Entity) (raw_body : string)=
        let value = raw_body.Substring(e.Offset, e.Length)
        match e.Type with
        |"mention" ->  Telegram.sendMessage TOKEN cid ("mention "+value)
        |"bot_command" -> Telegram.sendMessage TOKEN cid ("bot_command "+value)
        |"hashtag" -> Telegram.sendMessage TOKEN cid ("hashtag "+value)
        |_ -> ignore()

    let rec match_entities (entities: JsonParser.Message.Entity[]) (raw_body : string)= 
        match entities with
        |[||] -> ()
        |_ as arr -> match_entity(arr.[0])( raw_body); match_entities(Array.sub arr 1 (arr.Length-1))(raw_body)

    match_entities  parsed.Message.Entities body

    Console.WriteLine ("done with match 1. body = "+body)

    match body with
      | msg when msg = PREFIX + "hi" -> 
          Telegram.sendMessage TOKEN cid "hi"
      | msg when msg = PREFIX + "me" ->
          Telegram.sendMessage TOKEN cid (sprintf "hi %d" <| Telegram.from_id message)
      | msg when msg = PREFIX + "rand" ->
          Telegram.sendMessage TOKEN cid (sprintf "%O" <| rand ())
      | _ -> ignore ()

  // --

  let rec mainLoop (offset: int) =
    let newUpdate = Telegram.getUpdatesO TOKEN offset 
    let newOffset = (newUpdate >>= Telegram.result >>= Telegram.getNewId)

    sleep 100 

    match (newUpdate >>= Telegram.result) with
      | Some results -> Seq.iter (fun res -> handler res) results
      | None         -> ignore ()

    match newOffset with
      | Some nO -> mainLoop nO
      | None    -> mainLoop 0 

  [<EntryPoint>]
  let main args =
    let initial = Telegram.getUpdates TOKEN 

    match (initial >>= Telegram.result >>= Telegram.getNewId) with
      | Some x -> mainLoop x 
      | None   -> mainLoop 0 
    0