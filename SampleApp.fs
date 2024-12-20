module SampleApp

open System.Reactive.Subjects
open System.Reactive.Linq
open System
open Widgets
open Types

let sampleAppState = new BehaviorSubject<AppState>({ Text = "Hello, world"; Count = 1 })

type App() as this =
    inherit BaseComponent<Map<string, obj>>()

    let onClick() =
        printfn "Clicked! %A" sampleAppState.Value
        sampleAppState.OnNext({ Text = "Hello, world"; Count = sampleAppState.Value.Count + 1 })
        printfn "After clicked! %A %A" sampleAppState.Value this.Props.Value

    override this.Init() =
        printfn "Init()"

        this.sub <- Some(sampleAppState.Subscribe(fun latestAppState -> 
            printfn "combine!"

            this.Props.OnNext(
                Map.ofList [
                    "text", latestAppState.Text :> obj
                    "count", latestAppState.Count :> obj
                ]
            )
        ))

    override this.Destroy() =
        match this.sub with
        | Some(subscription) -> subscription.Dispose()
        | None -> ()


    override this.Render() =
        printfn "Rendering SampleApp"

        let textNodes = 
            [ for _ in 1 .. sampleAppState.Value.Count -> unformattedText sampleAppState.Value.Text ]

        let children = 
            [ button("Add text", Some onClick) ] @ textNodes

        WidgetNode (
            makeRootNode children
        )

    member val sub: IDisposable option = None with get, set
