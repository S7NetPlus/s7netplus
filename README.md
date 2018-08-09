# s7netplus
#### A .NET Library for Siemens S7 Connectivity

## Overview

S7.Net Plus is a continuation of the work done on the [S7.Net](http://s7net.codeplex.com/) project by [Juergen1969](http://www.codeplex.com/site/users/view/juergen1969).
I found the library simple and effective, but the project has languished unchanged since late 2009.

I was doing some automation work already and saw a few places where the code base could be improved. Because Juergen did not respond
to my request for committing code, I decided to pick up where he left off here on GitHub.

## Documentation
Check the Wiki and feel free to edit it: https://github.com/killnine/s7netplus/wiki

S7.Net Plus has a [User Manual](https://github.com/killnine/s7netplus/blob/master/Documentation/Documentation.pdf), check it out.

## Supported PLC

+ Compatible S7 PLC (S7-200, S7-300, S7-400, S7-1200, S7-1500)

## Supported frameworks
+ .NET Framework 4.5.2 and higher
+ .NET Standard 1.3 (.NET Core 1.0, UWP 10.0, Xamarin, ...)
+ .NET Standard 2.0 (.NET Core 2.0, .NET Framework 4.6.1)

## Compile
You need at least Visual Studio 2017 (you can download the Community Edition for free).

## Nuget

PM> Install-Package S7netplus

## Latest build (Appveyor)
[![Build status](https://ci.appveyor.com/api/projects/status/ousjt8sn9b1w43p6?svg=true)](https://ci.appveyor.com/project/mesta1/s7netplus)
[https://ci.appveyor.com/project/mesta1/s7netplus](https://ci.appveyor.com/project/mesta1/s7netplus)

## Running the tests

Unit tests use Snap7 server, so port 102 must be not in use.
If you have Siemens Step7 installed, the service s7oiehsx64 is stopped when running unit tests.
You have to restart the service manually if you need it.
