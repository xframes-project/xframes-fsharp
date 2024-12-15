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

type WidgetNode = {
    Type: string
    Props: BehaviorSubject<Map<string, obj>>
    Children: BehaviorSubject<WidgetNode list>
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
