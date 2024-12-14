module Widgets

open System.Reactive.Subjects
open DEdge.Diffract
open Types
open Services

let createWidgetNode id widgetType initialProps initialChildren =
    {
        Id = id
        Type = widgetType
        Props = new BehaviorSubject<Map<string, obj>>(initialProps)
        Children = new BehaviorSubject<WidgetNode list>(initialChildren)
    }

let updateProps (widget: WidgetNode) key value =
    let newProps = widget.Props.Value.Add(key, value)
    widget.Props.OnNext(newProps)

let observeProps (widget: WidgetNode) =
    widget.Props.Subscribe(fun newProps ->
        printfn "Props updated: %A" newProps
    )

type AppState = {
    Text: string
}

let appState = new BehaviorSubject<AppState>({ Text = "Hello, world" })

let widgetNodeFactory (widgetType: string, props: Map<string, obj>, children: WidgetNode list) =
    {
        Id = WidgetRegistrationService.getNextWidgetId()
        Type = widgetType
        Props = new System.Reactive.Subjects.BehaviorSubject<Map<string, obj>>(props)
        Children = new System.Reactive.Subjects.BehaviorSubject<WidgetNode list>(children)
    }

let node (children: WidgetNode list) =
    let props = Map.ofList []
    widgetNodeFactory("node", props, children)

let unformattedText (text: string) =
    let props = Map.ofList [("text", box text)]
    widgetNodeFactory("unformatted-text", props, [])

let button (label: string) (onClick: (unit -> unit) option) =
    let props =
        Map.ofList [
            ("label", box label)
            match onClick with
            | Some handler -> ("onClick", box handler)
            | None -> ()
        ]
    widgetNodeFactory("di-button", props, [])

let render (state: AppState) =
    node [
        unformattedText state.Text
        button "Click me!" (Some(fun () -> appState.OnNext({ Text = "Button clicked!" })))
    ]

let app () =
    let onClick = Some(fun () ->
        appState.OnNext({ Text = "Button clicked!" })
    )

    node [
        unformattedText (appState.Value.Text)
        button "Click me!" onClick
    ]

let runApp () =
    //let state = new BehaviorSubject<Map<string, string>>(Map.ofList [("text", "Hello, world")])

    // Track the previous tree
    let mutable oldTree = app

    // Function to apply changes
    let applyChanges (changes: Diff option) =
        match changes with
        | Some (Value(x1, x2)) ->
            // Handle leaf value differences, e.g., text content
            printfn "Value difference detected: %A -> %A" x1 x2
            // Update the widget content here

        | Some (Nullness(x1, x2)) ->
            // Handle nullness differences (null vs. non-null)
            printfn "Nullness difference detected: %A -> %A" x1 x2
            // Handle the widget addition/removal here

        | Some (Record fields) ->
            // Handle record differences (widget fields differ)
            printfn "Record difference detected, fields: %A" fields
            // Update the widget fields accordingly

        | Some (UnionCase(caseName1, caseName2)) ->
            // Handle union case differences (different union cases)
            printfn "Union case difference detected: %s -> %s" caseName1 caseName2
            // Replace the widget or update accordingly

        | Some (UnionField(case, fields)) ->
            // Handle field differences within the same union case
            printfn "Union field difference detected for case %s, fields: %A" case fields
            // Update the widget fields here

        | Some (Collection(count1, count2, items)) ->
            // Handle collection differences (lengths or items differ)
            printfn "Collection difference detected: %d -> %d, items: %A" count1 count2 items
            // Update child widgets accordingly

        | Some (Dictionary(keysInX1, keysInX2, common)) ->
            // Handle dictionary differences (keys or values differ)
            printfn "Dictionary difference detected, keys in X1: %A, keys in X2: %A" keysInX1 keysInX2
            // Update the widget properties accordingly

        | Some (Custom customDiff) ->
            // Handle custom diffing logic
            printfn "Custom diff detected: %A" customDiff
            // Apply custom diff logic here

        | None ->
            // No changes detected
            printfn "No changes detected"

    // Subscribe to state changes
    appState.Subscribe(fun _ ->
        let newTree = app
        let changes = Differ.Diff(oldTree, newTree) // Calculate the diff
        applyChanges changes // Apply the changes to your UI
        oldTree <- newTree // Update the old tree
    ) |> ignore

    // Initial render
    oldTree