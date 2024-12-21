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
                printfn "Yes, different"

                let shallowRenderableOuput = component.Render()
                
                match shallowRenderableOuput with
                | BaseComponent _baseComponent -> ignore()
                | WidgetNode widgetNode -> 
                    //let itemDiffs =
                    //    [0 .. max shadowNode.Children.Length widgetNode.Children.Value.Length - 1]
                    //    |> List.choose (fun i ->
                    //        match List.tryItem i shadowNode.Children, List.tryItem i widgetNode.Children.Value with
                    //        | Some c1, Some c2 ->
                    //            if c1.Type <> c2.Type then
                    //                // type mismatch
                    //                ignore()
                    //            None
                    //        | Some c1, None -> Some { Name = sprintf "Child[%d]" i; Diff = Diff.Value(c1, null) }
                    //        | None, Some c2 -> Some { Name = sprintf "Child[%d]" i; Diff = Diff.Value(null, c2) }
                    //        | None, None -> None
                    //    )
                    ignore()

                

                for childShadowNode in shadowNode.Children do
                    childShadowNode.Renderable

                match shallowRenderableOuput with
                | BaseComponent component -> ignore()
                | WidgetNode widgetNode -> ignore()


            shadowNode.CurrentProps <- newProps

            ignore()
        ))
    | WidgetNode widgetNode ->
        shadowNode.PropsChangeSubscription <- Some(widgetNode.Props.Skip(1).Subscribe(fun newProps ->
            printfn "new props for widget node %A" newProps

            patchElement(shadowNode.Id, JsonConvert.SerializeObject(newProps));

            shadowNode.CurrentProps <- newProps

            ignore()
            
            //let newChildren = widgetNode.Children.Value
            //let newShadowChildren = 
            //    newChildren |> List.map (fun child -> traverseTree (WidgetNode child))

            //shadowNode.Children <- newShadowChildren
        ))

let rec updateTree(root: Renderable) =
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

        let shadowNode = ShadowNode(id, widgetNode.Type, WidgetNode widgetNode, subscribeToPropsHelper)
        shadowNode.Children <- List.rev shadowChildren
        shadowNode.CurrentProps <- widgetNode.Props.Value

        shadowNode.Init()

        shadowNode