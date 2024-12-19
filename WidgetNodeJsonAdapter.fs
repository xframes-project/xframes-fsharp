module WidgetNodeJsonAdapter

open System.Reactive.Subjects
open Types

type WidgetNodeJsonAdapter() =
    /// Converts a JSON-like Map<string, obj> to a WidgetNode
    member this.FromJson(json: Map<string, obj>) =
        let ``type`` = json.["type"] :?> string
        let id = json.["id"] :?> int
        let props =
            json 
            |> Map.remove "id" 
            |> Map.remove "type"

        // Initialize WidgetNode with reactive Props
        {
            //Id = id
            Type = ``type``
            Props = new BehaviorSubject<Map<string, obj>>(props)
            Children = new BehaviorSubject<WidgetNode list>([]) // Modify if children are part of the JSON
        }

    member this.ToJson(widgetNode: RawWidgetNodeWithId) =
        let props = widgetNode.Props

        props
        |> Map.add "id" (box widgetNode.Id)
        |> Map.add "type" (box widgetNode.Type)

    member this.ToJson(widgetNode: RawChildlessWidgetNodeWithId) =
        let props = widgetNode.Props

        props
        |> Map.add "id" (box widgetNode.Id)
        |> Map.add "type" (box widgetNode.Type)


let jsonAdapter = WidgetNodeJsonAdapter()