#I "tools/FAKE/tools"
#r "FakeLib.dll"

#load "tools/XamarinHelper.fsx"

#nowarn "20"

open System
open System.IO

open Fake
open Fake.FileUtils
open Fake.RestorePackageHelper

open Fake.XamarinHelper

Target "Restore" <| fun _ ->
    RestoreComponents "FieldService/FieldService.Xamarin.sln"
    RestorePackages ()

Target "Build" <| fun _ ->
    BuildiOSPackage "FieldService/FieldService.iOS/FieldService.iOS.csproj"

Target "Clean" <| fun _ ->
    CleanDirs [
        "FieldService/FieldService.iOS/bin"
        "FieldService/FieldService.iOS/obj"
    ]

"Clean"
    ==> "Restore"
    ==> "Build"

RunTargetOrDefault "Build"
