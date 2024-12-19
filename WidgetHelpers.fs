module WidgetHelpers

open System.Reactive.Subjects
open Types
open Services

let createWidgetNode(widgetType: string, initialProps: Map<string, obj>, initialChildren: WidgetNode list) : WidgetNode =
    { 
        Type = widgetType; 
        Props = new BehaviorSubject<Map<string, obj>>(initialProps); 
        Children = new BehaviorSubject<WidgetNode list>(initialChildren) 
    }

let createRawWidgetNode (widgetType: string, initialProps: Map<string, obj>, initialChildren: RawWidgetNode list) : RawWidgetNode =
    { 
        Type = widgetType; 
        Props = initialProps; 
        Children = initialChildren 
    }

let rec createRawWidgetNodeWithIdFromRawWidgetNodeWithoutId (rawWidgetNode: RawWidgetNode) : RawWidgetNodeWithId =
    { 
        Id = WidgetRegistrationService.getNextWidgetId()
        Type = rawWidgetNode.Type; 
        Props = rawWidgetNode.Props; 
        Children = rawWidgetNode.Children |> List.map createRawWidgetNodeWithIdFromRawWidgetNodeWithoutId
    }

let createRawWidgetNodeWithId (id: int, widgetType: string, initialProps: Map<string, obj>, initialChildren: RawWidgetNode list) : RawWidgetNodeWithId =
    { 
        Id = id
        Type = widgetType 
        Props = initialProps
        Children = initialChildren |> List.map createRawWidgetNodeWithIdFromRawWidgetNodeWithoutId
    }

// todo: rename!
let createRawChildlessWidgetNodeWithId (id: int, widgetType: string, initialProps: Map<string, obj>) : RawChildlessWidgetNodeWithId =
    { 
        Id = id
        Type = widgetType 
        Props = initialProps
    }

let createWidgetNodeFromRawWidgetNode (rawWidgetNode: RawWidgetNode) =
    createWidgetNode(rawWidgetNode.Type, rawWidgetNode.Props, [])

let updateProps (widget: WidgetNode) key value =
    let newProps = widget.Props.Value.Add(key, value)
    widget.Props.OnNext(newProps)

let observeProps (widget: WidgetNode) =
    widget.Props.Subscribe(fun newProps ->
        printfn "Props updated: %A" newProps
    )


let widgetNodeFactory (widgetType: string, props: Map<string, obj>, children: WidgetNode list) =
    {
        Type = widgetType
        Props = new System.Reactive.Subjects.BehaviorSubject<Map<string, obj>>(props)
        Children = new System.Reactive.Subjects.BehaviorSubject<WidgetNode list>(children)
    }