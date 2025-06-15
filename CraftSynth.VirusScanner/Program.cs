using System;
using System.CodeDom;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using CraftSynth.BuildingBlocks.Common;
using CraftSynth.BuildingBlocks.IO;
using CraftSynth.BuildingBlocks.Logging;
using System.IO.Compression;

using CraftSynth.BuildingBlocks;

namespace CraftSynth.VirusScanner
{
	class Program
	{
		private const int whichNumberToReturnIfErrorOccured = -1;
		static int Main(string[] args)
		{
			int? r = null;	

			BuildingBlocks.Logging.CustomTraceLog log = new CustomTraceLog("Starting...----------------------------------------------------------------------------------------------------------", true, false, CustomTraceLogAddLinePostProcessingEvent);
			try
			{
				DateTime now = DateTime.Now;
				string nowUniqueString = now.ToDateAndTimeInSortableFormatForFileSystem() + "-" + now.Millisecond.ToString().PadLeft(3,'0');
				string sendersEMailAddress = "Unknown";
				string receiversEMailAddress = "Unknown";
				string subject = string.Empty;
				bool isPlainText = false; 
                string eMailHeadingAndBody = string.Empty;

                string filePath = args[0];
				string destinationFolderForClonedEmlFiles = null;
				bool shouldDeleteClonedEmlFileAfterProcessing = true;
				bool shouldDeleteClonedEmlFileFolderAfterProcessing = false;
				bool shouldDeleteExtractedEmlContentAfterProcessing = false;
				long maxSizeOfEmlFileThatWillBeProcessedInKBytes = -1;
				long virusIsNeverLargerThanXKBytes = -1;
				List<string> virusFileNameExtensions = null;
				List<string> virusPhrases = null;
				int virusNeverHasMoreThanXFilesPackedInZip = -1;
				List<string> virusFileNameExtensionsInZip = null;
				bool shouldCopyVirusToDestinationFolder = false;
				string destinationFolderForDetectedVirus = null;
				int whichNumberToReturnIfIsVirus = -1;
				int whichNumberToReturnIfIsNotVirus = -1;
				List<string> trustedEMails = null;
				List<string> trustedEMailsDomains = null;
				List<string> trustedEMailsDomainsOfBigProviders = null;
                List<string> trustedPhrases = null;
				bool useAiSpamMailDetector = true;
				string modelFilePath = null;


                using (log.LogScope("Processing '" + filePath.ToNonNullString("null") + "' ..."))
				{
					using (log.LogScope("Reading and parsing parameters... "))
                    {
                        destinationFolderForClonedEmlFiles = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<string>("destinationFolderForClonedEmlFiles", null, true, null, false, string.Empty, false, null, '=');
                        log.AddLine("destinationFolderForClonedEmlFiles=" + destinationFolderForClonedEmlFiles);

                        shouldDeleteClonedEmlFileAfterProcessing = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<bool>("shouldDeleteClonedEmlFileAfterProcessing", null, true, false, true, false, false, false, '=');
                        log.AddLine("shouldDeleteClonedEmlFileAfterProcessing=" + shouldDeleteClonedEmlFileAfterProcessing);

                        shouldDeleteExtractedEmlContentAfterProcessing = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<bool>("shouldDeleteExtractedEmlContentAfterProcessing", null, true, false, true, false, false, false, '=');
                        log.AddLine("shouldDeleteExtractedEmlContentAfterProcessing=" + shouldDeleteExtractedEmlContentAfterProcessing);

                        maxSizeOfEmlFileThatWillBeProcessedInKBytes = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<long>("maxSizeOfEmlFileThatWillBeProcessed", null, true, -1, false, long.MaxValue, false, -1, '=');
                        log.AddLine("maxSizeOfEmlFileThatWillBeProcessedInKBytes=" + maxSizeOfEmlFileThatWillBeProcessedInKBytes);

                        virusIsNeverLargerThanXKBytes = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<long>("virusIsNeverLargerThanXKBytes", null, true, -1, false, long.MaxValue, false, -1, '=');
                        log.AddLine("virusIsNeverLargerThanXKBytes=" + virusIsNeverLargerThanXKBytes);

                        string virusFileNameExtensionsCsv = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<string>("virusFileNameExtensionsCsv", null, true, null, false, string.Empty, false, null, '=');
                        log.AddLine("virusFileNameExtensionsCsv=" + virusFileNameExtensionsCsv);
                        virusFileNameExtensions = virusFileNameExtensionsCsv.IsNullOrWhiteSpace() ? new List<string>() : virusFileNameExtensionsCsv.ParseCSV();

                        string virusPhrasesCsv = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<string>("virusPhrasesCsv", null, true, null, false, string.Empty, false, null, '=');
                        log.AddLine("virusPhrasesCsv=" + virusPhrasesCsv);
                        virusPhrases = LoadListOfStringsFromCsvValueOrFile(virusPhrasesCsv, log);
						log.AddLine("virusPhrases.Count=" + virusPhrases.Count.ToString());

                        virusNeverHasMoreThanXFilesPackedInZip = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<int>("virusNeverHasMoreThanXFilesPackedInZip", null, true, -1, true, -1, false, -1, '=');
                        log.AddLine("virusNeverHasMoreThanXFilesPackedInZip=" + virusNeverHasMoreThanXFilesPackedInZip);

                        string virusFileNameExtensionsInZipCsv = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<string>("virusFileNameExtensionsInZipCsv", null, true, null, false, string.Empty, false, null, '=');
                        log.AddLine("virusFileNameExtensionsInZipCsv=" + virusFileNameExtensionsInZipCsv);
                        virusFileNameExtensionsInZip = virusFileNameExtensionsInZipCsv.IsNullOrWhiteSpace() ? new List<string>() : virusFileNameExtensionsInZipCsv.ParseCSV();

                        shouldCopyVirusToDestinationFolder = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<bool>("shouldCopyVirusToDestinationFolder", null, true, false, true, false, false, false, '=');
                        log.AddLine("shouldCopyVirusToDestinationFolder=" + shouldCopyVirusToDestinationFolder);

                        destinationFolderForDetectedVirus = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<string>("destinationFolderForDetectedVirus", null, true, null, false, string.Empty, false, null, '=');
                        log.AddLine("destinationFolderForDetectedVirus=" + destinationFolderForDetectedVirus);

                        whichNumberToReturnIfIsVirus = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<int>("whichNumberToReturnIfIsVirus", null, true, -1, true, -1, false, -1, '=');
                        log.AddLine("whichNumberToReturnIfIsVirus=" + whichNumberToReturnIfIsVirus);

                        whichNumberToReturnIfIsNotVirus = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<int>("whichNumberToReturnIfIsNotVirus", null, true, -1, true, -1, false, -1, '=');
                        log.AddLine("whichNumberToReturnIfIsNotVirus=" + whichNumberToReturnIfIsNotVirus);

                        string trustedEMailsCsv = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<string>("trustedEMailsCsv", null, true, null, false, string.Empty, false, null, '=');
                        log.AddLine("trustedEMailsCsv=" + trustedEMailsCsv);
						trustedEMails = LoadListOfStringsFromCsvValueOrFile(trustedEMailsCsv, log);
						trustedEMails = trustedEMails.Select(e => e.Trim()).Where(e => e.IsEMail()).ToList();
						log.AddLine("trustedEMails.Count=" + trustedEMails.Count.ToString());

                        string trustedEMailsDomainsCsv = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<string>("trustedEMailsDomainsCsv", null, true, null, false, string.Empty, false, null, '=');
                        log.AddLine("trustedEMailsDomainsCsv=" + trustedEMailsDomainsCsv);
                        trustedEMailsDomains = LoadListOfStringsFromCsvValueOrFile(trustedEMailsDomainsCsv, log);
                        trustedEMailsDomains = trustedEMailsDomains.Select(e => e.Trim()).ToList();
                        log.AddLine("trustedEMailsDomains.Count=" + trustedEMailsDomains.Count.ToString());
						
						string trustedEMailsDomainsOfBigProvidersCsv = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<string>("trustedEMailsDomainsOfBigProvidersCsv", null, true, null, false, string.Empty, false, null, '=');
                        log.AddLine("trustedEMailsDomainsOfBigProvidersCsv=" + trustedEMailsDomainsOfBigProvidersCsv);
                        trustedEMailsDomainsOfBigProviders = LoadListOfStringsFromCsvValueOrFile(trustedEMailsDomainsOfBigProvidersCsv, log);
                        trustedEMailsDomainsOfBigProviders = trustedEMailsDomainsOfBigProviders.Select(e => e.Trim()).ToList();
                        log.AddLine("trustedEMailsDomainsOfBigProviders.Count=" + trustedEMailsDomainsOfBigProviders.Count.ToString());

                        string trustedPhrasesCsv = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<string>("trustedPhrasesCsv", null, true, null, false, string.Empty, false, null, '=');
                        log.AddLine("trustedPhrasesCsv=" + trustedPhrasesCsv);                        
                        trustedPhrases = LoadListOfStringsFromCsvValueOrFile(trustedPhrasesCsv, log);
						log.AddLine("trustedPhrases.Count=" + trustedPhrases.Count.ToString());

                        useAiSpamMailDetector = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<bool>("useAiSpamMailDetector", null, true, false, true, false, false, false, '=');
                        log.AddLine("useAiSpamMailDetector=" + useAiSpamMailDetector);

						if(useAiSpamMailDetector)
						{
							if(modelFilePath == null)
							{
								modelFilePath = CraftSynth.BuildingBlocks.IO.FileSystem.GetSettingFromIniFile<string>("modelFilePath", null, true, null, true, null, false, null, '=');
							}
							log.AddLine("modelFilePath=" + modelFilePath);
							if(File.Exists(Path.GetFullPath(modelFilePath))){
								modelFilePath = Path.GetFullPath(modelFilePath);
							}else if(File.Exists(Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, modelFilePath)))
							{
								modelFilePath = Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, modelFilePath);
							}
							else if(!File.Exists(modelFilePath) && !File.Exists(Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, modelFilePath)))
							{
								throw new Exception("Model file not found: " + modelFilePath);
							}
						}
                    }

