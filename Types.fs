module Types

open System.Collections.Generic
open System.Reactive.Subjects

type WidgetNode = {
    Id: int
    Type: string
    mutable Props: BehaviorSubject<Map<string, obj>>
    mutable Children: WidgetNode list
}

type Theme2(colorsDict: Dictionary<int, List<obj>>) =
    member val colors = colorsDict with get, set


type FontDef = {
    name: string
    size: int
}
