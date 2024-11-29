using Thomsen.AccTools.SharedMemory;
using MyTrueGear;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Thomsen.AccTools.Tester;

internal class Program
{
    private static TrueGearMod _TrueGear = null;

    private static string _SteamExe;
    public const string STEAM_OPENURL = "steam://rungameid/805550";

    private const int SW_MINIMIZE = 6;
    private const int SW_RESTORE = 9;

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public static string SteamExePath()
    {
        return (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamExe", null);
    }
    private static async Task Main(string[] args)
    {
        //当有两个程序运行的时候，关闭前一个程序，保留当前程序
        string currentProcessName = Process.GetCurrentProcess().ProcessName;
        Process[] processes = Process.GetProcessesByName(currentProcessName);
        if (processes.Length > 1)
        {
            if (processes[0].UserProcessorTime.TotalMilliseconds > processes[1].UserProcessorTime.TotalMilliseconds)
            {
                processes[0].Kill();
            }
            else
            {
                processes[1].Kill();
            }
        }

        // 获取当前 Console 窗口句柄
        IntPtr hWnd = GetConsoleWindow();

        if (hWnd != IntPtr.Zero)
        {
            // 最小化窗口
            ShowWindow(hWnd, SW_MINIMIZE);
        }

        _SteamExe = SteamExePath();
        if (_SteamExe != null) Process.Start(_SteamExe, STEAM_OPENURL);

        Thread.Sleep(500);

        CancellationTokenSource cts = new();

        // Initialize the API
        using AccSharedMemory acc = new();


        _TrueGear = new TrueGearMod();

        //获取间隔调节（ms）
        acc.PhysicsUpdateInterval = 50;


        // Subscribe the event for StaticInfoUpdated and write some example data to stdout
        //acc.StaticInfoUpdated += (sender, e) =>
        //{
        //    Console.WriteLine($"Static info updated: {e.Data.CarModel} on {e.Data.Track}.");

        //};

        // Subscribe the event for PhysicsUpdated and write some example data to stdout
        acc.PhysicsUpdated += (sender, e) => {
            //Console.WriteLine("-------------------------------------------------");
            //Console.WriteLine($"Physics updated: IsEngineRunning: {e.Data.IsEngineRunning}.");
            //Console.WriteLine($"Physics updated: Speed: {e.Data.SpeedKmh}.");
            //Console.WriteLine($"Physics updated: AccG: {e.Data.Gas}.");
            //Console.WriteLine($"Physics updated: Brake: {e.Data.Brake}.");
            //Console.WriteLine($"Physics updated: SteerAngle: {e.Data.SteerAngle}.");
            //Console.WriteLine($"Physics updated: Gear: {e.Data.Gear}.");
            //Console.WriteLine($"Physics updated: Rpms: {e.Data.Rpms}."); 
            //Console.WriteLine($"Physics updated: KerbVibration: {e.Data.KerbVibration}.");
            //Console.WriteLine($"Physics updated: FrontLeft: {e.Data.WheelSlip.FrontLeft}.");
            //Console.WriteLine($"Physics updated: RearLeft: {e.Data.WheelSlip.RearLeft}.");
            //Console.WriteLine($"Physics updated: FrontRight: {e.Data.WheelSlip.FrontRight}.");
            //Console.WriteLine($"Physics updated: RearRight: {e.Data.WheelSlip.RearRight}.");
            //Console.WriteLine($"Physics updated: EngineBrake: {e.Data.EngineBrake}.");
            DriveInfoCheck(e.Data.IsEngineRunning,e.Data.SpeedKmh, e.Data.Gas, e.Data.Brake, e.Data.SteerAngle, e.Data.Gear, e.Data.Rpms, e.Data.KerbVibration, e.Data.WheelSlip.FrontLeft, e.Data.WheelSlip.RearLeft, e.Data.WheelSlip.FrontRight, e.Data.WheelSlip.RearRight);
        };

        // Subscribe the event for GraphicsUpdated and write some example data to stdout
        //acc.GraphicsUpdated += (sender, e) =>
        //{
        //    Console.WriteLine($"Graphics updated: In Pits: {e.Data.IsInPit}, In Pit Lane: {e.Data.IsInPitLane}, Flag: {e.Data.Flag}, LineOn: {e.Data.IdealLineOn}, Cars: {e.Data.ActiveCars}.");
        //};

        //// Subscribe to ctrl+c event on the console to cancel the program
        //Console.CancelKeyPress += (sender, e) =>
        //{
        //    Console.WriteLine("Cancelling...");

        //    cts.Cancel();
        //};

        // Wait for connection to the game (game startup)
        Console.WriteLine("Connecting...");

        await acc.ConnectAsync(cts.Token);

        Console.WriteLine("... Connected");

        // Wait till program canceled
        if (!cts.Token.IsCancellationRequested)
        {
            cts.Token.WaitHandle.WaitOne();
        }

        Environment.Exit(0);
    }

    private static int lastEngineStart = 0;
    private static float lastSpeed = 0;
    private static float lastGas = 0;
    private static float lastBrake = 0;
    private static int lastGear = 0;

    private static void DriveInfoCheck(int engineStart,float speed,float gas, float brake,float steerAngle,int gear,int rpms,float kerbVibration,float frontLeft, float rearLeft,float frontRight,float rearRight)
    {
        if (engineStart == 0)
        {
            if (lastEngineStart == 1)
            {
                Console.WriteLine("EngineClosed"); 
                _TrueGear.Play("EngineClosed");
            }
            lastEngineStart = 0;
            return;
        }
        Console.WriteLine("-------------------------------------------------");
        if (lastEngineStart == 0 && engineStart == 1)
        {
            lastEngineStart = 1;
            Console.WriteLine("EngineStarted");
            _TrueGear.Play("EngineStarted");
        }
        lastEngineStart = 1;
        if (lastGear > gear)
        {
            Console.WriteLine("DownShift");
            Console.WriteLine(lastGear);
            Console.WriteLine(gear);

            _TrueGear.Play("DownShift");
        }
        else if (lastGear < gear)
        {
            Console.WriteLine("UpShift");
            Console.WriteLine(lastGear);
            Console.WriteLine(gear);
            _TrueGear.Play("UpShift");
        }
        lastGear = gear;
        if (gear == 0)
        {
            if (lastGas < gas)
            {
                int power = (int)((gas - lastGas) * 5);
                Console.WriteLine("AcceleratorR" + power);
                _TrueGear.Play("AcceleratorR" + power);
            }
            if (lastBrake < brake)
            {
                if (speed > 5)
                {
                    //int power = (int)((brake - lastBrake) * 5);
                    int power = (int)(brake * 5);
                    Console.WriteLine("BrakeR" + power);
                    _TrueGear.Play("BrakeR" + power);
                }
            }
        }
        else
        {
            if (lastGas < gas)
            {
                int power = (int)((gas - lastGas) * 5);
                Console.WriteLine("Accelerator" + power);
                _TrueGear.Play("Accelerator" + power);
            }
            if (brake > 0)
            {
                if (speed > 10)
                {
                    //int power = (int)((brake - lastBrake) * 5);
                    int power = (int)(brake * 5);
                    Console.WriteLine("Brake" + power);
                    _TrueGear.Play("Brake" + power);
                }
            }
        }
        lastGas = gas;
        
        if (steerAngle != 0 && speed >= 1)
        {
            if (Math.Abs(steerAngle) > 0.01)
            {
                int power = Math.Abs((int)(steerAngle * 5));
                if (steerAngle < 0)
                {
                    Console.WriteLine("TurnLeft" + power);
                    _TrueGear.Play("TurnLeft" + power);
                }
                else if (steerAngle > 0)
                {
                    Console.WriteLine("TurnRight" + power);
                    _TrueGear.Play("TurnRight" + power);
                }
            }
        }
        //if (rpms > 0)
        //{
        //    int power = Math.Abs((int)(rpms / 2000));
        //    if (power > 5) power = 5;
        //    Console.WriteLine("Rmps" + power);
        //    _TrueGear.Play("Rmps" + power);
        //}
        if (kerbVibration > 0.01 && speed > 5)
        {
            int power = Math.Abs((int)(kerbVibration * 50));
            if (power > 5) power = 5;
            Console.WriteLine("KerbVibration" + power);
            _TrueGear.Play("KerbVibration" + power);
        }
        if (frontLeft > 4)
        {
            int power = Math.Abs((int)(frontLeft / 4));
            if (power > 5) power = 5;
            Console.WriteLine("WheelSlipFrontLeft" + power);
            _TrueGear.Play("WheelSlipFrontLeft" + power);
        }
        if (rearLeft > 4)
        {
            int power = Math.Abs((int)(rearLeft / 4));
            if (power > 5) power = 5;
            Console.WriteLine("WheelSlipRearLeft" + power);
            _TrueGear.Play("WheelSlipRearLeft" + power);
        }
        if (frontRight > 4)
        {
            int power = Math.Abs((int)(frontRight / 4));
            if (power > 5) power = 5;
            Console.WriteLine("WheelSlipFrontRight" + power);
            _TrueGear.Play("WheelSlipFrontRight" + power);
        }
        if (rearRight > 4)
        {
            int power = Math.Abs((int)(rearRight / 4));
            if (power > 5) power = 5;
            Console.WriteLine("WheelSlipRearRight" + power);
            _TrueGear.Play("WheelSlipRearRight" + power);
        }
        lastBrake = brake;
        if (lastSpeed < speed)
        {
            if (gear == 0)
            {
                int power = (int)((speed - lastSpeed - 1.1) * 10);
                if (power >= 0)
                {
                    if (power > 1) power = 1;
                    Console.WriteLine("SpeedUpR" + power);
                    _TrueGear.Play("SpeedUpR" + power);
                }
            }
            else
            {
                int power = (int)((speed - lastSpeed - 0.5) * 10);
                if (power >= 0)
                {
                    if (power > 5) power = 5;
                    Console.WriteLine("SpeedUp" + power);
                    _TrueGear.Play("SpeedUp" + power);
                }
            }
            lastSpeed = speed;
        }
        else if (lastSpeed > speed)
        {

            if (gear == 0)
            {
                if (brake > 0)
                {
                    if (lastSpeed - speed < 5)
                    {
                        lastSpeed = speed;
                        return;
                    }
                }
                else
                {
                    if (lastSpeed - speed < 2)
                    {
                        lastSpeed = speed;
                        return;
                    }
                }
                int power = (int)((lastSpeed - speed) / 10);
                if (power > 5) power = 5;
                Console.WriteLine("SpeedDownR" + power);
                _TrueGear.Play("SpeedDownR" + power);
            }
            else
            {
                if (brake > 0)
                {
                    if (lastSpeed - speed < 15)
                    {
                        lastSpeed = speed;
                        return;
                    }
                }
                else
                {
                    if (lastSpeed - speed < 4)
                    {
                        lastSpeed = speed;
                        return;
                    }
                }
                int power = (int)((lastSpeed - speed) / 30);
                if (power > 5) power = 5;
                Console.WriteLine("SpeedDown" + power);
                _TrueGear.Play("SpeedDown" + power);
            }
        }
        lastSpeed = speed;

    }
}