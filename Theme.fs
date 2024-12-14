module Theme

open System.Collections.Generic
open Enums

let createColorDef (hex: string) (opacity: float) : obj list = 
    [hex; opacity]




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