param($installPath, $toolsPath, $package, $project)

$LeapItem = $project.ProjectItems.Item("Leap.dll")
$LeapOutputProp = $LeapItem.Properties.Item("CopyToOutputDirectory")
$LeapOutputProp.Value = 2

$LeapCSharpItem = $project.ProjectItems.Item("LeapCSharp.dll")
$LeapCSharpOutputProp = $LeapCSharpItem.Properties.Item("CopyToOutputDirectory")
$LeapCSharpOutputProp.Value = 2
