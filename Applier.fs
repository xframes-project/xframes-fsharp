module Applier

open System.Collections.Generic
open Newtonsoft.Json
open Types
open WidgetNodeJsonAdapter
open WidgetHelpers
open Services
open Externs

type IApplier<'T> =
    abstract Current: 'T
    abstract OnBeginChanges: unit -> unit
    abstract OnEndChanges: unit -> unit
    abstract Down: node: 'T -> unit
    abstract Up: unit -> unit
    abstract InsertTopDown: int * 'T -> unit
    abstract InsertBottomUp: int * 'T -> unit
    abstract Remove: int * int -> unit
    abstract Move: int * int * int -> unit
    abstract Clear: unit -> unit



[<AbstractClass>]
type AbstractApplier<'T>(root: 'T) =
    let stack = ResizeArray<'T>()
    let mutable current = root

    member this.Root = root
    member this.Stack = stack
    member this.Current
        with get() = current
        and set(value) = current <- value

    abstract member InsertTopDown: index: int * instance: RawWidgetNode -> unit
    abstract member InsertBottomUp: index: int * instance: RawWidgetNode -> unit
    abstract member Remove: index: int * count: int -> unit
    abstract member Move: fromIndex: int * toIndex: int * count: int -> unit
    abstract member OnClear: unit -> unit

    interface IApplier<'T> with
        member this.Current = current

        member this.OnBeginChanges() = ()
        member this.OnEndChanges() = ()
        member this.Down(node) =
            stack.Add(current)
            current <- node
        member this.Up() =
            if stack.Count > 0 then
                current <- stack.[stack.Count - 1]
                stack.RemoveAt(stack.Count - 1)
            else failwith "Empty stack"

        member this.InsertTopDown(index, instance) = failwith "Not implemented"
        member this.InsertBottomUp(index, instance) = failwith "Not implemented"
        member this.Remove(index, count) =
            if count = 1 then stack.RemoveAt(index)
            else stack.RemoveRange(index, count)

        member this.Move(fromIndex, toIndex, count) =
            let dest = if fromIndex > toIndex then toIndex else toIndex - count
            if count = 1 then
                let item = stack.[fromIndex]
                stack.RemoveAt(fromIndex)
                stack.Insert(dest, item)
            else
                let items = stack.GetRange(fromIndex, count)
                stack.RemoveRange(fromIndex, count)
                stack.InsertRange(dest, items)

        member this.Clear() =
            stack.Clear()
            current <- root
            this.OnClear()




type WidgetTreeApplier(jsonAdapter: WidgetNodeJsonAdapter, root: RawWidgetNodeWithId) =
    inherit AbstractApplier<RawWidgetNodeWithId>(root)

    // Helper functions for serialization and deserialization
    let serializeToJson (data: obj) = JsonConvert.SerializeObject(data)
    let deserializeList (json: string) =
        JsonConvert.DeserializeObject<List<int>>(json)

    // Reactive helper: Update `Children` safely
    let updateChildren (node: WidgetNode) (updateFn: WidgetNode list -> WidgetNode list) =
        let currentChildren = node.Children.Value
        node.Children.OnNext(updateFn currentChildren)

    // Clear all children of the root
    override this.OnClear() =
        root.Children <- []

    // Insert a node bottom-up (logic can be customized)
    override this.InsertBottomUp(index: int, instance: RawWidgetNode) =
        ignore()

    // Insert a node top-down
    override this.InsertTopDown(index: int, instance: RawWidgetNode) =
        let widgetWithId = createRawWidgetNodeWithIdFromRawWidgetNodeWithoutId(instance)

        WidgetRegistrationService.registerWidget(widgetWithId.Id, widgetWithId)

        match widgetWithId.Props.TryFind("onClick") with
        | Some value ->
            match value with
            | :? (unit -> unit) as onClickFn -> WidgetRegistrationService.registerWidgetForOnClickEvent(widgetWithId.Id, onClickFn)
            | _ -> ignore()
        | None -> ignore()
        
        let json = jsonAdapter.ToJson(widgetWithId)
        let jsonString = serializeToJson json

        printfn "%s" jsonString

        setElement(jsonString)

        let childrenJson = serializeToJson [ widgetWithId.Id ]
        setChildren(this.Current.Id, childrenJson)

        this.Root.Children <- this.Root.Children @ [widgetWithId]

        ignore()

    // Move nodes within the current node's children
    override this.Move(fromIndex: int, toIndex: int, count: int) =
        let moveItems (list: WidgetNode list) fromIndex toIndex count =
            let itemsToMove = list |> List.skip fromIndex |> List.take count
            let remaining = 
                list 
                |> List.mapi (fun i x -> if i >= fromIndex && i < fromIndex + count then None else Some x)
                |> List.choose id
            let (before, after) = remaining |> List.splitAt toIndex
            before @ itemsToMove @ after

        //updateChildren this.Current (fun children ->
        //    moveItems children fromIndex toIndex count
        //)
        ignore()

    // Remove a range of children
    override this.Remove(index: int, count: int) =
        ignore()
        //updateChildren this.Current (fun children ->
        //    children.[0..index - 1] @ children.[index + count..]
        //)