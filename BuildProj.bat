@echo off

rem echo Test this.

rem /p:Configuration=Release

rem Command line options:
rem https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference

rem Common Properties:
rem https://docs.microsoft.com/en-us/visualstudio/msbuild/common-msbuild-project-properties

rem MSBuild Reference:
rem https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-reference

rem MSBuild in Wikipedia:
rem https://en.wikipedia.org/wiki/MSBuild

rem MSBuild Task Reference:
rem https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-task-reference

rem Visual C++ Tasks:
rem https://msdn.microsoft.com/en-us/library/ff960151.aspx

rem /toolsversion:14.0 ??

rem This should already be running in the
rem working directory this batch file is in.
rem cd c:\Eric\ClimateModel

rem @File
rem Insert command line switches from a file.

rem Verbosity in the build log.
rem q[uiet], m[inimal], n[ormal],
rem d[etailed], and diag[nostic].
rem /verbosity:quiet
rem /verbosity:minimal
 
c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe  /nologo /p:Configuration=Release /p:DebugSymbols=false /p:DefineDebug=false /p:DefineTrace=false /fileLogger /verbosity:normal ClimateModel.csproj

rem Help:
rem c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe  /help

pause
