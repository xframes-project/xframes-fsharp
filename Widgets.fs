module Widgets

open Types
open WidgetHelpers

let addStyleIfPresent styleKey styleOption props =
    match styleOption with
    | Some style -> Map.add styleKey (box style) props
    | None -> props

let makeRootNode (children: Renderable list) =
    let props = Map.ofList [("root", box true)]
    widgetNodeFactory(WidgetTypes.Node, props, children)

let node (children: Renderable list) =
    let props = Map.ofList [("root", box false)]
    widgetNodeFactory(WidgetTypes.Node, props, children)

let unformattedText (text: string, style: WidgetStyle option) =
    let baseProps = Map.ofList [("text", box text)]
    // Conditionally add each style to the properties map if provided
    let addStyleIfPresent styleKey styleOption props =
        match styleOption with
        | Some style -> Map.add styleKey (box style) props
        | None -> props

    let props =
        baseProps
        |> addStyleIfPresent "style" style
    //|> addStyleIfPresent "hoverStyle" hoverStyle
    //|> addStyleIfPresent "activeStyle" activeStyle
    //|> addStyleIfPresent "disabledStyle" disabledStyle

    widgetNodeFactory(WidgetTypes.UnformattedText, props, [])

let button (label: string, onClick: (unit -> unit) option, style: WidgetStyle option) =
    let baseProps =
        Map.ofList [
            ("label", box label)
            match onClick with
            | Some handler -> ("onClick", box handler)
            | None -> ()
        ]

    let props =
        baseProps
        |> addStyleIfPresent "style" style

    widgetNodeFactory(WidgetTypes.Button, props, [])

let rec normalizeRawWidgetNodeWithIdTree(node: RawWidgetNodeWithId): RawWidgetNode =
    {
        Type = node.Type
        Props = node.Props
        Children = node.Children |> List.map normalizeRawWidgetNodeWithIdTree
    }






