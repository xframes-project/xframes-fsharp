module TreeTraversal

open System
open System.Reactive.Linq
open System.Reactive.Subjects
open Newtonsoft.Json
open Types
open Services
open Externs
open Widgets
open WidgetNodeJsonAdapter

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

let rec traverseTree<'T>(root: Renderable<'T>): ShadowNode =
    match root with
    | Component component ->
        // Extract props and children for the component
        let props = Map.empty // Placeholder for actual props (e.g., from component)
        let child = component.Render()
        let id = WidgetRegistrationService.getNextComponentId()
        

        // Subscribe to changes in the component's props
        shadowNodeManager.SubscribeToBehaviorSubject(component.Props, fun _ ->
            printfn "Component Props or Children changed, re-rendering..."
            // Trigger re-render for this specific component
        )

        // Recursively traverse the child
        let shadowChild = traverseTree child

        let shadowWidget = 
            { 
                Id = id
                Type = "Component"
                Props = props
                Children = [shadowChild]
            }

        shadowWidget

    | WidgetNode widgetNode ->
        // Extract children and props for the WidgetNode
        let children = widgetNode.Children.Value
        let props = widgetNode.Props.Value
        let id = WidgetRegistrationService.getNextWidgetId()

        let rawNode = createRawChildlessWidgetNodeWithId(id, widgetNode.Type, widgetNode.Props.Value)

        let json = jsonAdapter.ToJson(rawNode)
        let jsonString = JsonConvert.SerializeObject(json)

        printfn "%s" jsonString

        setElement(jsonString)

        // Subscribe to changes in props and children
        shadowNodeManager.SubscribeToBehaviorSubject(widgetNode.Props, fun newProps ->
            printfn "WidgetProps for %s changed: %A" widgetNode.Type newProps
            // Trigger re-render for this specific widget node
        )

        shadowNodeManager.SubscribeToBehaviorSubject(widgetNode.Children, fun newChildren ->
            printfn "WidgetChildren for %s changed: %A" widgetNode.Type newChildren
            // Trigger re-render or other update logic here
        )

        let shadowChildren = 
            children |> List.map (fun child -> traverseTree (WidgetNode child))

        let childrenIds =
            shadowChildren |> List.map (fun shadowChild -> shadowChild.Id)

        let childrenJson = JsonConvert.SerializeObject(childrenIds)

        setChildren(id, childrenJson)

        let shadowWidget = 
            { 
                Id = id
                Type = widgetNode.Type
                Props = props
                Children = List.rev shadowChildren 
            }

        shadowWidget