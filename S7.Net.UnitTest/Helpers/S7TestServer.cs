using System;
using Snap7;

namespace S7.Net.UnitTest.Helpers
{
    class S7TestServer
    {
        static S7Server Server;
        static private byte[] DB1 = new byte[1024];  // Our DB1
        static private byte[] DB2 = new byte[64000]; // Our DB2
        static private byte[] DB3 = new byte[1024]; // Our DB3
        static private byte[] DB4 = new byte[6] { 3, 128, 1, 0, 197, 104 }; // Our DB4

        private static S7Server.TSrvCallback TheEventCallBack; // <== Static var containig the callback
        private static S7Server.TSrvCallback TheReadCallBack; // <== Static var containig the callback

        // Here we use the callback to show the log, this is not the best choice since
        // the callback is synchronous with the client access, i.e. the server cannot
        // handle futher request from that client until the callback is complete.
        // The right choice is to use the log queue via the method PickEvent.

        static void EventCallback(IntPtr usrPtr, ref S7Server.USrvEvent Event, int Size)
        {
            Console.WriteLine(Server.EventText(ref Event));
        }

        static void ReadEventCallback(IntPtr usrPtr, ref S7Server.USrvEvent Event, int Size)
        {
            Console.WriteLine(Server.EventText(ref Event));
        }

        public static void Start(short port)
        {
            Server = new S7Server();
            // Share some resources with our virtual PLC
            Server.RegisterArea(S7Server.srvAreaDB,  // We are registering a DB
                                1,                   // Its number is 1 (DB1)
                                DB1,                 // Our buffer for DB1
                                DB1.Length);         // Its size
            // Do the same for DB2, DB3, and DB4
            Server.RegisterArea(S7Server.srvAreaDB, 2, DB2, DB2.Length);
            Server.RegisterArea(S7Server.srvAreaDB, 3, DB3, DB3.Length);
            Server.RegisterArea(S7Server.srvAreaDB, 4, DB4, DB4.Length);

            // Exclude read event to avoid the double report
            // Set the callbacks (using the static var to avoid the garbage collect)
            TheEventCallBack = new S7Server.TSrvCallback(EventCallback);
            TheReadCallBack = new S7Server.TSrvCallback(ReadEventCallback);

            Server.EventMask = ~S7Server.evcDataRead;
            Server.SetEventsCallBack(TheEventCallBack, IntPtr.Zero);
            Server.SetReadEventsCallBack(TheReadCallBack, IntPtr.Zero);

            // Uncomment next line if you don't want to see wrapped messages 
            // (Note : Doesn't work in Mono 2.10)

            // Console.SetBufferSize(100, Int16.MaxValue - 1);

            // Start the server onto the default adapter.
            // To select an adapter we have to use Server->StartTo("192.168.x.y").
            // Start() is the same of StartTo("0.0.0.0");       

            Server.SetParam(S7Consts.p_u16_LocalPort, ref port);

            int Error = Server.Start();
            if (Error != 0)
            {
                throw new Exception($"Error starting Snap7 server: {Server.ErrorText(Error)}");
            }
            //if (Error == 0)
            //{
            //    // Now the server is running ... wait a key to terminate
            //    //Console.ReadKey();
            //    Server.Stop();
            //}
            //else
            //    Console.WriteLine(Server.ErrorText(Error));
            // If you got a start error:
            // Windows - most likely you ar running the server in a pc on wich is
            //           installed step 7 : open a command prompt and type
            //             "net stop s7oiehsx"    (Win32) or
            //             "net stop s7oiehsx64"  (Win64)
            //           And after this test :
            //             "net start s7oiehsx"   (Win32) or
            //             "net start s7oiehsx64" (Win64)
            // Unix - you need root rights :-( because the isotcp port (102) is
            //        low and so it's considered "privileged".
        }

        public static void Stop() 
        {
            int Error = Server.Stop();
        }
    }
}
