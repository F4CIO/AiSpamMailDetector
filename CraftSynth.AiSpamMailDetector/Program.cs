using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CraftSynth.BuildingBlocks.IO;
using CraftSynth.BuildingBlocks.Logging;
using Microsoft.ML;

namespace CraftSynth.AiSpamMailDetector;

public class Program
{
    [STAThread] 
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the CraftSynth.AiSpamMailDetector!");
        Console.WriteLine("This program detects spam emails using a machine learning model.");
        Console.WriteLine("You can specify the .eml file path as the first parameter, and optionally:");
        Console.WriteLine("  - resultNumberForSpam: the exit code for spam detection (default is read from settings file)");
        Console.WriteLine("  - resultNumberForHam: the exit code for ham detection (default is read from settings file)");
        Console.WriteLine("  - resultNumberForError: the exit code for errors (default is read from settings file)");
        Console.WriteLine("  - modelFilePath: the path to the model file (default is read from settings file)");
        
        int? resultNumberForSpam = null;
        int? resultNumberForHam = null;
        int? resultNumberForError = null;
        string? modelFilePath = null;
        if(args.Length == 0)
        {
            Console.WriteLine("No .eml file path specified. Please provide the path as the first parameter.");
        }
        else{ 
            string emlFilepath = args[0];
            if(args.Length > 1)
            {
                resultNumberForSpam = int.TryParse(args[1], out int spamResult) ? spamResult : null;
            }
            if(args.Length > 2)
            {
                resultNumberForHam = int.TryParse(args[2], out int hamResult) ? hamResult : null;
            }
            if(args.Length > 3)
            {
                resultNumberForError = int.TryParse(args[3], out int errorResult) ? errorResult : null;
            }
            if(args.Length > 4)
            {
                modelFilePath = args[4];
            }
            if(args.Length > 5)
            {
                Console.WriteLine("Too many parameters specified. Only the first five are used: .eml file path, resultNumberForSpam, resultNumberForHam, resultNumberForError, modelFilePath.");
            }

            int exitCode = Detect(emlFilepath, resultNumberForSpam, resultNumberForHam, resultNumberForError, modelFilePath);
            Environment.Exit(exitCode);
        }
    }

    public static int Detect(string emlFilepath, int? resultNumberForSpam=null, int? resultNumberForHam=null, int? resultNumberForError=null, string? modelFilePath=null){
        int? r = null;

        BuildingBlocks.Logging.CustomTraceLog log = new CustomTraceLog("Starting...----------------------------------------------------------------------------------------------------------", true, false, CustomTraceLogAddLinePostProcessingEvent);

        try
        {
            using(log.LogScope("Reading and parsing parameters... "))
            {
                if(resultNumberForError == null)
                {
                    resultNumberForError = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<int>("resultNumberForError", null, true, -1, true, -1, false, -1, '=');
                }
                log.AddLine("resultNumberForError=" + resultNumberForError);

                if(resultNumberForSpam == null)
                {
                    resultNumberForSpam = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<int>("resultNumberForSpam", null, true, -1, true, -1, false, -1, '=');
                }
                log.AddLine("resultNumberForSpam=" + resultNumberForSpam);

                if(resultNumberForHam == null)
                {
                    resultNumberForHam = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<int>("resultNumberForHam", null, true, -1, true, -1, false, -1, '=');
                }
                log.AddLine("resultNumberForHam=" + resultNumberForSpam);

                if(modelFilePath == null)
                {
                    modelFilePath = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<string>("modelFilePath", null, true, null, true, null, false, null, '=');
                }
                log.AddLine("modelFilePath=" + modelFilePath);
                if(!File.Exists(modelFilePath) && File.Exists(Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, modelFilePath)))
                {
                    modelFilePath = Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, modelFilePath);
                }
                else if(!File.Exists(modelFilePath) && !File.Exists(Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, modelFilePath)))
                {
                    log.AddLine("Model file not found: " + modelFilePath);
                    r = resultNumberForError;
                }
                if(r == null && emlFilepath == null)
                {
                    log.AddLine(".eml file path must be specified as first parameter.");
                    r = resultNumberForError;
                }
                if(r == null && !File.Exists(emlFilepath))
                {
                    log.AddLine(".eml file not found: " + emlFilepath);
                    r = resultNumberForError;
                }
                if(r==null)
                {
                    log.AddLine("emlFilepath=" + emlFilepath);
                }
            }

            if(r==null)
            {
                bool isSpam = CraftSynth.AiSpamMailDetector.Engine.Main.Detect(emlFilepath, modelFilePath, log);
                r = isSpam ? resultNumberForSpam : resultNumberForHam;
            }

        }
        catch(Exception e)
        {
            log.AddLine("Error:" + e.Message);
            e = BuildingBlocks.Common.Misc.GetDeepestException(e);
            log.AddLine("Deepest exception:" + e.Message);
            log.AddLine("StackTrace:" + e.StackTrace);
            r = resultNumberForError??-1;
        }

        log.AddLine("Exit code: " + r);
        return r.Value;
    }
       
    private static void CustomTraceLogAddLinePostProcessingEvent(BuildingBlocks.Logging.CustomTraceLog log, string line, bool inNewLine, int level, string lineVersionSuitableForLineEnding, string lineVersionSuitableForNewLine)
    {
        string logFilePath = BuildingBlocks.Common.Misc.ApplicationRootFolderPath + "\\CraftSynth.AiSpamMailDetector.log";
        BuildingBlocks.IO.FileSystem.AppendFile(logFilePath, inNewLine ? "\r\n" + lineVersionSuitableForNewLine : lineVersionSuitableForLineEnding, FileSystem.ConcurrencyProtectionMechanism.Lock, null);
        Console.Write((inNewLine ? "\r\n" + line : line));
    }
}
