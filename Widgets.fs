module Widgets

open System.Reactive.Subjects
open Types

//let createWidgetNode id nodeType =
//    {
//        Id = id
//        Type = nodeType
//        Props = new BehaviorSubject<Map<string, obj>>(Map.empty)
//        Children = []
//    }

let createWidgetNode id widgetType initialProps children =
    {
        Id = id
        Type = widgetType
        Props = new BehaviorSubject<Map<string, obj>>(initialProps)
        Children = children
    }

let updateProps (widget: WidgetNode) key value =
    let newProps = widget.Props.Value.Add(key, value)
    widget.Props.OnNext(newProps)

let observeProps (widget: WidgetNode) =
    widget.Props.Subscribe(fun newProps ->
        printfn "Props updated: %A" newProps
    )

