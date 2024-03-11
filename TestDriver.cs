﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adiIRC_DeepL_plugin_test
{
    internal class TestDriver
    {
        /// <summary>
        /// Driver for compiling and executing this plugin as a standalone application for test purposes.
        /// To use this, change compilation from Library to Console Application
        /// TODO:
        ///     Implement unit tests
        ///     Implement DeepL API Access
        /// </summary>
        /// <param name="args"></param>
        public static async Task Main(string[] args)
        {
            adiIRC_DeepL_plugin testPlugin = new adiIRC_DeepL_plugin();

            // Set up channel
            IChannel fuelratsChan = new IChannel();
            fuelratsChan.Name = "#fuelrats";
            List<IChannel> fuelratsChannelList = new List<IChannel>();
            fuelratsChannelList.Add(fuelratsChan);

            // Set up server
            IServer fuelratsServer = new IServer(fuelratsChannelList);
            List<IServer> fuelratsServerList = new List<IServer>();
            fuelratsServerList.Add(fuelratsServer);
            IPluginHost pluginHost = new IPluginHost(fuelratsChan, ".\\", fuelratsServerList);
            testPlugin.Initialize(pluginHost);

            //If API key is not saved, prompt user for key
            if(testPlugin.config_items.apikey == "") {
                Console.WriteLine("Input DeepL API Key: ");
                testPlugin.config_items.apikey = Console.ReadLine();
                testPlugin.save_config_items();
            }

            bool testResult = false;
            bool testAPI = false; // flip this switch to test API calls, keep off to save API usage

            // Test enabling debugmode
            testPlugin.deepl_set(new RegisteredCommandArgs("debugmode", fuelratsChan));
            if (adiIRC_DeepL_plugin.debugmode) testResult = true;
            else testResult = false;
            PrintTestResult("Enable Debug Mode", testResult);

            // Test deepl-any call
            // TODO: error handle API functions, and make unit tests
            //await testPlugin.deepl_any(new RegisteredCommandArgs("_ EN This is a test.", fuelratsChan));
            //await testPlugin.deepl_any(new RegisteredCommandArgs("_ ZZ This is a test.", fuelratsChan));

            // Test basic EN ratsignal
            Console.WriteLine("\n==== Autodetect EN Case ====");
            string rsig = "RATSIGNAL Case #3 PC HOR – CMDR Velica Foriana – System: \"TASCHETER SECTOR QN - T A3 - 1\" (Brown dwarf 79.9 LY from Sol) – Language: English (United Kingdom) (en-US) – Nick: Velica_Foriana (ODY_SIGNAL)";
            // Create example mecha rsig message
            ChannelNormalMessageArgs ratsignal = new ChannelNormalMessageArgs(rsig, fuelratsChan);
            ratsignal.User.Nick = "MechaSqueak[BOT]";
            testPlugin.deepl_auto_case(new RegisteredCommandArgs("", fuelratsChan));
            testPlugin.OnChannelNormalMessage(ratsignal);

            // Check if case was onboarded into correct monitor_items slot
            if (testPlugin.monitor_items[3] != null &&
                testPlugin.monitor_items[3].nickname.Equals("Velica_Foriana")) testResult = true;
            else testResult = false;
            PrintTestResult("EN Rsig Autodetect", testResult);


            // Test Xbox RSignal with (Offline) by cmdr name
            Console.WriteLine("\n==== Autodetect XB Offline Case ====");
            rsig = "RATSIGNAL Case #7 Xbox – CMDR Delrat (Offline) – System: \"LHS 2191\" (Invalid system name) – Language: English (United States) (en-US) (XB_SIGNAL)";
            // Create example mecha rsig message
            ratsignal = new ChannelNormalMessageArgs(rsig, fuelratsChan);
            ratsignal.User.Nick = "MechaSqueak[BOT]";
            testPlugin.OnChannelNormalMessage(ratsignal);

            // Check if case was onboarded into correct monitor_items slot
            if (testPlugin.monitor_items[7] != null &&
                testPlugin.monitor_items[7].nickname.Equals("Delrat")) testResult = true;
            else testResult = false;
            PrintTestResult("XBox Rsig Autodetect", testResult);



            // Test RU ratsignal
            Console.WriteLine("\n==== Autodetect RU Case ====");
            rsig = "RATSIGNAL Case #5 PC HOR – CMDR Delryn – System: \"SECTOR RU-C A14 - 2\" (Unconfirmed) – Language: Russian (Russia) (ru-RU) (HOR_SIGNAL)";
            ratsignal = new ChannelNormalMessageArgs(rsig, fuelratsChan);
            ratsignal.User.Nick = "MechaSqueak[BOT]";
            testPlugin.OnChannelNormalMessage(ratsignal);

            // Check if case was onboarded into correct monitor_items slot
            if (testPlugin.monitor_items[5] != null && 
                testPlugin.monitor_items[5].nickname.Equals("Delryn") &&
                testPlugin.monitor_items[5].langcode.Equals("RU")) testResult = true;
            else testResult = false;
            PrintTestResult("RU Rsig Autodetect", testResult);


            // Test auto EN
            ChannelNormalMessageArgs userMessage = new ChannelNormalMessageArgs("This is a test.", fuelratsChan);
            if (false)
            {
                Console.WriteLine("\n==== AutoStop Translations ====");
                userMessage.User.Nick = "Delryn";
                testPlugin.OnChannelNormalMessage(userMessage);
                System.Threading.Thread.Sleep(1000);
                testPlugin.OnChannelNormalMessage(userMessage);
                System.Threading.Thread.Sleep(1000);
                testPlugin.OnChannelNormalMessage(userMessage);
                System.Threading.Thread.Sleep(1000);
                testPlugin.OnChannelNormalMessage(userMessage);
                if (testPlugin.monitor_items[5] != null &&
                    testPlugin.monitor_items[5].langcode.Equals("EN")) testResult = true;
                else testResult = false;
                PrintTestResult("Auto Stop Translate", testResult);
            }

            // Test case remove
            Console.WriteLine("\n==== /dl-rm by case number ====");
            testPlugin.deepl_rm(new RegisteredCommandArgs("3", fuelratsChan));
            // test case 3 should be null, and test case 5 should be unaffected
            if (testPlugin.monitor_items[3] == null && testPlugin.monitor_items[5] != null && testPlugin.monitor_items[5].nickname.Equals("Delryn")) testResult = true;
            else testResult = false;
            PrintTestResult("Case Number Removed", testResult);


            // Test manual monitoring
            Console.WriteLine("\n==== /dl-mon ====");
            testPlugin.deepl_mon(new RegisteredCommandArgs("JaneDoe", fuelratsChan));
            if (testPlugin.monitor_items[20] != null &&
                testPlugin.monitor_items[20].nickname.Equals("JaneDoe") &&
                testPlugin.monitor_items[20].langcode.Equals("ZZ")) testResult = true;
            else testResult = false;
            PrintTestResult("Manual Monitor Add", testResult);


            // Test manual remove
            Console.WriteLine("\n==== /dl-rm for Manual Add ====");
            testPlugin.deepl_rm(new RegisteredCommandArgs("JaneDoe", fuelratsChan));
            if (testPlugin.monitor_items.Count == 20) testResult = true;
            else testResult = false;
            PrintTestResult("Manual Monitor Removed", testResult);


            // Print help message
            //testPlugin.deepl_help(new RegisteredCommandArgs("", fuelratsChan));


            // Test good & bad lang codes
            if (testAPI)
            {
                Console.WriteLine("\n==== Good and Bad Lang Code ====");
                string success = await testPlugin.deepl_any(new RegisteredCommandArgs("_ FR This is a test.", fuelratsChan));
                string failure = await testPlugin.deepl_any(new RegisteredCommandArgs("_ ZZ This is a test.", fuelratsChan));
                if (success.Equals("Il s'agit d'un test.") && String.IsNullOrEmpty(failure)) testResult = true;
                else testResult = false;
                PrintTestResult("Good/Bad Lang Code", testResult);
            }


            // Test Reverse Translation
            if (testAPI)
            {
                Console.WriteLine("\n==== Reverse Translation ====");
                if (!adiIRC_DeepL_plugin.reverseTranslate) testPlugin.deepl_set(new RegisteredCommandArgs("reverseTranslate", fuelratsChan));
                string translation = await testPlugin.deepl_any(new RegisteredCommandArgs("_ FR This is a test.", fuelratsChan));
                if (translation.Equals("Il s'agit d'un test.|This is a test.")) testResult = true;
                else testResult = false;
                PrintTestResult("Reverse Translation", testResult);
            }


            Console.Write("Press the any key to continue...");
            Console.Read();

        }

        public static void PrintTestResult(string testName, bool passed)
        {

            if (passed)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("PASS");
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("FAILED");
            }


            Console.ResetColor();
            Console.WriteLine(" - " +testName);
        }
    }
}
