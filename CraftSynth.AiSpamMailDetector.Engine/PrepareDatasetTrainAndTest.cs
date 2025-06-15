using System;
using System.Diagnostics;
using Microsoft.ML;
using MimeKit;
using CraftSynth.BuildingBlocks.Common;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace CraftSynth.AiSpamMailDetector.Engine;

public class PrepareDatasetTrainAndTest
{
    private const string WORKING_FOLDER_PATH = @"D:\Projects\CraftSynth.VirusScanner\WorkingFolder";

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void DummyTest()
    {
        // This is a dummy test to ensure the test framework is working
        Assert.Pass("Dummy test passed.");
    }

    //step 01: save desired emails as .eml files into AllMailFolder

    [Test]
    public void Step02_AllMailsToAllMailsAsPlainText()
    {
        var allMailFolderPath = Path.Combine(WORKING_FOLDER_PATH,"AllMail");
        var allMailAsPlainTextFolderPath = Path.Combine(WORKING_FOLDER_PATH, "AllMailAsPlainText");
        var allMailFilepaths = CraftSynth.BuildingBlocks.IO.FileSystem.GetFilePaths(allMailFolderPath, false, "*.eml");
        foreach(var filePath in allMailFilepaths)
        {
            string from;
            var mailAsPlainTextContent = EMailPreparator.GetEMailAsPlainText(filePath, out from, out _, out _);
            var mailAsPlainTextFilePath = Path.Combine(allMailAsPlainTextFolderPath, from+" - "+Path.GetFileName(filePath).FirstXChars(100,"...eml"));
            if(File.Exists(mailAsPlainTextFilePath))
            {
                File.Delete(mailAsPlainTextFilePath);
            }
            File.WriteAllText(mailAsPlainTextFilePath, mailAsPlainTextContent);
        }
    }

    //step 03: make backup of AllMailAsPlainText folder


    [Test]
    public void Step04_CollectedAddressesCsvToSafeSendersIni()
    {
        var collectedAddressesCsvFilePath = Path.Combine(WORKING_FOLDER_PATH, "Collected Addresses.csv");
        var safeSendersIniFilePath = Path.Combine(WORKING_FOLDER_PATH, "SafeSenders.ini");

        if(!File.Exists(collectedAddressesCsvFilePath))
        {
            Assert.Fail($"The file {collectedAddressesCsvFilePath} does not exist.");
        }

        var safeSenders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(var line in File.ReadLines(collectedAddressesCsvFilePath))
        {
            //detect all email addresses in the line and add them to the safeSenders set
            var emailAddresses = GetEmailAddresses(line);
            foreach(var emailAddress in emailAddresses)
            {
                if(!string.IsNullOrWhiteSpace(emailAddress))
                {
                    safeSenders.Add(emailAddress.Trim().ToLowerInvariant());
                }
            }
        }

        File.WriteAllLines(safeSendersIniFilePath, safeSenders);
    }
  
    private static IEnumerable<string> GetEmailAddresses(string input)
    {
        if(string.IsNullOrWhiteSpace(input))
        {
            return new List<string>();
        }

        // Regular expression to match email addresses
        var emailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
        var matches = emailRegex.Matches(input);

        var emailAddresses = new List<string>();
        foreach(Match match in matches)
        {
            emailAddresses.Add(match.Value);
        }

        return emailAddresses;
    }

    //step 05: hand pick emails from safe senders and copy into FromSafeSendersHandPicked folder

    //step 06: make backup of FromSafeSendersHandPicked folder

