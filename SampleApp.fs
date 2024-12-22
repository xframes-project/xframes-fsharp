module SampleApp

open System.Reactive.Subjects
open System.Reactive.Linq
open System
open Widgets
open Types

type AppState = {
    Text: string
    Count: int
}

let sampleAppState = new BehaviorSubject<AppState>({ Text = "Hello..."; Count = 1 })

type App() =
    inherit BaseComponent()

    let onClick() =
        sampleAppState.OnNext({ Text = "Hello, world!"; Count = sampleAppState.Value.Count + 1 })

    override this.Init() =

        this.sub <- Some(sampleAppState.Subscribe(fun latestAppState -> 
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
        let textNodes = 
            [ for _ in 1 .. sampleAppState.Value.Count -> 
                Renderable.WidgetNode(unformattedText sampleAppState.Value.Text)
            ]

        let children = 
            [ Renderable.WidgetNode(button("Add text", Some onClick)) ] @ textNodes

        WidgetNode (
            node children
        )

    member val sub: IDisposable option = None with get, set

type Root() =
    inherit BaseComponent()

    override this.Init() =
        ignore()

    override this.Destroy() =
        ignore()

    override this.Render() =
        let ret: BaseComponent = App()

        WidgetNode (
            makeRootNode [(BaseComponent ret)]
        )

    member val sub: IDisposable option = None with get, set
