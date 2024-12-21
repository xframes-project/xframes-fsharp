module Widgets

open System.Reactive.Subjects
open Types
open WidgetHelpers



let makeRootNode (children: Renderable list) =
    let props = Map.ofList [("root", box true)]
    widgetNodeFactory(WidgetTypes.Node, props, children)

let node (children: Renderable list) =
    let props = Map.ofList [("root", box false)]
    widgetNodeFactory(WidgetTypes.Node, props, children)

let unformattedText (text: string) =
    let props = Map.ofList [("text", box text)]
    widgetNodeFactory(WidgetTypes.UnformattedText, props, [])

let button (label: string, onClick: (unit -> unit) option) =
    let props =
        Map.ofList [
            ("label", box label)
            match onClick with
            | Some handler -> ("onClick", box handler)
            | None -> ()
        ]
    widgetNodeFactory(WidgetTypes.Button, props, [])

let rec normalizeRawWidgetNodeWithIdTree(node: RawWidgetNodeWithId): RawWidgetNode =
    {
        Type = node.Type
        Props = node.Props
        Children = node.Children |> List.map normalizeRawWidgetNodeWithIdTree
    }

//let rec normalizeWidgetNodeTree(node: WidgetNode): RawWidgetNode =
//    {
//        Type = node.Type
//        Props = node.Props.Value
//        Children = node.Children.Value |> List.map normalizeWidgetNodeTree
//    }




