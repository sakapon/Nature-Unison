using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Markup;

// アセンブリに関する一般情報は以下の属性セットをとおして制御されます。
// アセンブリに関連付けられている情報を変更するには、
// これらの属性値を変更してください。
[assembly: AssemblyTitle("NatureUnison.Platforms.Kinect")]
[assembly: AssemblyDescription("The library for natural user interface.")]
[assembly: AssemblyMetadata("ProjectUrl", "https://github.com/sakapon/Nature-Unison")]
[assembly: AssemblyMetadata("LicenseUrl", "https://github.com/sakapon/Nature-Unison/blob/master/LICENSE")]
[assembly: AssemblyMetadata("Tags", "NUI Kinect")]
[assembly: AssemblyMetadata("ReleaseNotes", "")]

// ComVisible を false に設定すると、その型はこのアセンブリ内で COM コンポーネントから 
// 参照不可能になります。COM からこのアセンブリ内の型にアクセスする場合は、
// その型の ComVisible 属性を true に設定してください。
[assembly: ComVisible(false)]

// 次の GUID は、このプロジェクトが COM に公開される場合の、typelib の ID です
[assembly: Guid("86592911-e24d-4cb2-aaf9-ded968636181")]

[assembly: CLSCompliant(true)]
[assembly: XmlnsDefinition("http://schemas.saka-pon.net/natureunison", "NatureUnison.Platforms.Kinect")]
[assembly: XmlnsPrefix("http://schemas.saka-pon.net/natureunison", "nu")]
