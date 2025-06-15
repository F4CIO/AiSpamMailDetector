using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CraftSynth.BuildingBlocks.Logging;
using Microsoft.ML;

namespace CraftSynth.AiSpamMailDetector.Engine;
public class Main
{
    public static bool Detect(string emlFilepath, string? modelFilePath, CustomTraceLog log)
    {
        string from;
        string emailContent;
        log = log ?? new CustomTraceLog("Starting...----------------------------------------------------------------------------------------------------------", true, false, CustomTraceLogAddLinePostProcessingEvent);

        using(log.LogScope("Reading .eml file... "))
        {
            emailContent = EMailPreparator.GetEMailAsPlainText(emlFilepath, out from, out _, out _);
            log.AddLine("From: " + from);
        }

        var mlContext = new MLContext();

        ITransformer loadedModel;
        using(log.LogScope("Loading model from the disk... "))
        {
            DataViewSchema modelSchema;
            using(var fileStream = new FileStream(modelFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(fileStream, out modelSchema);
            }
        }

        PredictionEngine<Email, SpamPrediction> predictionEngine;
        using(log.LogScope("Creating prediction engine... "))
        {
            predictionEngine = mlContext.Model.CreatePredictionEngine<Email, SpamPrediction>(loadedModel);
        }

        var email = new Email { Content = emailContent };
        SpamPrediction emailPrediction;
        using(log.LogScope("Detecting... "))
        {
            emailPrediction = predictionEngine.Predict(email);
        }

        log.AddLine("IsSpam: " + emailPrediction.IsSpam);
        return emailPrediction.IsSpam;
    }

    private static void CustomTraceLogAddLinePostProcessingEvent(BuildingBlocks.Logging.CustomTraceLog log, string line, bool inNewLine, int level, string lineVersionSuitableForLineEnding, string lineVersionSuitableForNewLine)
    {
        //string logFilePath = BuildingBlocks.Common.Misc.ApplicationPhysicalExeFilePathWithoutExtension + ".log";
        //BuildingBlocks.IO.FileSystem.AppendFile(logFilePath, inNewLine ? "\r\n" + lineVersionSuitableForNewLine : lineVersionSuitableForLineEnding, FileSystem.ConcurrencyProtectionMechanism.Lock, null);
        Console.Write((inNewLine ? "\r\n" + line : line));
    }
}