    [Test]
    public void Step07_FromSafeSendersHandPickedFolder_BuildSafeSendersIniFile()
    {
        var safeSendersFolder = Path.Combine(WORKING_FOLDER_PATH, "FromSafeSendersHandPicked");
        var safeSendersIniFile = Path.Combine(WORKING_FOLDER_PATH, "SafeSenders.ini");
        var allMailFilepaths = CraftSynth.BuildingBlocks.IO.FileSystem.GetFilePaths(safeSendersFolder, false, "*.eml");
        List<string> safeSenders = new List<string>();
        
        File.ReadAllLines(safeSendersIniFile).ToList().ForEach(line => safeSenders.Add(line.ToLower().Trim()));
        
        foreach(var filePath in allMailFilepaths)
        {
            string from;
            var mailAsPlainTextContent = EMailPreparator.GetEMailAsPlainText(filePath, out from, out _, out _);
            from = from.ToLower().Trim();

            if(!safeSenders.Contains(from))
            {
                safeSenders.Add(from);
            }            
        }

        safeSenders = safeSenders.Distinct().ToList();
        safeSenders.Sort(StringComparer.OrdinalIgnoreCase); // Sort the list in a case-insensitive manner
        
        //write the safe senders to the ini file
        using(var writer = new StreamWriter(safeSendersIniFile, false, Encoding.UTF8))
        {
            foreach(var sender in safeSenders)
            {
                writer.WriteLine(sender);
            }
        }
    }

    [Test]
    public void Step08_MoveAllMailsFromAllMailAsPlainTextFolder_ToFromSafeSendersFolder_OnlyOnesSentbySendersFromSafeSendersIniFile()
    {
        var allMailAsPlainTextFolderPath = Path.Combine(WORKING_FOLDER_PATH, "AllMailAsPlainText");
        var safeSendersIniFile = Path.Combine(WORKING_FOLDER_PATH, "SafeSenders.ini");
        var safeSenders = File.ReadAllLines(safeSendersIniFile).Select(line => line.ToLower().Trim()).ToList();
        var allMailAsPlainTextFilePaths = CraftSynth.BuildingBlocks.IO.FileSystem.GetFilePaths(allMailAsPlainTextFolderPath, false, "*.eml");
        var fromSafeSendersFolderPath = Path.Combine(WORKING_FOLDER_PATH, "FromSafeSenders");
       
        foreach(var filePath in allMailAsPlainTextFilePaths)
        {
            string from;
            var mailAsPlainTextContent = EMailPreparator.GetEMailAsPlainText(filePath, out from, out _, out _);
            from = from.ToLower().Trim();
            if(safeSenders.Contains(from))
            {
                //copy the file to the SafeMails folder
                var destFilePath = Path.Combine(fromSafeSendersFolderPath, Path.GetFileName(filePath));
                if(File.Exists(destFilePath))
                {
                    File.Delete(destFilePath);
                }
                File.Copy(filePath, destFilePath);
                File.Delete(filePath); 
            }
        }
    }

    [Test]
    public void Step09_MoveAllMailsFromAllMailAsPlainTextFolder_ToSpamFolder_IfTheyContainSpamPhrases()
    {
        var allMailAsPlainTextFolderPath = Path.Combine(WORKING_FOLDER_PATH, "AllMailAsPlainText");
        var spamPhrasesTxtFile = Path.Combine(WORKING_FOLDER_PATH, "SpamPhrases.txt");
        var spamFolderPath = Path.Combine(WORKING_FOLDER_PATH, "Spam");
        var allMailAsPlainTextFilePaths = CraftSynth.BuildingBlocks.IO.FileSystem.GetFilePaths(allMailAsPlainTextFolderPath, false, "*.eml");
        
        var spamPhrases = File.ReadAllText(spamPhrasesTxtFile).Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.ToLower().Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        foreach(var filePath in allMailAsPlainTextFilePaths)
        {
            var mailContent = File.ReadAllText(filePath);
            foreach(var spamPhrase in spamPhrases)
            {
                if(mailContent.ToLower().Contains(spamPhrase))
                {
                    //copy the file to the Spam folder
                    var destFilePath = Path.Combine(spamFolderPath, Path.GetFileName(filePath));
                    if(File.Exists(destFilePath))
                    {
                        File.Delete(destFilePath);
                    }
                    File.Copy(filePath, destFilePath);
                    File.Delete(filePath);
                    break; // No need to check other spam phrases for this file
                }
            }
        }
    }

    //step 10: for remaining emails in AllMailAsPlainText folder, move them either to HamHandPicked or SpamHandPicked folder based on manual inspection

