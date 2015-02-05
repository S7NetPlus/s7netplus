# s7netplus
#### A .NET Library for Siemens S7 Connectivity

## Overview

S7.Net Plus is a continuation of the work done on the [S7.Net](http://s7net.codeplex.com/) project by [Juergen1969](http://www.codeplex.com/site/users/view/juergen1969).
I found the library simple and effective, but the project has languished unchanged since late 2009.

I was doing some automation work already and saw a few places where the code base could be improved. Because Juergen did not respond
to my request for committing code, I decided to pick up where he left off here on GitHub.

## Requirements

+ Compatible S7 PLC (S7-200, S7-300, S7-400, S7-1200)
+ .NET Framework 3.5 or higher

## Basic Usage

```C#
//Basic connection properties
string deviceIpAddress = "172.25.116.87";
int rackNumber = 0;
int slotNumber = 2;

//Connection to device
using (var plc = new PLC(CPU_Type.S7300, deviceIpAddress, rackNumber, slotNumber))
{
	//Ensure IP is responding
    if (plc.IsAvailable)
    {
        ErrorCode connectionResult = plc.Open();

		 //Verify that connection was successful
        if (connectionResult.Equals(ErrorCode.NoError))
        {
            //Get data
            object data = plc.Read("MB59");

            Console.WriteLine("SUCCESS: Read result of MB59 is {0}", data);
        }
        else
        {
            Console.WriteLine("ERROR: Device is available but connection was unsuccessful!");
        }
    }
    else
    {
        Console.WriteLine("ERROR: Device is not available!");
    }
} 
```

## Running the tests

Unit tests use Snap7 server, so port 102 must be not in use.
If you have Siemens Step7 installed, the service s7oiehsx64 is stopped when running unit tests.
You have to restart the service manually if you need it.
