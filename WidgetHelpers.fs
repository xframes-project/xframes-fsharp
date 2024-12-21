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

let createRawWidgetNode(widgetType: string, initialProps: 'T, initialChildren: RawWidgetNode list) : RawWidgetNode =
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

let createRawWidgetNodeWithId(id: int, widgetType: string, initialProps: Map<string, obj>, initialChildren: RawWidgetNode list) : RawWidgetNodeWithId =
    { 
        Id = id
        Type = widgetType 
        Props = initialProps
        Children = initialChildren |> List.map createRawWidgetNodeWithIdFromRawWidgetNodeWithoutId
    }

// todo: rename!
let createRawChildlessWidgetNodeWithId(id: int, widgetType: string, initialProps: Map<string, obj>) : RawChildlessWidgetNodeWithId =
    { 
        Id = id
        Type = widgetType 
        Props = initialProps
    }

let createWidgetNodeFromRawWidgetNode (rawWidgetNode: RawWidgetNode) =
    createWidgetNode(rawWidgetNode.Type, rawWidgetNode.Props, [])

let widgetNodeFactory (widgetType: string, props: Map<string, obj>, children: WidgetNode list) =
    {
        Type = widgetType
        Props = new BehaviorSubject<Map<string, obj>>(props)
        Children = new BehaviorSubject<WidgetNode list>(children)
    }

module PropsHelper =
    let tryGet<'T>(key: string, props: Map<string, obj>) : 'T option =
        match props.TryFind(key) with
        | Some value -> 
            match value :?> 'T with
            | value -> Some value
            | _ -> None
        | None -> None

    let get<'T>(key: string, props: Map<string, obj>) : 'T =
        match tryGet<'T>(key, props) with
        | Some value -> value
        | None -> failwithf "Missing or invalid prop: %s" key