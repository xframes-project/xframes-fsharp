﻿module Externs

open System
open System.Runtime.InteropServices

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

let marshalFloatArray (ptr: IntPtr) (length: int) : float[] =
    let managedArray = Array.zeroCreate<float> length
    Marshal.Copy(ptr, managedArray, 0, length)
    managedArray

type OnInitCb = unit -> unit
type OnTextChangedCb = delegate of int * string -> unit
type OnComboChangedCb = delegate of int * int -> unit
type OnNumericValueChangedCb = delegate of int * float -> unit
type OnBooleanValueChangedCb = delegate of int * bool -> unit
type OnMultipleNumericValuesChangedCb = delegate of int * IntPtr * int -> unit
type OnClickCb = delegate of int -> unit

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
