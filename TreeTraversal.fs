module TreeTraversal

open System
open System.Reactive.Linq
open System.Reactive.Subjects
open Newtonsoft.Json
open Types
open Services
open Externs
open WidgetNodeJsonAdapter
open WidgetHelpers

type ShadowNode<'T>(id: int, renderable: Renderable<'T>, subscribeToProps: ShadowNode<'T> -> unit) = 
    member val Id: int = id with get
    member val Renderable = renderable
    member val Children: ShadowNode<'T> list = [] with get, set
    member val PropsChangeSubscription: IDisposable option = None with get, set
    member val ChildrenChangeSubscription: IDisposable option = None with get, set

    member this.Init() =
        subscribeToProps this

let rec subscribeToPropsHelper<'T> (shadowNode: ShadowNode<'T>) =
    match shadowNode.Renderable with
    | BaseComponent component ->
        shadowNode.PropsChangeSubscription <- Some(component.Props.Skip(1).Subscribe(fun newProps ->
            printfn "new props for component %A" newProps

            // can't call traverseTree just like that, as `component` has already been processed, we just need
            // to invoke patchElement() using the new props, then process the children and look for differences

            //let newChild = traverseTree(component.Render())
            //shadowNode.Children <- [newChild]

            ignore()
        ))
    | WidgetNode widgetNode ->
        shadowNode.PropsChangeSubscription <- Some(widgetNode.Props.Skip(1).Subscribe(fun newProps ->
            printfn "new props for widget node %A" newProps

            ignore()
            
            //let newChildren = widgetNode.Children.Value
            //let newShadowChildren = 
            //    newChildren |> List.map (fun child -> traverseTree (WidgetNode child))

            //shadowNode.Children <- newShadowChildren
        ))

and handleComponent<'T>(comp: BaseComponent<'T>) =
    ignore()

and handleWidgetNode(widget: RawChildlessWidgetNodeWithId) =
    match widget.Type with
    | t when t = WidgetTypes.Button ->
        match widget.Props.TryFind("onClick") with
        | Some(onClickFunc) -> 
            match onClickFunc with
            | :? (unit -> unit) as onClickHandler -> 
                WidgetRegistrationService.registerWidgetForOnClickEvent(widget.Id, onClickHandler)
            | _ -> 
                printfn "onClick handler is not of the expected type."
        | None -> 
            printfn "No onClick handler found for button."
    | _ -> ()

and traverseTree<'T>(root: Renderable<'T>): ShadowNode<'T> =
    match root with
    | BaseComponent component ->
        component.Init()
        let child = component.Render()
        let id = WidgetRegistrationService.getNextComponentId()
        
        handleComponent(component)

        let shadowChild = traverseTree child

        let shadowNode = ShadowNode(id, BaseComponent component, subscribeToPropsHelper)
        shadowNode.Children <- [shadowChild]

        shadowNode.Init()

        shadowNode

    | WidgetNode widgetNode ->
        // Extract children and props for the WidgetNode
        let children = widgetNode.Children.Value
        let id = WidgetRegistrationService.getNextWidgetId()

        let rawNode = createRawChildlessWidgetNodeWithId(id, widgetNode.Type, widgetNode.Props.Value)

        let json = jsonAdapter.ToJson(rawNode)
        let jsonString = JsonConvert.SerializeObject(json)

        handleWidgetNode(rawNode)

        setElement(jsonString)

        let shadowChildren = 
            children |> List.map (fun child -> traverseTree (WidgetNode child))

        let childrenIds =
            shadowChildren |> List.map (fun shadowChild -> shadowChild.Id)

        let childrenJson = JsonConvert.SerializeObject(childrenIds)

        setChildren(id, childrenJson)

        let shadowNode = ShadowNode(id, WidgetNode widgetNode, subscribeToPropsHelper)
        shadowNode.Children <- List.rev shadowChildren

        shadowNode.Init()

        shadowNode