                    using (log.LogScope("Processing mail '" + filePath.ToNonNullString() + "'..."))
					{
						if (filePath.IsNullOrWhiteSpace())
						{
							throw new Exception("filePath can not be null or empty.");
						}
						if (string.Compare(Path.GetExtension(filePath).Trim('.'), "eml", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(Path.GetExtension(filePath).Trim('.'), "tmp", StringComparison.OrdinalIgnoreCase) != 0)
						{
							throw new Exception("Only .eml file type is supported however file extension can be .eml or .tmp.");
						}
                        if(File.Exists(Path.GetFullPath(filePath)))
                        {
                            filePath = Path.GetFullPath(filePath);
                        }
                        else if(File.Exists(Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, filePath)))
                        {
                            filePath = Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, filePath);
                        }
                        else if(!File.Exists(filePath) && !File.Exists(Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, filePath)))
                        {
                            throw new Exception("Eml file not found: " + filePath);
                        }

                        #region clone .eml file and set filePath to that new file

                        using (log.LogScope("Cloning .eml to destinationFolderForClonedEmlFiles..."))
						{
							//we need to clone hServerMail .tmp file in order to change it extension to .eml -only that extension is accepted by MsgReader.
							if (destinationFolderForClonedEmlFiles.StartsWith(@"\"))
							{
								destinationFolderForClonedEmlFiles = Path.Combine(BuildingBlocks.Common.Misc.ApplicationRootFolderPath, destinationFolderForClonedEmlFiles.TrimStart('\\'));
							}

							shouldDeleteClonedEmlFileFolderAfterProcessing = shouldDeleteClonedEmlFileAfterProcessing && destinationFolderForClonedEmlFiles.Contains("{0}");
							destinationFolderForClonedEmlFiles = destinationFolderForClonedEmlFiles.Replace("{0}", nowUniqueString);
							string destinationFilePath = Path.Combine(destinationFolderForClonedEmlFiles, nowUniqueString + ".eml"); //Path.GetFileName(filePath));
							log.AddLine("destinationFilePath=" + destinationFilePath);
							if (!Directory.Exists(Path.GetDirectoryName(destinationFilePath)))
							{
								using (log.LogScope("Creating directory..."))
								{
									Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
								}
							}

							using (log.LogScope("Performing copy..."))
							{
								File.Copy(filePath, destinationFilePath, true);
								filePath = destinationFilePath;                   //!!!
							}
						}
						#endregion

						try
						{
							log.AddLine("Checking against .eml file size...");
							long? emlFileSize = BuildingBlocks.IO.FileSystem.GetFileSizeInBytes(filePath, false);
							if (emlFileSize == null)
							{
								throw new Exception("Could not determine .eml file size.");
							}
							if (emlFileSize > maxSizeOfEmlFileThatWillBeProcessedInKBytes*1024)
							{
								log.AddLine(".eml too big for processing. assume clean.", false);
								r = whichNumberToReturnIfIsNotVirus;
							}
							else
							{
								using (log.LogScope("Parsing .eml file..."))
								{
									string extractedEmlFolderPath = Path.Combine(Path.GetDirectoryName(filePath), "_extractedEml_" + nowUniqueString);
									log.AddLine("extractedEmlFolderPath=" + extractedEmlFolderPath);
									if (!Directory.Exists(extractedEmlFolderPath))
									{
										using (log.LogScope("Creating extractedEmlFolderPath folder..."))
										{
											Directory.CreateDirectory(extractedEmlFolderPath);
										}
									}

									try
									{
										List<string> emailBodyAndAttachmentsFilePaths = null;
										using (log.LogScope("Extracting .eml file..."))
										{
											try
											{
												var msgReader = new MsgReader.Reader(); //https://www.codeproject.com/Tips/712072/Reading-an-Outlook-MSG-File-in-Csharp
												emailBodyAndAttachmentsFilePaths = msgReader.ExtractToFolder(filePath, extractedEmlFolderPath).ToList();
												if (emailBodyAndAttachmentsFilePaths.Count == 0)
												{
													throw new Exception("Nothing extracted from .eml file.");
												}
											}
											catch (Exception e)
											{
												throw new Exception("Error occured during extraction of .eml file.", e);
											}
										}
										eMailHeadingAndBody = File.ReadAllText(emailBodyAndAttachmentsFilePaths[0]).ToNonNullString();
                                        using(log.LogScope("Finding sender's email..."))
                                        {
                                            //TODO: use GetEMailAsPlainText instead MsgReader for everything not just from, to and subject fields
                                            CraftSynth.AiSpamMailDetector.Engine.EMailPreparator.GetEMailAsPlainText(filePath, out sendersEMailAddress, out receiversEMailAddress, out subject);
                                            //if (isPlainText)
                                            //{
                                            //	var lines = File.ReadAllLines(emailBodyAndAttachmentsFilePaths[0]);
                                            //	sendersEMailAddress = lines[0].GetSubstring("<", ">");
                                            //	subject = lines[3].GetSubstringAfter("Subject:").ToNonNullString().Trim();
                                            //	receiversEMailAddress = lines[2].GetSubstring("<", ">");
                                            //}
                                            //else
                                            //{
                                            //	//find first '"mailto:' phrase or @ sign
                                            //	//var text = File.ReadAllText(emailBodyAndAttachmentsFilePaths[0]);
                                            //	sendersEMailAddress = eMailHeadingAndBody.GetSubstring("&lt;", "&gt;"); //>Example: From:</td><td>f4cio&nbsp&lt;f4cio@f4cio.com&gt;</td></tr>
                                            //	subject = eMailHeadingAndBody.GetSubstring("Subject:</td><td>", "<br/>");
                                            //	receiversEMailAddress = eMailHeadingAndBody.GetSubstring("To:</td><td>", "</td>");
                                            //}
                                            if(sendersEMailAddress.IsNullOrWhiteSpace())
                                            {
                                                throw new Exception("Failed to parse sender's email.");
                                            }
                                            log.AddLine("   FROM: " + sendersEMailAddress.ToNonNullString("null"));
                                            log.AddLine("     TO: " + receiversEMailAddress.ToNonNullString("null"));
                                            log.AddLine("SUBJECT: " + subject.ToNonNullString("null"));
                                        }

                                        log.AddLine("Checking against trusted emails...");
										if(trustedEMails.Any(e => string.Compare(e, sendersEMailAddress, StringComparison.OrdinalIgnoreCase) == 0))
										{
											log.AddLine("EMail is from trusted sender '" + sendersEMailAddress + "'. seems clean.");
											r = whichNumberToReturnIfIsNotVirus;
										}
										else
										{
                                            log.AddLine("Checking against trusted domains...");
											string trustedDomain = GetFirstMatchedDomainOrReturnNull(sendersEMailAddress, trustedEMailsDomains, false, trustedEMailsDomainsOfBigProviders, log);
                                            if(trustedDomain!=null)
											{
												log.AddLine("EMail is from trusted domain '" + trustedDomain + "'. seems clean.");
												r = whichNumberToReturnIfIsNotVirus;
											}
											else
											{
												log.AddLine("EMail is not from trusted sender. Needs further checks.");

												log.AddLine("Checking against trusted phrases...");
												string phraseFound = GetFirstPhraseThatExistInTextOrReturnNull(eMailHeadingAndBody, trustedPhrases, true);
												if(phraseFound != null)
												{
													log.AddLine("Trusted phrase '" + phraseFound + "' found. seems clean.");
													r = whichNumberToReturnIfIsNotVirus;
												}
												else
												{
													log.AddLine("Checking against virus phrases...");
													phraseFound = GetFirstPhraseThatExistInTextOrReturnNull(eMailHeadingAndBody, virusPhrases, false);

													if(phraseFound != null)
													{
														log.AddLine("Virus phrase '" + phraseFound + "' found. virus detected.");
														r = whichNumberToReturnIfIsVirus;
													}
													else
													{
														log.AddLine("Neither trusted nor virus phrase found. needs further checks.");

														isPlainText = emailBodyAndAttachmentsFilePaths[0].ToLower().EndsWith(".txt");
														log.AddLine("isPlainText: " + isPlainText);


														if(emailBodyAndAttachmentsFilePaths.Count == 1)
														{
															log.AddLine("There are no attachments. Do AI check or assume clean.");
															r = AiCheck(filePath, log, whichNumberToReturnIfIsVirus, whichNumberToReturnIfIsNotVirus, whichNumberToReturnIfErrorOccured, useAiSpamMailDetector, modelFilePath);
														}
														else
														{
															if(emailBodyAndAttachmentsFilePaths.Count > 2)
															{
																log.AddLine("There is more than one attachment. Do AI check or assume clean.");
																r = AiCheck(filePath, log, whichNumberToReturnIfIsVirus, whichNumberToReturnIfIsNotVirus, whichNumberToReturnIfErrorOccured, useAiSpamMailDetector, modelFilePath);
															}
															else
															{
																using(log.LogScope("Checking attachment..."))
																{
																	string attachmentFilePath = emailBodyAndAttachmentsFilePaths[1];
																	if(!File.Exists(attachmentFilePath))
																	{
																		throw new Exception("File not found:" + attachmentFilePath.ToNonNullString());
																	}

																	log.AddLine("Checking against file size...");
																	long? fileSize = BuildingBlocks.IO.FileSystem.GetFileSizeInBytes(attachmentFilePath, false);
																	if(fileSize == null)
																	{
																		throw new Exception("Could not determine file size.");
																	}
																	if(fileSize > virusIsNeverLargerThanXKBytes * 1024)
																	{
																		log.AddLine("file size large. seems clean.", false);
																		r = whichNumberToReturnIfIsNotVirus;
																	}
																	else
																	{
																		log.AddLine("Needs further checks.", false);
																		log.AddLine("Checking against attachment extension...");
																		var ext = Path.GetExtension(attachmentFilePath).Trim('.');
																		if(virusFileNameExtensions.Exists(e => string.Compare(e, ext, StringComparison.OrdinalIgnoreCase) == 0))
																		{
																			log.AddLine("attachment extension '" + ext + "' is on a blacklist. virus detected.");
																			r = whichNumberToReturnIfIsVirus;
																		}
																		else
																		{
																			log.AddLine("Needs further checks.", false);
																			log.AddLine("Checking wether is zip...");
																			if(string.Compare(ext, "zip", StringComparison.OrdinalIgnoreCase) != 0)
																			{
																				log.AddLine("not zip. Do AI check or assume clean.", false);
																				r = AiCheck(filePath, log, whichNumberToReturnIfIsVirus, whichNumberToReturnIfIsNotVirus, whichNumberToReturnIfErrorOccured, useAiSpamMailDetector, modelFilePath);
																			}
																			else
																			{
																				log.AddLine("it is zip. Needs further checks.", false);
																				using(ZipArchive zip = ZipFile.OpenRead(attachmentFilePath))
																				{
																					log.AddLine("Zipfile:" + Path.GetFileName(attachmentFilePath));
																					log.AddLine("Comment:" + zip.Comment.ToNonNullString("null"));

																					if(zip.Entries.Count == 0)
																					{
																						log.AddLine("No files in zip. Do AI check or assume clean.");
																						r = AiCheck(filePath, log, whichNumberToReturnIfIsVirus, whichNumberToReturnIfIsNotVirus, whichNumberToReturnIfErrorOccured, useAiSpamMailDetector, modelFilePath);
																					}
																					else if(zip.Entries.Count > virusNeverHasMoreThanXFilesPackedInZip)
																					{
																						log.AddLine("Too many files in zip. Do AI check or assume clean.");
																						r = AiCheck(filePath, log, whichNumberToReturnIfIsVirus, whichNumberToReturnIfIsNotVirus, whichNumberToReturnIfErrorOccured, useAiSpamMailDetector, modelFilePath);
																					}
																					else
																					{
																						foreach(ZipArchiveEntry e in zip.Entries)
																						{
																							log.AddLine(string.Format("FileName={0}, Uncompressed={1} bytes, Compressed={2} bytes, Encrypted={3}",
																								e.Name,
																								//e.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
																								e.Length,
																								//e.CompressionRatio,
																								e.CompressedLength,
																								(e.IsEncrypted) ? "Y" : "N"));

																							string fileExtInZip = virusFileNameExtensionsInZip.FirstOrDefault(e1 => string.Compare(e1, Path.GetExtension(e.Name).Trim('.'), StringComparison.OrdinalIgnoreCase) == 0);
																							if(fileExtInZip != null)
																							{
																								log.AddLine("file extension from file in zip '" + fileExtInZip + "' is listed in virusFileNameExtensionsInZip. virus detected.");
																								r = whichNumberToReturnIfIsVirus;
																							}
																						}
																						if(r == null)
																						{
																							log.AddLine("Do AI check or assume clean.");
																							r = AiCheck(filePath, log, whichNumberToReturnIfIsVirus, whichNumberToReturnIfIsNotVirus, whichNumberToReturnIfErrorOccured, useAiSpamMailDetector, modelFilePath); ;
																						}
																					}
																				}

																			}
																		}
																	}
																}
															}
														}

													}
												}
											}
										}
									}
									finally
									{
										if (shouldDeleteExtractedEmlContentAfterProcessing)
										{
											try
											{
												Directory.Delete(extractedEmlFolderPath, true);
											}
											catch (Exception e)
											{
												log.AddLine("Couldn't delete extractedEmlFolderPath folder. Error:" + e.Message);
											}
										}
									}
								}
							}

							if (r == whichNumberToReturnIfIsVirus && shouldCopyVirusToDestinationFolder)
							{
								using (log.LogScope("Copying .eml to destinationFolderForDetectedVirus..."))
								{
									if (destinationFolderForDetectedVirus.StartsWith(@"\"))
									{
										destinationFolderForDetectedVirus = Path.Combine(BuildingBlocks.Common.Misc.ApplicationRootFolderPath, destinationFolderForDetectedVirus.TrimStart('\\'));
									}
									destinationFolderForDetectedVirus = destinationFolderForDetectedVirus.Replace("{0}", nowUniqueString);
									string destinationFilePath = Path.Combine(destinationFolderForDetectedVirus, nowUniqueString+" From "+sendersEMailAddress.ToNonNullString("null")+" to "+receiversEMailAddress.ToNonNullString("null")+" "+subject.ReplaceNonAlphaNumericCharacters("_").FirstXChars(100, "... ")+".eml");
									log.AddLine("destinationFilePath=" + destinationFilePath);
									if (!Directory.Exists(Path.GetDirectoryName(destinationFilePath)))
									{
										using (log.LogScope("Creating directory..."))
										{
											Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
										}
									}

									using (log.LogScope("Performing copy..."))
									{
										File.Copy(filePath, destinationFilePath, true);
									}

                                    try
                                    {
										if(isPlainText){
                                            File.WriteAllText(Path.ChangeExtension(destinationFilePath, ".txt"), eMailHeadingAndBody);
                                        }
                                        else{
                                            File.WriteAllText(Path.ChangeExtension(destinationFilePath, ".htm"), eMailHeadingAndBody);
                                        }											
                                    }
                                    catch(Exception)
                                    {
                                    }

									try
									{
										File.WriteAllText(Path.ChangeExtension(destinationFilePath,".log"), log.ToString());
									}
									catch (Exception)
									{
									}
                                }
							}
						}
						finally
						{
							try
							{
								using (log.LogScope("Cleaning up cloned file/its folder..."))
								{
									if (shouldDeleteClonedEmlFileFolderAfterProcessing)
									{
										Directory.Delete(Path.GetDirectoryName(filePath), true);
									}
									else if (shouldDeleteClonedEmlFileAfterProcessing)
									{
										File.Delete(filePath);
									}
									else
									{
										try
										{
											File.WriteAllText(Path.Combine(Path.GetDirectoryName(filePath),nowUniqueString+".log"), log.ToString());
										}
										catch (Exception)
										{
										}
									}
								}
							}
							catch (Exception e)
							{
								log.AddLine("Couldn't delete cloned file/its folder. Error:" + e.Message);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				log.AddLine("Error:"+e.Message);
				e = BuildingBlocks.Common.Misc.GetDeepestException(e);
				log.AddLine("Deepest exception:"+e.Message);
				log.AddLine("StackTrace:"+e.StackTrace);
				r = whichNumberToReturnIfErrorOccured;
			}

			log.AddLine("Exit code: "+r);
			return r.Value;
		}

        private static List<string> LoadListOfStringsFromCsvValueOrFile(string v, CustomTraceLog log)
        {
            List<string> r;
            if(v.IsNOTNullOrWhiteSpace())
            {
				if(v.StartsWith("Load from:"))
				{
					v = v.Substring("Load from:".Length).Trim();
					log.AddLine("Loading values from file: " + v);
					v = File.ReadAllText(v).Trim();
				}
         
				//separator char is the one that occurs most often in the value
				Dictionary<char, int> separatorCounts = new Dictionary<char, int>();
				separatorCounts.Add(',', v.Count(c => c == ','));
				separatorCounts.Add(';', v.Count(c => c == ';'));
				separatorCounts.Add('|', v.Count(c => c == '|'));
				separatorCounts.Add('\n', v.Count(c => c == '\n'));
				separatorCounts.Add('\r', v.Count(c => c == '\r'));
				char separatorChar = separatorCounts.OrderByDescending(kvp => kvp.Value).First().Key;

				if(separatorChar =='\n'){
					v = v.Replace("\r","");
				}
				if(separatorChar == '\r')
				{
					v = v.Replace("\n", "");
                }

                r = v.ParseCSV(new[] { separatorChar });
            }else{
				r = new List<string>();
			}
            
            return r;
        }

        /// <summary>
        /// Returns the part before and after the last dot in an email address or partial email address.
        /// Accepts partial addresses like "my.gmail.com" or "@gmail.com".
        /// </summary>
        private static string? GetEmailDomain(string email, string resultIfNotFoundOrInvalidEMailPart = null)
        {
			if(string.IsNullOrWhiteSpace(email))
			{
				return resultIfNotFoundOrInvalidEMailPart;
			}

            int lastDotIndex = email.LastIndexOf('.');
			if(lastDotIndex <= 0 || lastDotIndex == email.Length - 1)
			{
				return resultIfNotFoundOrInvalidEMailPart;
			}

            string before = email.Substring(0, lastDotIndex);
			if(before.IsNullOrWhiteSpace()){
				return resultIfNotFoundOrInvalidEMailPart;
            }

			int lastDotIndexBeforeLastDot = before.LastIndexOf('.');
			int lastAtIndexBeforeLastDot = before.LastIndexOf('@');
			if(Math.Max(lastDotIndexBeforeLastDot, lastAtIndexBeforeLastDot) >-1)
			{
				before = before.Substring(Math.Max(lastDotIndexBeforeLastDot+1, lastAtIndexBeforeLastDot+1));
			}
			if(before.IsNullOrWhiteSpace()){
				return resultIfNotFoundOrInvalidEMailPart;
			}

            string after = email.Substring(lastDotIndex + 1);
			if(after.IsNullOrWhiteSpace())
			{
				return resultIfNotFoundOrInvalidEMailPart;
			}

            return before+"."+after;
        }

		private static string? GetFirstMatchedDomainOrReturnNull(string emailOrDomain, List<string> domains, bool caseSensitive, List<string> domainsNeverToMatch, CustomTraceLog log)
		{
			string? r = null;
			domainsNeverToMatch = domainsNeverToMatch ?? new List<string>();

            if(emailOrDomain.IsNullOrWhiteSpace() || domains == null || domains.Count == 0)
			{
				r = null;
			}
			else
			{
				string eMailDomain = GetEmailDomain(emailOrDomain, null);
				if(eMailDomain == null)
				{
					r = null;
				}
				else
				{
					var comparer = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
					foreach(string domain in domains)
					{
						string domainToCheck = GetEmailDomain(domain, null);
						if(domainToCheck != null && eMailDomain.IndexOf(domainToCheck, comparer) == 0)
						{
							if(domainsNeverToMatch.Any(d => d.IndexOf(domainToCheck, StringComparison.OrdinalIgnoreCase) >= 0))
							{
								log.AddLine("Domain '" + domainToCheck + "' is in trustedEMailsDomainsToAllwaysIgnore. Skipping.");
								r = null;
								break;
							}
							else
							{
								r = domainToCheck;
								break;
							}
						}
					}
				}				
			}

			return r;
		}

        private static string GetFirstPhraseThatExistInTextOrReturnNull(string text, List<string> phrases, bool caseSensitive){
  		  if(text.IsNullOrWhiteSpace() || phrases == null || phrases.Count == 0)
			{
				return null;
			}
			var comparer = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            foreach(string phrase in phrases)
			{
				if(text.IndexOf(phrase, comparer) >= 0)
				{
					return phrase;
				}
			}
			return null;
        }

        private static int AiCheck(string filePath, BuildingBlocks.Logging.CustomTraceLog log, int whichNumberToReturnIfIsVirus, int whichNumberToReturnIfIsNotVirus, int whichNumberToReturnIfIsError, bool useAiSpamMailDetector, string modelFilePath)
        {
			int r = -1;

			if(!useAiSpamMailDetector)
			{
				log.AddLine("AiSpamMailDetector is disabled. Assuming clean.");
				r = whichNumberToReturnIfIsNotVirus;
			}
			else
			{
				using(log.LogScope("Performing AI spam mail detection..."))
				{
					bool isSpam;
					try
					{
						isSpam = CraftSynth.AiSpamMailDetector.Engine.Main.Detect(filePath, modelFilePath, log);
						if(isSpam)
						{
							r = whichNumberToReturnIfIsVirus;
							log.AddLine("AiSpamMailDetector result: spam.");
						}
						else if(!isSpam)
						{
							r = whichNumberToReturnIfIsNotVirus;
							log.AddLine("AiSpamMailDetector result: not spam.");
						}
					}
					catch
					{
						r = whichNumberToReturnIfIsError;
						log.AddLine("AiSpamMailDetector result: error");
					}
				}
			}

            return r;
        }

		private static void CustomTraceLogAddLinePostProcessingEvent(BuildingBlocks.Logging.CustomTraceLog log, string line, bool inNewLine, int level, string lineVersionSuitableForLineEnding, string lineVersionSuitableForNewLine)
		{
			string logFilePath = BuildingBlocks.Common.Misc.ApplicationPhysicalExeFilePathWithoutExtension + ".log";
			BuildingBlocks.IO.FileSystem.AppendFile(logFilePath, inNewLine?"\r\n"+lineVersionSuitableForNewLine:lineVersionSuitableForLineEnding, FileSystem.ConcurrencyProtectionMechanism.Lock, null);
			Console.Write((inNewLine ? "\r\n" + line : line));
		}
	}
}
