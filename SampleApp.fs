module SampleApp

open System.Reactive.Subjects
open System.Reactive.Linq
open System
open Widgets
open Types

let sampleAppState = new BehaviorSubject<AppState>({ Text = "Hello, world"; Count = 1 })



type App() =
    inherit BaseComponent<Map<string, obj>>()

    let onClick () =
        printfn "Clicked!"
        sampleAppState.OnNext({ Text = "Hello, world"; Count = sampleAppState.Value.Count + 1 })
        printfn "After clicked!"

    member val sub: IDisposable option = None with get, set

    interface IComponent<Map<string, obj>> with
        member this.Init() =
            printfn "Init()"

            this.sub <- Some(sampleAppState.Subscribe(fun data -> printfn "%A" data))

            //let combinedPropsObservable =
            //    Observable.CombineLatest(
            //        sampleAppState,
            //        this.Props,
            //        (fun appStateValue localPropsValue ->
            //            printfn "combine!"
            //            // Combine the two into a single Map<string, obj>
            //            let appProps =
            //                Map.ofList [
            //                    "text", appStateValue.Text :> obj
            //                    "count", appStateValue.Count :> obj
            //                ]
            //            // Combine appProps with localProps (localProps will override appState where keys are the same)
            //            Map.fold (fun acc key value -> Map.add key value acc) appProps localPropsValue
            //        )
            //    )

            //this.sub <- Some(combinedPropsObservable.Subscribe(fun props -> 
            //    printfn "outside"
            //    // Only update this.Props when the combined state changes
            //    if this.Props.Value <> props then
            //        printfn "inside"
            //        this.Props.OnNext(props)
            //))

        member this.Destroy() =
            match this.sub with
            | Some(subscription) -> subscription.Dispose()
            | None -> ()


        member this.Render() =
            printfn "Rendering SampleApp"

            let textNodes = 
                [ for _ in 1 .. sampleAppState.Value.Count -> unformattedText sampleAppState.Value.Text ]

            let children = 
                [ button("Add text", Some onClick) ] @ textNodes

            WidgetNode (
                makeRootNode children
            )