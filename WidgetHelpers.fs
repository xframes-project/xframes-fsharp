module WidgetHelpers

open System.Security.Cryptography
open System.Text
open System.Reactive.Subjects
open Types
open Services

let createWidgetNode(widgetType: WidgetTypes, initialProps: Map<string, obj>, initialChildren: Renderable list) : WidgetNode =
    { 
        Type = widgetType; 
        Props = new BehaviorSubject<Map<string, obj>>(initialProps); 
        Children = new BehaviorSubject<Renderable list>(initialChildren) 
    }

let createRawWidgetNode(widgetType: WidgetTypes, initialProps: Map<string, obj>, initialChildren: RawWidgetNode list) : RawWidgetNode =
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

let createRawWidgetNodeWithId(id: int, widgetType: WidgetTypes, initialProps: Map<string, obj>, initialChildren: RawWidgetNode list) : RawWidgetNodeWithId =
    { 
        Id = id
        Type = widgetType 
        Props = initialProps
        Children = initialChildren |> List.map createRawWidgetNodeWithIdFromRawWidgetNodeWithoutId
    }

// todo: rename!
let createRawChildlessWidgetNodeWithId(id: int, widgetType: WidgetTypes, initialProps: Map<string, obj>) : RawChildlessWidgetNodeWithId =
    { 
        Id = id
        Type = widgetType 
        Props = initialProps
    }

let createWidgetNodeFromRawWidgetNode (rawWidgetNode: RawWidgetNode) =
    createWidgetNode(rawWidgetNode.Type, rawWidgetNode.Props, [])

let widgetNodeFactory (widgetType: WidgetTypes, props: Map<string, obj>, children: Renderable list) =
    {
        Type = widgetType
        Props = new BehaviorSubject<Map<string, obj>>(props)
        Children = new BehaviorSubject<Renderable list>(children)
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

let computeHash (componentType: string, props: Map<string, obj>) =
    let propsString = 
        props
        |> Map.toSeq
        |> Seq.map (fun (key, value) -> sprintf "%s:%O" key value)
        |> String.concat ";"

    let input = sprintf "%s|%s" componentType propsString
    use sha256 = SHA256.Create()
    let bytes = Encoding.UTF8.GetBytes(input)
    sha256.ComputeHash(bytes)
    |> Array.map (fun b -> b.ToString("x2"))
    |> String.concat ""