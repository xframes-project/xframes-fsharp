module Types

open System.Collections.Generic
open System.Reactive.Subjects
open System

type WidgetTypes =
    | Component
    | Node
    | UnformattedText
    | Button
    member this.ToString() =
        match this with
        | Component -> "component"
        | Node -> "node"
        | UnformattedText -> "unformatted-text"
        | Button -> "di-button"

type RawWidgetNode = {
    Type: WidgetTypes
    Props: Map<string, obj>
    mutable Children: RawWidgetNode list
}

type RawWidgetNodeWithId = {
    Id: int
    Type: WidgetTypes
    Props: Map<string, obj>
    mutable Children: RawWidgetNodeWithId list
}

type RawChildlessWidgetNodeWithId = {
    Id: int
    Type: WidgetTypes
    Props: Map<string, obj>
}

[<AbstractClass>]
type BaseComponent() =
    member val Props = new BehaviorSubject<Map<string, obj>>(Map.empty) with get, set

    abstract member Init: unit -> unit
    abstract member Destroy: unit -> unit
    abstract member Render: unit -> Renderable

and Renderable =
    | BaseComponent of BaseComponent
    | WidgetNode of WidgetNode

and WidgetNode = 
    { 
        Type: WidgetTypes
        Props: BehaviorSubject<Map<string, obj>>
        Children: BehaviorSubject<Renderable list> 
    }

type Theme2(colorsDict: Dictionary<int, List<obj>>) =
    member val colors = colorsDict with get, set


type FontDef = {
    name: string
    size: int
}

type AppState = {
    Text: string
    Count: int
}
