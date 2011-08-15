@echo off
rm -r -v CSharpBot/bin
rm -r -v CSharpBot/obj
rm -r -v CSBConfig/bin
rm -r -v CSBConfig/obj
rm -r -v "CSharpBot Script Compiler/bin"
rm -r -v "CSharpBot Script Compiler/obj"
rm -r -v "SimpleLiveClient/bin"
rm -r -v "SimpleLiveClient/obj"
rem rm -r -v "WizardControls/bin" ; DO NOT DELETE since it is needed by VS to generate the controls from inside the designer
rm -r -v "WizardControls/obj"
rm -r -v "CSharpBot Installer/CSharpBot Installer/Client"
rm -r -v "CSharpBot Installer/CSharpBot Installer/Compiler"
rm -r -v "CSharpBot Installer/CSharpBot Installer/CSharpBot"
rm -r -v "CSharpBot Installer/CSharpBot Installer/Debug"
rm -r -v "CSharpBot Installer/CSharpBot Installer/Installer"
rm -r -v "CSharpBot Installer/CSharpBot Installer/LiveClient-Debug"
rm -r -v "CSharpBot Installer/CSharpBot Installer/Release"