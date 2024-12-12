open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Runtime.InteropServices
open Newtonsoft.Json

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void resizeWindow(int width, int height)

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void setElement(string elementJson)

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void patchElement(int id, string elementJson)

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void elementInternalOp(int id, string elementJson)

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void setChildren(int id, string childrenIds)

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void appendChild(int parentId, int childId)

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern IntPtr getChildren(int id)

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void appendTextToClippedMultiLineTextRenderer(int id, string data)

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern IntPtr getStyle()

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void patchStyle(string styleDef)

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void setDebug(bool debug)

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void showDebugWindow()

//// Define a managed function that matches the callback signature
//let onTextChangedHandler (id: int, value: string) =
//    printfn "Text changed! ID: %d, Value: %s" id value

//// Create a delegate using Marshal
//let onTextChangedDelegate = 
//    Marshal.GetFunctionPointerForDelegate(Action<int, string>(fun id value -> onTextChangedHandler(id, value)))

type OnInitCb = unit -> unit
//type OnTextChangedCb = int * string -> unit
type OnComboChangedCb = int * int -> unit
type OnNumericValueChangedCb = int * float -> unit
type OnBooleanValueChangedCb = int * bool -> unit
type OnMultipleNumericValuesChangedCb = int * float[] -> unit
type OnClickCb = int -> unit

