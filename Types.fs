module Types

open System.Collections.Generic
open System.Reactive.Subjects

type WidgetTypes =
    static member Component = "component"
    static member Node = "node"
    static member UnformattedText = "unformatted-text"
    static member Button = "di-button"

type RawWidgetNode = {
    Type: string
    Props: Map<string, obj>
    mutable Children: RawWidgetNode list
}

type RawWidgetNodeWithId = {
    Id: int
    Type: string
    Props: Map<string, obj>
    mutable Children: RawWidgetNodeWithId list
}

type RawChildlessWidgetNodeWithId = {
    Id: int
    Type: string
    Props: Map<string, obj>
}

type WidgetNode = 
    { 
        Type: string
        Props: BehaviorSubject<Map<string, obj>>
        Children: BehaviorSubject<WidgetNode list> 
    }

[<AbstractClass>]
type BaseComponent<'T>() =
    member val Props = new BehaviorSubject<'T>(Unchecked.defaultof<'T>) with get, set

    abstract member Init: unit -> unit
    abstract member Destroy: unit -> unit
    abstract member Render: unit -> Renderable<'T>

and Renderable<'T> =
    | BaseComponent of BaseComponent<'T>
    | WidgetNode of WidgetNode

type ShadowNode = 
    { 
        Id: int
        Type: string
        Props: Map<string, obj>
        Children: ShadowNode list 
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
