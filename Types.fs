module Types

open System.Collections.Generic


type WidgetNode = {
    Id: int
    Type: string
    mutable Props: Map<string, obj>
    mutable Children: WidgetNode list
}

type Theme2(colorsDict: Dictionary<int, List<obj>>) =
    member val colors = colorsDict with get, set


type FontDef = {
    name: string
    size: int
}
