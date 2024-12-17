module Widgets

open System
open System.Reactive.Linq
open System.Reactive.Subjects
open Types
open Services
open DEdge.Diffract

let createWidgetNode(widgetType: string, initialProps: Map<string, obj>, initialChildren: WidgetNode list) : WidgetNode =
    { 
        Type = widgetType; 
        Props = new BehaviorSubject<Map<string, obj>>(initialProps); 
        Children = new BehaviorSubject<WidgetNode list>(initialChildren) 
    }

let createRawWidgetNode (widgetType: string, initialProps: Map<string, obj>, initialChildren: RawWidgetNode list) : RawWidgetNode =
    { 
        Type = widgetType; 
        Props = initialProps; 
        Children = initialChildren 
    }

let rec createRawWidgetNodeWithIdFromRawWidgetNodeWithoutId (rawWidgetNode: RawWidgetNode) : RawWidgetNodeWithId =
    { 
        Id = WidgetRegistrationService.getNextWidgetId()
        Type = rawWidgetNode.Type; 
        Props = rawWidgetNode.Props; 
        Children = rawWidgetNode.Children |> List.map createRawWidgetNodeWithIdFromRawWidgetNodeWithoutId
    }

let createRawWidgetNodeWithId (id: int, widgetType: string, initialProps: Map<string, obj>, initialChildren: RawWidgetNode list) : RawWidgetNodeWithId =
    { 
        Id = id
        Type = widgetType 
        Props = initialProps
        Children = initialChildren |> List.map createRawWidgetNodeWithIdFromRawWidgetNodeWithoutId
    }

let createWidgetNodeFromRawWidgetNode (rawWidgetNode: RawWidgetNode) =
    createWidgetNode(rawWidgetNode.Type, rawWidgetNode.Props, [])

let updateProps (widget: WidgetNode) key value =
    let newProps = widget.Props.Value.Add(key, value)
    widget.Props.OnNext(newProps)

let observeProps (widget: WidgetNode) =
    widget.Props.Subscribe(fun newProps ->
        printfn "Props updated: %A" newProps
    )


let widgetNodeFactory (widgetType: string, props: Map<string, obj>, children: WidgetNode list) =
    {
        Type = widgetType
        Props = new System.Reactive.Subjects.BehaviorSubject<Map<string, obj>>(props)
        Children = new System.Reactive.Subjects.BehaviorSubject<WidgetNode list>(children)
    }

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

and arePropsDifferent (props1: Map<string, obj>) (props2: Map<string, obj>) =
    props1 |> Seq.exists (fun kvp ->
        match props2.TryFind kvp.Key with
        | Some value -> value <> kvp.Value
        | None -> true
    ) || props2 |> Seq.exists (fun kvp ->
        not (props1.ContainsKey kvp.Key)
    )

and diffNodes (node1: RawWidgetNode, node2: RawWidgetNode) : Option<Diff> =
    // Compare the types first
    if node1.Type <> node2.Type then
        Some(Diff.Value(node1.Type, node2.Type))
    
    // Compare the props
    elif arePropsDifferent node1.Props node2.Props then
        Some(Diff.Record [
            { Name = "Props"; Diff = Diff.Value(node1.Props, node2.Props) }
        ])
    
    // Compare the children
    else
        match diffChildren(node1.Children, node2.Children) with
        | Some(Diff.Collection(_, _, childDiffs)) when childDiffs.Count = 0 ->
            None // No differences in children, return None
            
        | Some(Diff.Collection(_, _, childDiffs)) -> 
            Some(Diff.Record [
                { Name = "Children"; Diff = Diff.Collection(0, 0, childDiffs) }
            ])
        
        | None -> 
            None // No child differences

            