[<DllImport("xframesshared.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void init(
    string assetsBasePath,
    string rawFontDefinitions,
    string rawStyleOverrideDefinitions,
    IntPtr onInit,
    IntPtr onTextChanged,
    IntPtr onComboChanged,
    IntPtr onNumericValueChanged,
    IntPtr onBooleanValueChanged,
    IntPtr onMultipleNumericValuesChanged,
    IntPtr onClick
)

// Define a non-generic delegate matching the callback signature
type OnTextChangedCb = delegate of int * string -> unit

// Create an instance of the delegate
let onTextChangedDelegate = OnTextChangedCb(fun id value -> printfn "Text changed: %d, %s" id value)



type ImGuiCol =
    | Text = 0
    | TextDisabled = 1
    | WindowBg = 2
    | ChildBg = 3
    | PopupBg = 4
    | Border = 5
    | BorderShadow = 6
    | FrameBg = 7
    | FrameBgHovered = 8
    | FrameBgActive = 9
    | TitleBg = 10
    | TitleBgActive = 11
    | TitleBgCollapsed = 12
    | MenuBarBg = 13
    | ScrollbarBg = 14
    | ScrollbarGrab = 15
    | ScrollbarGrabHovered = 16
    | ScrollbarGrabActive = 17
    | CheckMark = 18
    | SliderGrab = 19
    | SliderGrabActive = 20
    | Button = 21
    | ButtonHovered = 22
    | ButtonActive = 23
    | Header = 24
    | HeaderHovered = 25
    | HeaderActive = 26
    | Separator = 27
    | SeparatorHovered = 28
    | SeparatorActive = 29
    | ResizeGrip = 30
    | ResizeGripHovered = 31
    | ResizeGripActive = 32
    | Tab = 33
    | TabHovered = 34
    | TabActive = 35
    | TabUnfocused = 36
    | TabUnfocusedActive = 37
    | PlotLines = 38
    | PlotLinesHovered = 39
    | PlotHistogram = 40
    | PlotHistogramHovered = 41
    | TableHeaderBg = 42
    | TableBorderStrong = 43
    | TableBorderLight = 44
    | TableRowBg = 45
    | TableRowBgAlt = 46
    | TextSelectedBg = 47
    | DragDropTarget = 48
    | NavHighlight = 49
    | NavWindowingHighlight = 50
    | NavWindowingDimBg = 51
    | ModalWindowDimBg = 52
    | COUNT = 53


let createColorDef (hex: string) (opacity: float) : obj list = 
    [hex; opacity]


type Theme2(colorsDict: Dictionary<int, List<obj>>) =
    member val colors = colorsDict with get, set


type FontDef = {
    name: string
    size: int
}

type Widget = {
    Id: int
    Type: string
    mutable Props: Map<string, obj>
    mutable Children: Widget list
}


let createNode id type' props children =
    {
        Id = id
        Type = type'
        Props = props
        Children = children
    }

let fontDefs =
    let fontSizes = [16; 18; 20; 24; 28; 32; 36; 48]
    let fontName = "roboto-regular"
    fontSizes
    |> List.map (fun size -> { name = fontName; size = size })

let fontsDictionary = new Dictionary<string, FontDef list>()
fontsDictionary.Add("defs", fontDefs)

let fontDefsJson = JsonConvert.SerializeObject(fontsDictionary)

    
let theme2Colors = 
    [
        "white", "#FFFFFF"
        "lighterGrey", "#B0B0B0"
        "black", "#000000"
        "lightGrey", "#A0A0A0"
        "darkestGrey", "#1A1A1A"
        "darkerGrey", "#505050"
        "darkGrey", "#2E2E2E"
    ] |> Map.ofList

let colorsDict = 
    let dict = new Dictionary<int, obj list>()
    
    dict.Add(int ImGuiCol.Text, [theme2Colors.["white"]; 1])
    dict.Add(int ImGuiCol.TextDisabled, [theme2Colors.["lighterGrey"]; 1])
    dict.Add(int ImGuiCol.WindowBg, [theme2Colors.["black"]; 1])
    dict.Add(int ImGuiCol.ChildBg, [theme2Colors.["black"]; 1])
    dict.Add(int ImGuiCol.PopupBg, [theme2Colors.["white"]; 1])
    dict.Add(int ImGuiCol.Border, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.BorderShadow, [theme2Colors.["darkestGrey"]; 1])
    dict.Add(int ImGuiCol.FrameBg, [theme2Colors.["black"]; 1])
    dict.Add(int ImGuiCol.FrameBgHovered, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.FrameBgActive, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.TitleBg, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.TitleBgActive, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.TitleBgCollapsed, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.MenuBarBg, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.ScrollbarBg, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.ScrollbarGrab, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.ScrollbarGrabHovered, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.ScrollbarGrabActive, [theme2Colors.["darkestGrey"]; 1])
    dict.Add(int ImGuiCol.CheckMark, [theme2Colors.["darkestGrey"]; 1])
    dict.Add(int ImGuiCol.SliderGrab, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.SliderGrabActive, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.Button, [theme2Colors.["black"]; 1])
    dict.Add(int ImGuiCol.ButtonHovered, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.ButtonActive, [theme2Colors.["black"]; 1])
    dict.Add(int ImGuiCol.Header, [theme2Colors.["black"]; 1])
    dict.Add(int ImGuiCol.HeaderHovered, [theme2Colors.["black"]; 1])
    dict.Add(int ImGuiCol.HeaderActive, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.Separator, [theme2Colors.["darkestGrey"]; 1])
    dict.Add(int ImGuiCol.SeparatorHovered, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.SeparatorActive, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.ResizeGrip, [theme2Colors.["black"]; 1])
    dict.Add(int ImGuiCol.ResizeGripHovered, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.ResizeGripActive, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.Tab, [theme2Colors.["black"]; 1])
    dict.Add(int ImGuiCol.TabHovered, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.TabActive, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.TabUnfocused, [theme2Colors.["black"]; 1])
    dict.Add(int ImGuiCol.TabUnfocusedActive, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.PlotLines, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.PlotLinesHovered, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.PlotHistogram, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.PlotHistogramHovered, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.TableHeaderBg, [theme2Colors.["black"]; 1])
    dict.Add(int ImGuiCol.TableBorderStrong, [theme2Colors.["lightGrey"]; 1])
    dict.Add(int ImGuiCol.TableBorderLight, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.TableRowBg, [theme2Colors.["darkGrey"]; 1])
    dict.Add(int ImGuiCol.TableRowBgAlt, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.TextSelectedBg, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.DragDropTarget, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.NavHighlight, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.NavWindowingHighlight, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.NavWindowingDimBg, [theme2Colors.["darkerGrey"]; 1])
    dict.Add(int ImGuiCol.ModalWindowDimBg, [theme2Colors.["darkerGrey"]; 1])
    
    dict


let themeJson = JsonConvert.SerializeObject(colorsDict)

let rec keepProcessRunning () =
    async {
        let mutable flag = true

        while flag do
            // Simulate some periodic work here if needed
            // printfn "Process is running..."

            // Delay for 1000ms (1 second) without blocking the thread
            do! Task.Delay(1000) |> Async.AwaitTask
    }

[<EntryPoint>]
let main argv =
    
    let buttonWidget = {
        Id = 1
        Type = "Button"
        Props = Map.ofList [("text", "Click Me")]
        Children = []
    }

    let labelWidget = {
        Id = 2
        Type = "Label"
        Props = Map.ofList [("text", "Hello World")]
        Children = []
    }

    let nodeWidget = createNode 3 "Node" (Map.ofList [("style", "vertical")]) [buttonWidget; labelWidget]


    printfn "%s" fontDefsJson

    // Example call to init (using some mock values)
    let assetsPath = "./assets"

    // Function that will contain the logic for initializing nodes
    let onInitLogic () =
        // Root node definition
        let rootNode = 
            let dict = Dictionary<string, obj>()
            dict.Add("id", 0 :> obj)
            dict.Add("type", "node" :> obj)
            dict.Add("root", true :> obj)
            dict

        // Text node definition
        let textNode = 
            let dict = Dictionary<string, obj>()
            dict.Add("id", 1 :> obj)
            dict.Add("type", "unformatted-text" :> obj)
            dict.Add("text", "Hello, world!" :> obj)
            dict

        // Serialize and set the elements
        setElement (JsonConvert.SerializeObject(rootNode))
        setElement (JsonConvert.SerializeObject(textNode))

        // Serialize and set the children
        setChildren (0, JsonConvert.SerializeObject([1]))
    

    let onInit = Marshal.GetFunctionPointerForDelegate(Action(fun () -> onInitLogic()))
    
    let onTextChangedPtr = Marshal.GetFunctionPointerForDelegate(onTextChangedDelegate)

    let onComboChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onNumericValueChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onBooleanValueChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onMultipleNumericValuesChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onClick = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))

    init(assetsPath, fontDefsJson, themeJson, onInit, onTextChangedPtr, onComboChanged, onNumericValueChanged, onBooleanValueChanged, onMultipleNumericValuesChanged, onClick)

    let process = keepProcessRunning ()
    Async.Start process
    
    // Prevent the app from terminating by waiting indefinitely
    Console.ReadLine() |> ignore
    0 // return an integer exit code
