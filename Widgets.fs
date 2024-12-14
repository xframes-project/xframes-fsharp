module Widgets

open System.Reactive.Subjects
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

