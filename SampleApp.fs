module SampleApp

open System.Reactive.Subjects
open Widgets
open Types

let appState = new BehaviorSubject<AppState>({ Text = "Hello, world"; Count = 1 })

type App() =
    inherit BaseComponent<Map<string, obj>>()

    let onClick () =
        appState.OnNext({ Text = "Hello, world"; Count = appState.Value.Count + 1 })

    interface IComponent<Map<string, obj>> with
        member this.Render() =
            let textNodes = 
                [ for _ in 1 .. appState.Value.Count -> unformattedText appState.Value.Text ]

            let children = 
                [ button("Add text", Some onClick) ] @ textNodes

            WidgetNode (
                makeRootNode children
            )