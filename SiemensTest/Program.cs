using S7.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SiemensTest
{
    class Program
    {
        private static object locked = new object();
        private static CpuType cpuType = CpuType.S71200;
        private static string pclIpAddress = "172.20.1.30";
        private static Int16 rack = 0;
        private static Int16 slot = 1;
        private static bool close = false;
        private static Plc plcOnDemand;
        private static Plc plcMonitor;
        private static Plc plcEncoder;
        private static Stopwatch stopwatch = new Stopwatch();
        private static byte[] cards = new byte[3];
        private static Dictionary<int, BitArray> cardBits = new Dictionary<int, BitArray>();

        static void Main(string[] args)
        {
            DataType dtype = DataType.DataBlock;
            //int db = 0;
            //int stringDb = 2;
            //int startByteAddr = 0;
            int lineIndex = 1;

            if (ConnectAll() == false)
            {
                Console.WriteLine($"This is where we would have to begin retry attempts.");
                Console.ReadLine();
                return;
            }

            Task.Run(() => 
            {
                RuntimeOperations rto = new RuntimeOperations();
                RuntimeOperations prevRto = new RuntimeOperations();
                int counter = 0;
                int pctr = -1;
                rto.Run = false;
                rto.SpliceInterrupt = false;
                rto.LineSpeed = 0;
                rto.EncoderCounter = 0;
                plcMonitor.WriteClass(rto, lineIndex, 0);
                try
                {
                    while (!close)
                    {
                        //plcMonitor.ReadClass(rto, lineIndex, 0);
                        counter = S7.Net.Types.DInt.FromByteArray(plcMonitor.ReadBytes(DataType.DataBlock, lineIndex, 2, 4));
                        if (counter != pctr)
                        {
                            pctr = counter;
                            Console.WriteLine($"{DateTime.Now:HH:mm:ss.ffffff} Counter:{counter}");
                        }
                        //System.Threading.Thread.Sleep(5);                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in Read Thread. {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            });

            System.Threading.Thread.Sleep(100);

            Task.Run(() =>
            {
                bool run = false;
                float encoder = 0.0F;
                RuntimeOperations rto = new RuntimeOperations();
                plcEncoder.ReadClass(rto, lineIndex, 0);
                try
                {
                    //lock (locked)
                    //{
                        plcEncoder.WriteBit(DataType.DataBlock, lineIndex, 0, 0, true);
                    //}
                    while (!close)
                    {
                        //lock (locked)
                        //{
                            plcEncoder.WriteBytes(DataType.DataBlock, lineIndex, 6, S7.Net.Types.Single.ToByteArray(encoder += 1.1F));
                        //}
                        System.Threading.Thread.Sleep(25);

                    }
                    //lock (locked)
                    //{
                        plcEncoder.WriteBit(DataType.DataBlock, lineIndex, 0, 0, false);
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in Read Thread. {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            });

            System.Threading.Thread.Sleep(5000);
            // Class read/write

            Inputs myReadClass = new Inputs();
            Inputs myInputClass = new Inputs();
            RecipeInfoMessage recipe = new RecipeInfoMessage();

            double d = 1.1;

            float f = 1.1f;

            float c = (float)d;

            double e = (double)(decimal)f;

            myReadClass.RuntimeInputs.Run = true;
            myReadClass.RuntimeInputs.LineSpeed = 600;
            myReadClass.SpecOverride.TopOfGraph = 90.2F;
            myReadClass.SpecOverride.UpperSpecLimit = 52.2F;
            myReadClass.SpecOverride.TargetSpec = 50.1F;
            myReadClass.SpecOverride.LowerSpecLimit = 48.2F;
            myReadClass.SpecOverride.BottomOfGraph = 20.2F;
            myReadClass.LaneOverride.WebWidth = 1000F;
            myReadClass.LaneOverride.EdgeTrim = 25.0F;
            myReadClass.LaneOverride.LaneCount = 5;
            myReadClass.LaneOverride.IndependentEdgerimLanes = true;


            //Console.WriteLine($"OnDemand Read: {myInputClass}");
            //plc.WriteClass(myReadClass, lineIndex, 0);
            plcOnDemand.ReadClass(myInputClass, lineIndex, 0);


            recipe.GraphSpec.TopOfGraph = 19.2F;
            recipe.GraphSpec.UpperSpecLimit = 16.2F;
            recipe.GraphSpec.TargetSpec = 15.1F;
            recipe.GraphSpec.LowerSpecLimit = 14.2F;
            recipe.GraphSpec.BottomOfGraph = 12.2F;
            recipe.ConversionRate = 10.76F;
            recipe.ThicknessMax = 22.46F;
            recipe.ThicknessMin = 18.8593F;
            recipe.Ior = 1.4F;
            recipe.StartWave = 1100F;
            recipe.EndWave = 1550F;
            recipe.LayerName = "NIST-124254_83";
            recipe.UnitOfMeasure = "microns";

            stopwatch.Start();

            RecipeInfo recipe2 = new RecipeInfo(recipe);

            Console.WriteLine($"OnDemand Write: {recipe2}");
            //lock (locked)
            //{
                plcOnDemand.WriteClass(recipe2, lineIndex, 1106);
            //}

            RecipeInfo recipe3 = new RecipeInfo();
            Console.WriteLine($"OnDemand Read Verification: {recipe3}");
            plcOnDemand.ReadClass(recipe3, lineIndex, 1106);

            RecipeInfoMessage newRecipe = new RecipeInfoMessage(recipe2);

            stopwatch.Stop();
            Console.WriteLine($"New Recipe: {newRecipe.LayerName}, {newRecipe.UnitOfMeasure}, {newRecipe.Ior}... done in {stopwatch.ElapsedMilliseconds}");


            double val = 0.0F;
            ProbeData myWriteClass = new ProbeData();
                val += 1.1;
                myWriteClass.Measurement = val;
                myWriteClass.Microns = val;
                myWriteClass.IntegrationTime = val;
                myWriteClass.DistanceToTarget = val;
                myWriteClass.CrossWebPosition = val;
                myWriteClass.ReadStatus = 0;
                myWriteClass.CycleAverage = val;
                myWriteClass.CycleAverageMicrons = val;
                myWriteClass.CycleMin = val;
                myWriteClass.CycleMax = val;
                myWriteClass.CycleStdDev = val;
                myWriteClass.CycleTotalCount = 80;
                myWriteClass.CycleEvalCount = 80;
                myWriteClass.LastUpdated = DateTime.Now;


            DateTime stopAt = DateTime.Now.AddSeconds(30);

            while (DateTime.Now < stopAt)
            {
                Console.WriteLine($"Writing to Probe1");
                WriteProbeData(IncValues(myWriteClass), lineIndex, 2744);
                //plcOnDemand.WriteClass(myWriteClass, lineIndex, 2748);
                System.Threading.Thread.Sleep(1000);

                Console.WriteLine($"Writing to Probe2");
                WriteProbeData(IncValues(myWriteClass), lineIndex, 2806);
                //plcOnDemand.WriteClass(myWriteClass, lineIndex, 2810);
                System.Threading.Thread.Sleep(1000);

                Console.WriteLine($"Writing to Probe3");
                WriteProbeData(IncValues(myWriteClass), lineIndex, 2868);
                //plcOnDemand.WriteClass(myWriteClass, lineIndex, 2872);
                System.Threading.Thread.Sleep(1000);

                Console.WriteLine($"Writing to Probe4");
                WriteProbeData(IncValues(myWriteClass), lineIndex, 2930);
                //plcOnDemand.WriteClass(myWriteClass, lineIndex, 2934);
                System.Threading.Thread.Sleep(1000);

                Console.WriteLine($"Writing to Probe5");
                WriteProbeData(IncValues(myWriteClass), lineIndex, 2992);
                //plcOnDemand.WriteClass(myWriteClass, lineIndex, 2934);
                System.Threading.Thread.Sleep(1000);

                Console.WriteLine($"Writing to Probe6");
                WriteProbeData(IncValues(myWriteClass), lineIndex, 3054);
                //plcOnDemand.WriteClass(myWriteClass, lineIndex, 2934);
                System.Threading.Thread.Sleep(1000);
            }

            System.Threading.Thread.Sleep(1000);
            close = true;

            System.Threading.Thread.Sleep(100);


            //DTS myWriteClass = new DTS();
            //DTS myReadClass = new DTS();
            //plc.ReadClass(myReadClass, 2, 0);

            //myWriteClass.LastUpdated = DateTime.Now;

            //plc.WriteClass(myWriteClass, 2, 0);

            //Console.WriteLine($"ReadClass DTS {myReadClass.LastUpdated}");


            // End of Class update

            #region old tests

            /*

            int cardcount = 3;
            int cardidx = 0;
            short bitpos = 0;
            byte[] outval = new byte[cardcount];
            byte[] val = new byte[1];
            byte[] mdb = new byte[3];
            cardBits.Add(0, new BitArray(6));
            cardBits.Add(1, new BitArray(8));
            cardBits.Add(2, new BitArray(8));

            int maxCnt = 254;

            int count = 252;
            int stringCount = 0;

            try
            {
                cards[0] = 0;
                cards[1] = 0;
                cards[2] = 1;
                // Establish connection to PLC

                BitArraytoByteArray();
                plc.WriteBytes(DataType.Output, 0, 0, cards);
                System.Threading.Thread.Sleep(100);

                Console.WriteLine($"Starting now.");
                stopwatch.Start();

                cardBits[0].Set(1, true);

                cardBits[0].Set(2, true);

                BitArraytoByteArray();
                //plc.WriteBytes(DataType.Output, 0, 0, cards);

                cardBits[1].Set(0, false);
                cardBits[1].Set(1, false);

                BitArraytoByteArray();
                cardidx = 1;
                val[0] = cards[1];
                plc.WriteBytes(DataType.Output, 0, cardidx, val);

                cardBits[1].Set(0, true);
                cardBits[1].Set(1, false);

                BitArraytoByteArray();
                cardidx = 1;
                val[0] = cards[1];
                plc.WriteBytes(DataType.Output, 0, cardidx, val);


                cardBits[1].Set(0, false);
                cardBits[1].Set(1, true);

                BitArraytoByteArray();
                cardidx = 1;
                val[0] = cards[1];
                plc.WriteBytes(DataType.Output, 0, cardidx, val);


                cardBits[1].Set(0, true);
                cardBits[1].Set(1, true);

                BitArraytoByteArray();
                cardidx = 1;
                val[0] = cards[1];
                plc.WriteBytes(DataType.Output, 0, cardidx, val);


                cardBits[2].Set(0, true);
                cardBits[2].Set(1, false);
                cardBits[2].Set(2, true);
                cardBits[2].Set(3, true);

                BitArraytoByteArray();

                cardidx = 0;
                val[0] = cards[0];
                plc.WriteBytes(DataType.Output, 0, cardidx, val);

                cardidx = 1;
                val[0] = cards[1];
                plc.WriteBytes(DataType.Output, 0, cardidx, val);

                cardidx = 2;
                val[0] = cards[2];
                plc.WriteBytes(DataType.Output, 0, cardidx, val);

                cardidx = 0;
                bitpos = 0;
                val[0] = Convert.ToByte(getIntFromBitArray(cardBits[cardidx]));
                plc.WriteBytes(DataType.Output, 0, cardidx, val);
                //_ = WriteBitAsync(cardidx, bitpos, cardBits[cardidx].Get(bitpos));
                //Console.WriteLine($"Finished (Card: {cardidx} Bit: {1} to {cardBits[cardidx].Get(1)}): took {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");

                //bitpos = 1;
                //_ = WriteBitAsync(cardidx, bitpos, cardBits[cardidx].Get(bitpos));
                //Console.WriteLine($"Finished (Card: {cardidx} Bit: {1} to {cardBits[cardidx].Get(1)}): took {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");

                cardidx = 1;
                bitpos = 1;
                val[0] = Convert.ToByte(getIntFromBitArray(cardBits[cardidx]));
                plc.Write(DataType.Output, 0, cardidx, val);
                //_ = WriteBitAsync(cardidx, bitpos, cardBits[cardidx].Get(bitpos));
                //Console.WriteLine($"Finished (Card: {cardidx} Bit: {1} to {cardBits[cardidx].Get(1)}): took {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");

                cardidx = 2;
                bitpos = 1;
                val[0] = Convert.ToByte(getIntFromBitArray(cardBits[cardidx]));
                plc.Write(DataType.Output, 0, cardidx, val);
                //_ = WriteBitAsync(cardidx, bitpos, cardBits[cardidx].Get(bitpos));
                //Console.WriteLine($"Finished (Card: {cardidx} Bit: {1} to {cardBits[cardidx].Get(1)}): took {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");

                System.Threading.Thread.Sleep(500);

                cardBits[0].Set(4, true);
                cardBits[0].Set(5, true);

                cardBits[1].Set(6, true);
                cardBits[2].Set(7, true);

                cardidx = 0;
                bitpos = 0;
                val[0] = Convert.ToByte(getIntFromBitArray(cardBits[cardidx]));
                plc.WriteBytes(DataType.Output, 0, cardidx, val);
                //_ = WriteBitAsync(cardidx, bitpos, cardBits[cardidx].Get(bitpos));
                //Console.WriteLine($"Finished (Card: {cardidx} Bit: {1} to {cardBits[cardidx].Get(1)}): took {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");

                //bitpos = 1;
                //_ = WriteBitAsync(cardidx, bitpos, cardBits[cardidx].Get(bitpos));
                //Console.WriteLine($"Finished (Card: {cardidx} Bit: {1} to {cardBits[cardidx].Get(1)}): took {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");

                cardidx = 1;
                bitpos = 1;
                val[0] = Convert.ToByte(getIntFromBitArray(cardBits[cardidx]));
                plc.Write(DataType.Output, 0, cardidx, val);
                //_ = WriteBitAsync(cardidx, bitpos, cardBits[cardidx].Get(bitpos));
                //Console.WriteLine($"Finished (Card: {cardidx} Bit: {1} to {cardBits[cardidx].Get(1)}): took {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");

                cardidx = 2;
                bitpos = 1;
                val[0] = Convert.ToByte(getIntFromBitArray(cardBits[cardidx]));
                plc.Write(DataType.Output, 0, cardidx, val);

                #region

                //while (count < maxCnt)
                //{

                    ////for (int i = 0; i < cardcount; i++)
                    ////{
                    ////    cards[i] = plc.ReadBytes(DataType.Output, 0, 0, 1);

                    ////}

                    //cards = plc.ReadBytes(DataType.Output, 0, 0, 3);

                    //int c = 0;
                    //foreach (var card in cards)
                    //{
                    //    Console.WriteLine($"Card{c++} = {card}");
                    //}

                    count++;
                    outval[0] = Convert.ToByte(count);
                    //outval[1] = Convert.ToByte(count * 2);
                    //outval[2] = Convert.ToByte(count * 3);

                    //plc.WriteBytes(DataType.Output, 0, 0, outval);

                    //BitArray outBits = new BitArray(new int[] { count });

                    //for (int i = 0; i < 6; i++)
                    //{
                    //    plc.WriteBit(DataType.Output, 0, cardidx, i, outBits[i]);
                    //    Console.WriteLine($"WriteIO (Card: {cardidx} Bit: {i} to {outBits[i]}): took {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");
                    //    plc.WriteBit(DataType.Output, 0, cardidx + 1, i, outBits[i]);
                    //    plc.WriteBit(DataType.Output, 0, cardidx + 2, i, outBits[i]);
                    //}
                    System.Threading.Thread.Sleep(100);

                    #region Verified working

                    //  This stuff works


                    switch (done)
                    {
                        case "R":
                        case "r":
                            stringCount = 0;
                            break;
                        case "1":
                            stringCount = 1;
                            break;
                        case "2":
                            stringCount = 2;
                            break;
                        case "3":
                            stringCount = 3;
                            break;
                        case "4":
                            stringCount = 4;
                            break;
                        default:
                            break;
                    }

                    if (stringCount >= 4)
                        stringCount = 0;

                    stringCount++;

                    int idx = 0;

                    //byte myByte = (byte)plc?.Read("M0.0"); //0-6
                    //Console.WriteLine($"Read(DB1.DBX0.0) : {myByte}");

                    mdb = plc.ReadBytes(DataType.DataBlock, 1, 0, 3);
                    idx = 0;
                    foreach (var b in mdb)
                    {
                        idx++;
                        Console.WriteLine($"DB1 Idx{idx} : {b}");
                        for (int i = 0; i < 8; i++)
                        {
                            Console.WriteLine($"DB1 Idx{idx} bit:{i} : {b.SelectBit(i)}");
                        }
                    }

                    //plc.WriteBytes(DataType.DataBlock, 1, 0, 1);

                    var mi = plc.ReadBytes(DataType.Input, 0, 0, 1);
                    idx = 0;
                    foreach (var i in mi)
                    {
                        Console.WriteLine($"Input Idx{idx++} : {i}");
                    }

                    var mo = plc.ReadBytes(DataType.Output, 0, 0, 3);
                    idx = 0;
                    foreach (var o in mo)
                    {
                        Console.WriteLine($"Output Idx{idx++} : {o}");
                    }


                    DateTime timestamp = DateTime.Now;

                    string stringToPlc = $"{timestamp:yyyy-MM-dd HH:mm:ss}";

                    DataType dataType = DataType.DataBlock;
                    db = 1;
                    stringDb = 1;
                    startByteAddr = 0;

                    int size = GetMaxStringSize(dataType, db, startByteAddr);
                    int length = GetActualStringSize(dataType, db, startByteAddr);

                    var stringGroup = plc.ReadBytes(dataType, db, startByteAddr, (size + 2) * stringCount);

                    stopwatch.Stop();
                    Console.WriteLine($"ReadBytes: {(size + 2) * stringCount} took {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");
                    stopwatch.Restart();

                    byte[] recipeArray = new byte[256];
                    byte[] trackingIdArray = new byte[256];
                    byte[] basecoatArray = new byte[256];
                    byte[] laneDefnArray = new byte[256];

                    Array.Copy(stringGroup, 0, recipeArray, 0, 256);
                    if (stringCount > 1)
                        Array.Copy(stringGroup, 256 * 1, trackingIdArray, 0, 256);
                    if (stringCount > 2)
                        Array.Copy(stringGroup, 256 * 2, basecoatArray, 0, 256);
                    if (stringCount > 3)
                        Array.Copy(stringGroup, 256 * 3, laneDefnArray, 0, 256);

                    string recipeName = S7.Net.Types.StringEx.FromByteArray(recipeArray);
                    string trackingId = S7.Net.Types.StringEx.FromByteArray(trackingIdArray);
                    string basecoat = S7.Net.Types.StringEx.FromByteArray(basecoatArray);
                    string laneDefn = S7.Net.Types.StringEx.FromByteArray(laneDefnArray);

                    stopwatch.Stop();
                    Console.WriteLine($"parse Bytes: {(size + 2) * stringCount} took {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");
                    stopwatch.Reset();

                    Console.WriteLine($"RecipeName = {recipeName}");
                    Console.WriteLine($"TrackingId = {trackingId}");
                    Console.WriteLine($"Basecoat   = {basecoat}");
                    Console.WriteLine($"Lane Defn  = {laneDefn}");

                    stringToPlc = "NIST 192-cl 24.5";
                    int newStrLen = stringToPlc.Length;

                    byte[] dataBytes = S7.Net.Types.String.ToByteArray(stringToPlc, size);

                    List<byte> values = new List<byte>();
                    values.Add((byte)size);
                    values.Add((byte)stringToPlc.Length);
                    values.AddRange(dataBytes);

                    plc.WriteBytes(dataType, stringDb, startByteAddr, values.ToArray());
                    //plc.WriteBytes(dataType, db, startByteAddr, values.ToArray());

                    size = GetMaxStringSize(dataType, db, startByteAddr);
                    length = GetActualStringSize(dataType, db, startByteAddr);
                    stopwatch.Start();

                    // Read all bytes of string including size/length bytes
                    byte[] rn = plc.ReadBytes(dataType, db, startByteAddr, size + 2);

                    stopwatch.Stop();
                    Console.WriteLine($"Write string: {stringToPlc} took {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");
                    stopwatch.Reset();

                    recipeName = S7.Net.Types.StringEx.FromByteArray(rn);
                    Console.WriteLine($"RecipeName = {recipeName}");

                    //

                    #endregion

                    //done = Console.ReadLine();
                //}
                Console.ReadLine();
                #endregion

            }
            catch (PlcException pex) when (pex.InnerException is System.Net.Sockets.SocketException)
            {
                Console.WriteLine($"{pex.Message}{Environment.NewLine}{Environment.NewLine}{pex.StackTrace}{Environment.NewLine}{Environment.NewLine}{pex.InnerException}");

                Exception baseEx = pex.GetBaseException();
                var code = ((System.Net.Sockets.SocketException)baseEx)?.SocketErrorCode;

            }
            catch (PlcException pex) when (pex.InnerException is System.IO.IOException)
            {
                Console.WriteLine($"{pex.Message}{Environment.NewLine}{Environment.NewLine}{pex.StackTrace}{Environment.NewLine}{Environment.NewLine}{pex.InnerException}");

                Exception baseEx = pex.GetBaseException();
                var code = ((System.Net.Sockets.SocketException)baseEx)?.SocketErrorCode;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{Environment.NewLine}{ex.InnerException}");
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"Total Time: {stopwatch.ElapsedMilliseconds} - {stopwatch.ElapsedTicks}");

                // Close connection to PLC
                plc.Close();
            }

            */

            #endregion

            Console.WriteLine("Process Complete!");
            Console.ReadLine();
        }

        private static bool ConnectAll()
        {
            try
            {
                plcOnDemand = new Plc(cpuType, pclIpAddress, rack, slot);
                plcOnDemand.Open();

                plcMonitor = new Plc(cpuType, pclIpAddress, rack, slot);
                plcMonitor.Open();

                plcEncoder = new Plc(cpuType, pclIpAddress, rack, slot);
                plcEncoder.Open();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to connect to PLC.");
            }
            return false;
        }

        private static IProbeData IncValues(ProbeData data)
        {
            data.Measurement += 1.1;
            data.Microns += 1.1;
            data.IntegrationTime += 1.1;
            data.DistanceToTarget += 1.1;
            data.CrossWebPosition += 1.1;
            data.ReadStatus = 0;
            data.CycleAverage += 1.1;
            data.CycleAverageMicrons += 1.1;
            data.CycleMin += 1.1;
            data.CycleMax += 1.1;
            data.CycleStdDev += 1.1;
            data.CycleTotalCount += 5;
            data.CycleEvalCount += 5;
            data.LastUpdated = DateTime.Now;
            return data;
        }

        private static void WriteProbeData(IProbeData myWriteClass, int lineIndex, int offset)
        {
            plcOnDemand.WriteClass(myWriteClass, lineIndex, offset);
        }

        private static int getIntFromBitArray(BitArray bitArray)
        {

            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];

        }

        private static void BitArraytoByteArray()
        {
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i] = Convert.ToByte(getIntFromBitArray(cardBits[i]));
            }
            
        }

        private static void WriteAllBytes(int cardidx, byte[] values)
        {
            plcOnDemand.WriteBytes(DataType.Output, 0, 0, values);
        }

        private static async Task WriteBitAsync(int cardidx, short bitPos, bool value)
        {
            await Task.Run(() =>
            {
                lock (locked)
                {
                    Console.WriteLine($"Writing Card({cardidx}).{bitPos} = {value}");
                    plcOnDemand.WriteBitAsync(DataType.Output, 0, cardidx, bitPos, value);
                }
            });
        }

        private static int GetMaxStringSize(DataType dataType, int db, int startByteAddr)
        {
            byte[] maxStringLenAsByte = plcOnDemand.ReadBytes(dataType, db, startByteAddr, 1);
            return Convert.ToInt32(maxStringLenAsByte[0]);
        }

        private static int GetActualStringSize(DataType dataType, int db, int startByteAddr)
        {
            byte[] actualStringLenAsByte = plcOnDemand.ReadBytes(dataType, db, startByteAddr + 1, 1);
            return Convert.ToInt32(actualStringLenAsByte[0]);
        }
    }
}
