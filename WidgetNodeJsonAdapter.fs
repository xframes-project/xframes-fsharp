module WidgetNodeJsonAdapter

open System.Reactive.Subjects
open Types

type WidgetNodeJsonAdapter() =
    member this.FromJson(json: Map<string, obj>) =
        ignore()

    member this.ToJson(widgetNode: RawWidgetNodeWithId) : Map<string, obj> =
        let props = widgetNode.Props |> Map.map (fun _ v -> box v)
        props
        |> Map.add "id" (box widgetNode.Id)
        |> Map.add "type" (box (widgetNode.Type.ToString()))

    member this.ToJson(widgetNode: RawChildlessWidgetNodeWithId) : Map<string, obj> =
        let props = widgetNode.Props |> Map.map (fun _ v -> box v)
        props
        |> Map.add "id" (box widgetNode.Id)
        |> Map.add "type" (box (widgetNode.Type.ToString()))


let jsonAdapter = WidgetNodeJsonAdapter()