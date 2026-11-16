using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Types;

namespace S7.Net.UnitTest.CommunicationTests;

[TestClass]
public class ReadTimeoutTests
{
    private static async Task RunBlackholeServerAsync(
        TcpListener listener,
        CancellationToken serverCt,
        bool replyToRead = false)
    {
        using var client = await listener.AcceptTcpClientAsync();
        using var stream = client.GetStream();
        var buf = new byte[1024];
        int msgCount = 0;

        while (!serverCt.IsCancellationRequested)
        {
            int n;
            try
            {
                n = await stream.ReadAsync(buf, 0, buf.Length, serverCt);
            }
            catch
            {
                break;
            }
            if (n <= 0) break;

            msgCount++;
            if (msgCount == 1)
            {
                byte[] cc =
                {
                    0x03, 0x00, 0x00, 0x16, 0x11, 0xd0, 0x00, 0x01, 0x00, 0x02, 0x00,
                    0xc0, 0x01, 0x0a, 0xc1, 0x02, 0x01, 0x00, 0xc2, 0x02, 0x01, 0x02
                };
                await stream.WriteAsync(cc, 0, cc.Length, serverCt);
            }
            else if (msgCount == 2)
            {
                byte[] s7setup =
                {
                    0x03, 0x00, 0x00, 0x1b, 0x02, 0xf0, 0x80,
                    0x32, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00,
                    0xf0, 0x00, 0x00, 0x01, 0x00, 0x01, 0x03, 0xc0
                };
                await stream.WriteAsync(s7setup, 0, s7setup.Length, serverCt);
            }
            else if (!replyToRead)
            {
                try
                {
                    await Task.Delay(Timeout.Infinite, serverCt);
                }
                catch
                {
                    break;
                }
            }
            else
            {
                byte[] readResp =
                {
                    0x03, 0x00, 0x00, 0x1b, 0x02, 0xf0, 0x80,
                    0x32, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x06, 0x00, 0x00,
                    0x04, 0x01,
                    0xff, 0x04, 0x00, 0x10, 0x00, 0x2a
                };
                await stream.WriteAsync(readResp, 0, readResp.Length, serverCt);
            }
        }
    }

    [TestMethod]
    public async Task ReadBytesAsync_TimesOut_WhenServerStopsResponding()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        using var serverCts = new CancellationTokenSource();
        var serverTask = RunBlackholeServerAsync(listener, serverCts.Token);

        try
        {
            using var plc = new Plc(CpuType.S71200, IPAddress.Loopback.ToString(), port, 0, 1)
            {
                ReadTimeout = 300,
                WriteTimeout = 300
            };

            using var lifeCts = new CancellationTokenSource();
            await plc.OpenAsync(lifeCts.Token);
            Assert.IsTrue(plc.IsConnected);

            var sw = Stopwatch.StartNew();
            var ex = await Assert.ThrowsExceptionAsync<PlcException>(() =>
                plc.ReadBytesAsync(DataType.DataBlock, 1, 0, 2, lifeCts.Token));
            sw.Stop();

            Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutException));
            Assert.IsFalse(plc.IsConnected);
            Assert.IsTrue(sw.ElapsedMilliseconds < 2000, $"Elapsed={sw.ElapsedMilliseconds}ms");
        }
        finally
        {
            serverCts.Cancel();
            listener.Stop();
            await Task.WhenAny(serverTask, Task.Delay(1000));
        }
    }

    [TestMethod]
    public async Task ReadMultipleVarsAsync_TimesOut_WhenServerStopsResponding()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        using var serverCts = new CancellationTokenSource();
        var serverTask = RunBlackholeServerAsync(listener, serverCts.Token);

        try
        {
            using var plc = new Plc(CpuType.S71200, IPAddress.Loopback.ToString(), port, 0, 1)
            {
                ReadTimeout = 300,
                WriteTimeout = 300
            };

            using var lifeCts = new CancellationTokenSource();
            await plc.OpenAsync(lifeCts.Token);

            var items = new List<DataItem>
            {
                new DataItem { DataType = DataType.DataBlock, DB = 1, StartByteAdr = 0, VarType = VarType.Word, Count = 1 }
            };

            var sw = Stopwatch.StartNew();
            var ex = await Assert.ThrowsExceptionAsync<PlcException>(() =>
                plc.ReadMultipleVarsAsync(items, lifeCts.Token));
            sw.Stop();

            Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutException));
            Assert.IsFalse(plc.IsConnected);
            Assert.IsTrue(sw.ElapsedMilliseconds < 2000, $"Elapsed={sw.ElapsedMilliseconds}ms");
        }
        finally
        {
            serverCts.Cancel();
            listener.Stop();
            await Task.WhenAny(serverTask, Task.Delay(1000));
        }
    }

    [TestMethod]
    public async Task ReadBytesAsync_ExternalCancellation_ThrowsOperationCanceledException()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        using var serverCts = new CancellationTokenSource();
        var serverTask = RunBlackholeServerAsync(listener, serverCts.Token);

        try
        {
            using var plc = new Plc(CpuType.S71200, IPAddress.Loopback.ToString(), port, 0, 1)
            {
                ReadTimeout = 5000,
                WriteTimeout = 5000
            };

            await plc.OpenAsync();

            using var callerCts = new CancellationTokenSource(200);
            try
            {
                await plc.ReadBytesAsync(DataType.DataBlock, 1, 0, 2, callerCts.Token);
                Assert.Fail("Expected OperationCanceledException");
            }
            catch (OperationCanceledException)
            {
                // Expected (includes TaskCanceledException)
            }
            Assert.IsFalse(plc.IsConnected);
        }
        finally
        {
            serverCts.Cancel();
            listener.Stop();
            await Task.WhenAny(serverTask, Task.Delay(1000));
        }
    }
}
