module Services
open Newtonsoft.Json
open System.Reactive.Subjects
open System.Threading
open System.Collections.Generic
open Externs
open Types

module WidgetRegistrationService =
    let idGeneratorLock = new ReaderWriterLockSlim()
    let idRegistrationLock = new ReaderWriterLockSlim()

    // Widget registry
    let widgetRegistry = new Dictionary<int, RawWidgetNodeWithId>()
    //let onClickRegistry = new Dictionary<int, unit -> unit>()

    let onClickRegistry = new BehaviorSubject<Map<int, unit -> unit>>(Map.empty)

    // Widget ID management
    let mutable lastWidgetId = 0

    let mutable lastComponentId = 0

    let getWidgetById (id: int) =
        // Return widget from registry by ID
        if widgetRegistry.ContainsKey(id) then Some(widgetRegistry.[id]) else None

    let registerWidget (id: int, widget: RawWidgetNodeWithId) =
        idRegistrationLock.EnterWriteLock()
        try
            widgetRegistry.[id] <- widget
        finally
            idRegistrationLock.ExitWriteLock()

    let getNextWidgetId () =
        idGeneratorLock.EnterWriteLock()
        try
            let id = lastWidgetId
            lastWidgetId <- lastWidgetId + 1
            id
        finally
            idGeneratorLock.ExitWriteLock()

    let getNextComponentId () =
        idGeneratorLock.EnterWriteLock()
        try
            let id = lastComponentId
            lastComponentId <- lastComponentId + 1
            id
        finally
            idGeneratorLock.ExitWriteLock()

    let registerWidgetForOnClickEvent(id: int, fn: (unit -> unit)) =
        onClickRegistry.OnNext(onClickRegistry.Value.Add(id, fn))

    let dispatchOnClickEvent id =
        match onClickRegistry.Value.TryFind id with
        | Some fn -> 
            printfn "About to invoke onClick fn for widget %d" id
            fn()
        | None -> printfn "No event handler for ID %d" id

    let getStyle () =
        // Assuming `xFramesWrapper` has a method `getStyle()`
        // Placeholder for style retrieval
        ""

    let setData (id: int) (data: List<obj>) =
        //let opDictionary = SetData
        //opDictionary.Add("")
        elementInternalOp(id, JsonConvert.SerializeObject(""))

    let appendData (id: int) (data: List<obj>) =
        //let jsonData = JsonAppendData(Op = "appendData", Data = data)
        elementInternalOp(id, JsonConvert.SerializeObject(""))

    let resetData (id: int) =
        //let jsonData = JsonResetData(Op = "resetData")
        elementInternalOp(id, JsonConvert.SerializeObject(""))

    let appendDataToPlotLine (id: int) (x: float) (y: float) =
        //let jsonData = JsonAppendDataToPlotLine(Op = "appendData", X = x, Y = y)
        elementInternalOp(id, JsonConvert.SerializeObject(""))

    let setPlotLineAxesDecimalDigits (id: int) (x: float) (y: float) =
        //let jsonData = JsonSetAxesDecimalDigits(Op = "setAxesDecimalDigits", X = x, Y = y)
        elementInternalOp(id, JsonConvert.SerializeObject(""))

    let setAxisAutoFitEnabled (id: int) (enabled: bool) =
        //let jsonData = JsonSetAxesAutoFit(Op = "setAxesAutoFit", Enabled = enabled)
        elementInternalOp(id, JsonConvert.SerializeObject(""))

    let appendTextToClippedMultiLineTextRenderer (id: int) (text: string) =
        appendTextToClippedMultiLineTextRenderer(id, text)

    let setInputTextValue (id: int) (value: string) =
        //let jsonData = JsonSetValue(Op = "setValue", Value = value)
        elementInternalOp(id, JsonConvert.SerializeObject(""))

    let setComboSelectedIndex (id: int) (index: int) =
        //let jsonData = JsonSetSelectedIndex(Op = "setSelectedIndex", Index = index)
        elementInternalOp(id, JsonConvert.SerializeObject(""))