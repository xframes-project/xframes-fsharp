module Applier

open System.Collections.Generic
open Types

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




//type OffsetApplier<'T>(applier: IApplier<'T>, offset: int) =
//    let mutable nesting = 0

//    interface IApplier<'T> with
//        member _.Current = applier.Current

//        member _.OnBeginChanges() = applier.OnBeginChanges()
//        member _.OnEndChanges() = applier.OnEndChanges()
        
//        member _.Down(node) =
//            nesting <- nesting + 1
//            applier.Down(node)

//        member _.Up() =
//            if nesting <= 0 then failwith "OffsetApplier up called with no corresponding down"
//            nesting <- nesting - 1
//            applier.Up()

//        member _.InsertTopDown(index, instance) =
//            let effectiveIndex = if nesting = 0 then index + offset else index
//            applier.InsertTopDown(effectiveIndex, instance)

//        member _.InsertBottomUp(index, instance) =
//            let effectiveIndex = if nesting = 0 then index + offset else index
//            applier.InsertBottomUp(effectiveIndex, instance)

//        member _.Remove(index, count) =
//            let effectiveIndex = if nesting = 0 then index + offset else index
//            applier.Remove(effectiveIndex, count)

//        member _.Move(fromIndex, toIndex, count) =
//            let effectiveFrom = if nesting = 0 then fromIndex + offset else fromIndex
//            let effectiveTo = if nesting = 0 then toIndex + offset else toIndex
//            applier.Move(effectiveFrom, effectiveTo, count)

//        member _.Clear() = failwith "Clear is not valid on OffsetApplier"

