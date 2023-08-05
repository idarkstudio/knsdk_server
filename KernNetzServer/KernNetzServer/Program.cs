using System;
using FigNet.Core;
using FigNetCommon;
using FigNet.Server;
using System.Threading;
using System.Diagnostics;

namespace EntangleServer
{
    class Program
    {
        private static int frameMilliseconds;
        private static float deltaTime = 0;
        private static string _version = "v0.7.3f1";
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;

            string welcome = @" ______________        _____   __    _____     __________      _____                      ______     
___  ____/__(_)______ ___  | / /______  /_    ___  ____/________  /______ ______________ ___  /____ 
__  /_   __  /__  __ `/_   |/ /_  _ \  __/    __  __/  __  __ \  __/  __ `/_  __ \_  __ `/_  /_  _ \
_  __/   _  / _  /_/ /_  /|  / /  __/ /_      _  /___  _  / / / /_ / /_/ /_  / / /  /_/ /_  / /  __/
/_/      /_/  _\__, / /_/ |_/  \___/\__/      /_____/  /_/ /_/\__/ \__,_/ /_/ /_/_\__, / /_/  \___/ 
              /____/                                                             /____/              ";


            string version = @"                                           _ _                    _ _ _   _             
                                          (_) |                  | (_) | (_)            
  ___ ___  _ __ ___  _ __ ___  _   _ _ __  _| |_ _   _    ___  __| |_| |_ _  ___  _ __  
 / __/ _ \| '_ ` _ \| '_ ` _ \| | | | '_ \| | __| | | |  / _ \/ _` | | __| |/ _ \| '_ \ 
| (_| (_) | | | | | | | | | | | |_| | | | | | |_| |_| | |  __/ (_| | | |_| | (_) | | | |
 \___\___/|_| |_| |_|_| |_| |_|\__,_|_| |_|_|\__|\__, |  \___|\__,_|_|\__|_|\___/|_| |_|
                                                  __/ |                                 
                                                 |___/                                  ";
            
            Console.WriteLine(welcome);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(version);
            
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.Title = $"FigNet Entangle {_version} Community Edition provided as a development kit, not intented for distribution";

            FN.BeforeInit();
            ENetProvider.Module.Load();
            WebSocketCoreProvider.Module.Load();
            //LiteNetLibProvider.Module.Load();
            
            FN.Logger = new DefaultLogger();
            FN.OnSettingsLoaded = () => {

                var logger = new DefaultLogger();
                bool fileLogging = false;
                if (FN.Settings.LoggingMethod == "NONE")
                {
                    FN.Logger = new NullLogger();
                }
                else
                {
                    if (FN.Settings.LoggingMethod == "CONSOLE")
                    {
                        fileLogging = false;
                    }
                    else if (FN.Settings.LoggingMethod == "FILE")
                    {
                        fileLogging = true;
                    }
                    logger.SetUp(fileLogging, "ServerLogs");
                    FN.Logger = logger;
                }
            };
            IServer serverApp = new ServerApplication();
            serverApp.SetUp();
            ServiceLocator.Bind(typeof(IServer), serverApp);


            var serializer = new Default_Serializer();
            serializer.RegisterPayloads();

            FigNetCommon.Utils.Logger = FN.Logger;
            FigNetCommon.Utils.Serializer = serializer;

            //  FigNetCommon.Utils.RegisterPayloads();

            IServerSocketListener listner = new ServerConnectionListener();
            FN.BindServerListner(listner);

            FN.Logger.Info($"FrameRate {FN.Settings.FrameRate}");

            Run(serverApp);
            serverApp.TearDown();
        }

        private static void Run(IServer server)
        {
            frameMilliseconds = 1000 / FN.Settings.FrameRate;

            Stopwatch stopwatch = new Stopwatch();
            int overTime = 0;

            while (true)
            {
                stopwatch.Restart();

                deltaTime = (frameMilliseconds + overTime) * 0.001f;

                server.Process(deltaTime);
                stopwatch.Stop();

                int stepTime = (int)stopwatch.ElapsedMilliseconds;

                if (stepTime <= frameMilliseconds)
                {
                    Thread.Sleep(frameMilliseconds - stepTime);
                    overTime = 0;
                }
                else
                {
                    overTime = stepTime - frameMilliseconds;
                }
            }
        }
    }
}
