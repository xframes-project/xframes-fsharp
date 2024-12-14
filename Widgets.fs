module Widgets

open System.Reactive.Subjects
open Types

let createNode id type' props children =
    {
        Id = id
        Type = type'
        Props = props
        Children = children
    }