and diffChildren (children1: RawWidgetNode list, children2: RawWidgetNode list) : Option<Diff> =
    let count1 = List.length children1
    let count2 = List.length children2

    // Generate the list of differences between the child nodes
    let itemDiffs =
        [0 .. max count1 count2 - 1]
        |> List.choose (fun i ->
            match List.tryItem i children1, List.tryItem i children2 with
            | Some c1, Some c2 ->
                match diffNodes(c1, c2) with
                | Some diff when diff <> Diff.Value(c1, c2) ->
                    Some { Name = sprintf "Child[%d]" i; Diff = diff }
                | _ -> None
            | Some c1, None -> Some { Name = sprintf "Child[%d]" i; Diff = Diff.Value(c1, null) }
            | None, Some c2 -> Some { Name = sprintf "Child[%d]" i; Diff = Diff.Value(null, c2) }
            | None, None -> None
        )

    // If there are differences in the children, return Diff.Collection wrapped in Some, otherwise None.
    if List.isEmpty itemDiffs then
        None
    else
        Some(Diff.Collection(count1, count2, itemDiffs))

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
                node [
                    unformattedText "hello"
                ]
            )

type App() =
    inherit BaseComponent<Map<string, obj>>()
    interface IComponent<Map<string, obj>> with
        member this.Render() =
            Component(Inner())

type ShadowNodeManager() =
    // Store subscriptions for props and children
    let subscriptions = new System.Collections.Generic.Dictionary<obj, IDisposable>()

    // Function to subscribe to a BehaviorSubject and handle reactivity
    member this.SubscribeToBehaviorSubject<'T>(subject: BehaviorSubject<'T>, onNext: 'T -> unit) =
        // If already subscribed, don't subscribe again
        if not (subscriptions.ContainsKey(subject)) then
            // Skip very first observable as the component would have just been rendered
            let subscription = subject.Skip(1).Subscribe(onNext)
            subscriptions.Add(subject, subscription)
    
    // Function to unsubscribe from a BehaviorSubject
    member this.UnsubscribeFromBehaviorSubject(subject: obj) =
        // Try to get the subscription associated with the subject
        match subscriptions.TryGetValue(subject) with
        | true, subscription ->
            // Dispose of the subscription if found
            subscription.Dispose()
            // Remove the subject from the dictionary
            subscriptions.Remove(subject) |> ignore
        | _ -> 
            printfn "No subscription found for the provided subject"

let shadowNodeManager = ShadowNodeManager()

let rec traverseTree<'T>(root: Renderable<'T>): ShadowNode * int =
    match root with
    | Component component ->
        // Extract props and children for the component
        let props = Map.empty // Placeholder for actual props (e.g., from component)
        let child = component.Render()

        // Subscribe to changes in the component's props
        shadowNodeManager.SubscribeToBehaviorSubject(component.Props, fun _ ->
            printfn "Component Props or Children changed, re-rendering..."
            // Trigger re-render for this specific component
        )

        // Recursively traverse the child
        let shadowChild, nextId = traverseTree child
        ShadowComponent(WidgetRegistrationService.getNextWidgetId(), "Component", props, [shadowChild]), nextId

    | WidgetNode widgetNode ->
        // Extract children and props for the WidgetNode
        let children = widgetNode.Children.Value
        let props = widgetNode.Props.Value

        // Subscribe to changes in props and children
        shadowNodeManager.SubscribeToBehaviorSubject(widgetNode.Props, fun newProps ->
            printfn "WidgetProps for %s changed: %A" widgetNode.Type newProps
            // Trigger re-render for this specific widget node
        )

        shadowNodeManager.SubscribeToBehaviorSubject(widgetNode.Children, fun newChildren ->
            printfn "WidgetChildren for %s changed: %A" widgetNode.Type newChildren
            // Trigger re-render or other update logic here
        )

        // Recursively process children
        let shadowChildren, nextId =
            children |> List.fold (fun (acc, id) child ->
                let shadowChild, newId = traverseTree (WidgetNode child)
                (shadowChild :: acc, newId)
            ) ([], WidgetRegistrationService.getNextWidgetId())

        ShadowWidget(WidgetRegistrationService.getNextWidgetId(), widgetNode.Type, props, List.rev shadowChildren), nextId



