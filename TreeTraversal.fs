module TreeTraversal

open System
open System.Reactive.Linq
open System.Reactive.Subjects
open Newtonsoft.Json
open Types
open Services
open WidgetHelpers
open TreeDiffing

type ShadowNode(id: int, renderableType: string, renderable: Renderable, subscribeToProps: ShadowNode -> unit) = 
    member val Id: int = id with get
    member val Type: string = renderableType with get
    member val Renderable = renderable
    member val CurrentProps = Map.empty with get, set
    member val Children: ShadowNode list = [] with get, set
    member val PropsChangeSubscription: IDisposable option = None with get, set
    member val ChildrenChangeSubscription: IDisposable option = None with get, set

    member this.Init() =
        subscribeToProps this


let rec subscribeToPropsHelper (shadowNode: ShadowNode) =
    match shadowNode.Renderable with
    | BaseComponent component ->
        shadowNode.PropsChangeSubscription <- Some(component.Props.Skip(1).Subscribe(fun newProps ->
            printfn "new props for component %A" newProps

            if arePropsDifferent(shadowNode.CurrentProps, newProps) then
                printfn "Yes, different: %A %A" shadowNode.CurrentProps newProps

                let shadowChild = component.Render()

                printfn "new props for component %A" shadowChild

                //ignore()

                traverseTree shadowChild

                //shadowNode.Children <- [traverseTree shadowChild]
                //shadowNode.CurrentProps <- newProps

                //let childrenJson = JsonConvert.SerializeObject(shadowNode.Children |> List.map (fun child -> child.Id))

                //setChildren(shadowNode.Id, childrenJson)

                ignore()

                //ignore()

            //shadowNode.CurrentProps <- newProps

            ignore()
        ))
    | WidgetNode widgetNode ->
        shadowNode.PropsChangeSubscription <- Some(widgetNode.Props.Skip(1).Subscribe(fun newProps ->
            printfn "new props for widget node %A" newProps

            WidgetRegistrationService.patchWidget(shadowNode.Id, newProps)

            shadowNode.CurrentProps <- newProps

            ignore()
        ))

and updateTree(root: Renderable) =
    match root with
    | BaseComponent component ->
        ignore()

    | WidgetNode widgetNode ->
        ignore()

and handleComponent(comp: BaseComponent) =
    ignore()

and handleWidgetNode(widget: RawChildlessWidgetNodeWithId) =
    match widget.Type with
    | t when t = WidgetTypes.Button ->
        let onClick = PropsHelper.tryGet<unit -> unit>("onClick", widget.Props)

        match onClick with
        | Some handler -> 
            WidgetRegistrationService.registerWidgetForOnClickEvent(widget.Id, handler)
        | None -> 
            printfn "No onClick handler for button."
    | _ -> ()

and traverseTree(root: Renderable): ShadowNode =
    match root with
    | BaseComponent component ->
        component.Init()
        let child = component.Render()
        let id = WidgetRegistrationService.getNextComponentId()
        
        handleComponent(component)

        let shadowChild = traverseTree child

        let shadowNode = ShadowNode(id, WidgetTypes.Component, BaseComponent component, subscribeToPropsHelper)
        shadowNode.Children <- [shadowChild]
        shadowNode.CurrentProps <- component.Props.Value

        printfn "shadowNode.CurrentProps %A" component.Props.Value

        shadowNode.Init()

        shadowNode

    | WidgetNode widgetNode ->
        // Extract children and props for the WidgetNode
        let children = widgetNode.Children.Value
        let id = WidgetRegistrationService.getNextWidgetId()
        let rawNode = createRawChildlessWidgetNodeWithId(id, widgetNode.Type, widgetNode.Props.Value)

        handleWidgetNode(rawNode)

        WidgetRegistrationService.createWidget(rawNode)

        printfn "after setElement()"

        let shadowChildren = 
            children |> List.map (fun child -> traverseTree child)

        let childrenIds =
            shadowChildren |> List.map (fun shadowChild -> shadowChild.Id)

        WidgetRegistrationService.linkChildren(id, childrenIds)

        let shadowNode = ShadowNode(id, widgetNode.Type, WidgetNode widgetNode, subscribeToPropsHelper)
        shadowNode.Children <- List.rev shadowChildren
        shadowNode.CurrentProps <- widgetNode.Props.Value

        shadowNode.Init()

        shadowNode