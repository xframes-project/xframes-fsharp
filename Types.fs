module Types

open System.Collections.Generic
open System.Reactive.Subjects

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

type IHasProps<'T> =
    abstract member Props: BehaviorSubject<'T>

type WidgetNode = 
    { 
        Type: string
        Props: BehaviorSubject<Map<string, obj>>
        Children: BehaviorSubject<WidgetNode list> 
    } 
    interface IHasProps<Map<string, obj>> with
        member this.Props = this.Props

and Renderable<'T> =
    | Component of IComponent<'T>
    | WidgetNode of WidgetNode

and IComponent<'T> =
    inherit IHasProps<'T>
    abstract member Render: unit -> Renderable<'T>

type ShadowNode =
    | ShadowComponent of int * string * Map<string, obj> * ShadowNode list
    | ShadowWidget of int * string * Map<string, obj> * ShadowNode list

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
