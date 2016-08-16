namespace Shkoda.Telegram.Bot

module Main =
  open System

  [<EntryPoint>]
  let main args = 
    Telegram.UpdateSubscribers.Add(CommandHandler.handleUpdates)
    Async.Start (Telegram.getUpdatesBackground())

//^_^

    while true do Console.ReadLine()
    0