    //step 11: HamHandPicked to Ham folder and SpamHandPicked to Spam folder

    //step 12: backup prevously created model.zip file if you want

    [Test]
    public void Step13_Train()
    {
        // Define file paths
        string hamFolderpath = Path.Combine(WORKING_FOLDER_PATH, "Ham");
        string spamFolderpath = Path.Combine(WORKING_FOLDER_PATH, "Spam");
        string modelPath = Path.Combine(WORKING_FOLDER_PATH, "ML_Model.zip");

        // Load Data
        var data = new List<Email>();        
        CraftSynth.BuildingBlocks.IO.FileSystem.GetFilePaths(hamFolderpath, false, "*.eml")
            .ForEach(filePath => data.Add(new Email { Content = File.ReadAllText(filePath), IsSpam = false }));
        CraftSynth.BuildingBlocks.IO.FileSystem.GetFilePaths(spamFolderpath, false, "*.eml")
            .ForEach(filePath => data.Add(new Email { Content = File.ReadAllText(filePath), IsSpam = true }));
        
        // Initialize ML Context and load data to it
        var mlContext = new MLContext();
        var trainData = mlContext.Data.LoadFromEnumerable(data);
        
        // Prepare Data
        var pipeline = mlContext.Transforms.Text
            .FeaturizeText("Features", nameof(Email.Content))
            .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());
        
        // Train the model
        var model = pipeline.Fit(trainData);
        
        // Save/overwrite the model to disk
        using(var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
        {
            mlContext.Model.Save(model, trainData.Schema, fileStream);
        }
    }

    //step 14: save some emails as .eml into ToTest folder (in Thunderbird select several, right-click and select Save...)

    [Test]
    public void Step15_TestModel()
    {
        // Define the path where you want to save the model
        string modelFilePath = Path.Combine(WORKING_FOLDER_PATH, "ML_Model.zip");

        // Initialize ML Context
        var mlContext = new MLContext();       

        // Load the model from disk
        ITransformer loadedModel;
        DataViewSchema modelSchema;
        using(var fileStream = new FileStream(modelFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            loadedModel = mlContext.Model.Load(fileStream, out modelSchema);
        }

        // Test Model with a sample email
        var sampleEmail = new Email { Content = "Special discount, buy now!" };
        var predictionEngine = mlContext.Model.CreatePredictionEngine<Email, SpamPrediction>(loadedModel);
        var prediction = predictionEngine.Predict(sampleEmail);
        //Assert.That(prediction.IsSpam);
        Debug.WriteLine($"Email: '{sampleEmail.Content}' is {(prediction.IsSpam ? "spam" : "not spam")}");
        //Assert.Pass();

        //test model against mails in ToTest folder
        var toTestFolderPath = Path.Combine(WORKING_FOLDER_PATH, "ToTest");
        var toTestFilePath = CraftSynth.BuildingBlocks.IO.FileSystem.GetFilePaths(toTestFolderPath, false, "*.eml");
        foreach(var filePath in toTestFilePath)
        {
            Debug.WriteLine($"FileName:" + Path.GetFileName(filePath)); 
            var emailContent = EMailPreparator.GetEMailAsPlainText(filePath, out _, out _, out _);
            var email = new Email { Content = emailContent };
            var emailPrediction = predictionEngine.Predict(email);
            Debug.WriteLine($"Email: '{email.Content}' is {(emailPrediction.IsSpam ? "spam" : "not spam")}");
            
            //delete both spam and ham files if they exist
            var filePathForSpamResult = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".spam");
            if(File.Exists(filePathForSpamResult))
            {
                File.Delete(filePathForSpamResult);
            }
            var filePathForHamResult = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".ham");            
            if(File.Exists(filePathForHamResult))
            {
                File.Delete(filePathForHamResult);
            }

            //Save the email content to the appropriate file based on the prediction
            if(emailPrediction.IsSpam)
            {
                File.WriteAllText(filePathForSpamResult, emailContent);
            }
            else
            {
                File.WriteAllText(filePathForHamResult, emailContent);
            }
        }
    }
}