module Widgets

open System.Reactive.Subjects
open Types
open WidgetHelpers

let makeRootNode (children: WidgetNode list) =
    let props = Map.ofList [("root", box true)]
    widgetNodeFactory("node", props, children)

let node (children: WidgetNode list) =
    let props = Map.ofList []
    widgetNodeFactory("node", props, children)

let unformattedText (text: string) =
    let props = Map.ofList [("text", box text)]
    widgetNodeFactory("unformatted-text", props, [])

let button (label: string, onClick: (unit -> unit) option) =
    let props =
        Map.ofList [
            ("label", box label)
            match onClick with
            | Some handler -> ("onClick", box handler)
            | None -> ()
        ]
    widgetNodeFactory("di-button", props, [])

let rec normalizeRawWidgetNodeWithIdTree (node: RawWidgetNodeWithId): RawWidgetNode =
    {
        Type = node.Type
        Props = node.Props
        Children = node.Children |> List.map normalizeRawWidgetNodeWithIdTree
    }

let rec normalizeWidgetNodeTree (node: WidgetNode): RawWidgetNode =
    {
        Type = node.Type
        Props = node.Props.Value
        Children = node.Children.Value |> List.map normalizeWidgetNodeTree
    }


type BaseComponent<'T>() =
    let defaultProps = new BehaviorSubject<'T>(Unchecked.defaultof<'T>)
    interface IComponent<'T> with
        member this.Props = defaultProps
        member this.Render() =
            // Abstract method to be overridden
            failwith "Render method must be implemented"

type Inner() =
    inherit BaseComponent<Map<string, obj>>()
    interface IComponent<Map<string, obj>> with
        member this.Render() =
            WidgetNode (
                makeRootNode [
                    unformattedText "hello"
                ]
            )

type App() =
    inherit BaseComponent<Map<string, obj>>()
    interface IComponent<Map<string, obj>> with
        member this.Render() =
            Component(Inner())




