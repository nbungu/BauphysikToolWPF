using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.CustomControls;
using BT.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BauphysikToolWPF.Models.Domain.Helper
{
    /*
    
    Use this for Demo-Version -> cannot share btk files between machines!

    HMAC (symmetric cryptography) guarantees integrity and authenticity, but only works if both parties share the same key:
    Cannot send files and open on other machine unless they have the same key.
    
    The hmacKey parameter is the secret key used to sign and verify your project files. You don’t want to re-generate it every time (that would make old files unreadable). Instead, you:

    Generate a key once (first run, installer, or setup).

    Store it securely (DPAPI, Keychain, environment variable, etc.).

    Load it at runtime and pass it to both SaveProjectToFileSecure and GetProjectFromFileSecure.

     */


    /*

    public-key signatures (better for sharing): use digital signatures with asymmetric cryptography:

    You (sender) sign the project with your private key.

    Your friend verifies it with your public key (which is safe to share).

    */


    public static class ProjectSerializer
    {
        public static void SaveProjectToFile(Project? project, string filePath)
        {
            if (project is null || string.IsNullOrEmpty(filePath))
            {
                Logger.LogError("Project or file path is null or empty. Cannot save project.");
                return;
            }
            try
            {
                Logger.LogInfo($"Start saving project to file: {PathService.MaskPath(filePath)}");
                //var options = new JsonSerializerOptions
                //{
                //    WriteIndented = true,
                //    ReferenceHandler = ReferenceHandler.Preserve,
                //    DefaultIgnoreCondition = JsonIgnoreCondition.Never, // Include all values
                //    //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault, // omits values that are default (0, null, false, etc.)
                //};

                //string jsonString = JsonSerializer.Serialize(project, options);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    // Avoid ReferenceHandler.Preserve unless required
                    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                    MaxDepth = 64 // limit recursion
                    //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault, // omits values that are default (0, null, false, etc.)
                };
                string jsonString = JsonSerializer.Serialize(project, options);

                File.WriteAllText(filePath, jsonString);
                Logger.LogInfo($"Successfully saved project to file: {PathService.MaskPath(filePath)}");
                MainWindow.ShowToast($"Projekt '{Session.SelectedProject?.Name ?? string.Empty}' gespeichert unter {filePath}.", ToastType.Success);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error saving project to file: {PathService.MaskPath(filePath)}, {e.Message}");
                MainWindow.ShowToast($"Projekt '{Session.SelectedProject?.Name ?? string.Empty}' konnte nicht gespeichert werden: {e.Message}.", ToastType.Error);
            }
        }

        //public static void SaveProjectToFileHmac(Project? project, string filePath, byte[] hmacKey)
        //{
        //    if (project is null || string.IsNullOrEmpty(filePath))
        //    {
        //        Logger.LogError("Project or file path is null or empty. Cannot save project.");
        //        return;
        //    }
        //    try
        //    {
        //        Logger.LogInfo($"Start saving project to file: {PathService.MaskPath(filePath)}");

        //        var options = new JsonSerializerOptions
        //        {
        //            WriteIndented = true,
        //            // Avoid ReferenceHandler.Preserve unless required
        //            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        //            MaxDepth = 64 // limit recursion
        //            //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault, // omit values that are default (0, null, false, etc.)
        //        };
        //        string json = JsonSerializer.Serialize(project, options);

        //        // Compute signature
        //        var signatureHex = Integrity.ComputeHmacHex(json, hmacKey);

        //        // Write atomically: write temp files then move/replace
        //        string tempJsonPath = filePath + ".tmp";
        //        string tempSigPath = filePath + ".sig.tmp";
        //        File.WriteAllText(tempJsonPath, json, Encoding.UTF8);
        //        File.WriteAllText(tempSigPath, signatureHex, Encoding.UTF8);

        //        // Optionally set restrictive permissions here (platform-specific)
        //        // Then atomically replace
        //        File.Replace(tempJsonPath, filePath, null); // overwrites target atomically on Windows
        //        File.Replace(tempSigPath, filePath + ".sig", null);


        //        Logger.LogInfo($"Successfully saved project to file: {PathService.MaskPath(filePath)}");
        //        MainWindow.ShowToast($"Projekt '{Session.SelectedProject?.Name ?? string.Empty}' gespeichert unter {filePath}.", ToastType.Success);
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.LogError($"Error saving project to file: {PathService.MaskPath(filePath)}, {e.Message}");
        //        MainWindow.ShowToast($"Projekt '{Session.SelectedProject?.Name ?? string.Empty}' konnte nicht gespeichert werden: {e.Message}.", ToastType.Error);
        //    }
        //}

        public static Project GetProjectFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Logger.LogError($"File path is null or empty or file does not exist: {PathService.MaskPath(filePath)}");
                return Project.Empty;
            }
            try
            {
                Logger.LogInfo($"Start reading project from file: {PathService.MaskPath(filePath)}");
                string jsonString = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    // do NOT use ReferenceHandler.Preserve unless absolutely required
                    ReferenceHandler = ReferenceHandler.Preserve,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    MaxDepth = 64
                };
                Project? project = JsonSerializer.Deserialize<Project>(jsonString, options);
                if (project != null)
                {
                    Logger.LogInfo($"Successfully read project from file: {PathService.MaskPath(filePath)}");

                    // Validate critical fields
                    if (!Integrity.ValidateProject(project))
                    {
                        Logger.LogError("Project validation failed after deserialization.");
                        return Project.Empty;
                    }

                    project.CreatedByUser = true;
                    project.IsModified = false;
                    return project;
                }
                Logger.LogWarning($"Could not read from project file: {PathService.MaskPath(filePath)}");
                return Project.Empty;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error reading project from file: {PathService.MaskPath(filePath)}, {e.Message}");
                return Project.Empty;
            }
        }

        //public static Project GetProjectFromFileHmac(string filePath, byte[] hmacKey)
        //{
        //    if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        //    {
        //        Logger.LogError($"File path is null or empty or file does not exist: {PathService.MaskPath(filePath)}");
        //        return Project.Empty;
        //    }
        //    try
        //    {
        //        Logger.LogInfo($"Start reading project from file: {PathService.MaskPath(filePath)}");
        //        string json = File.ReadAllText(filePath, Encoding.UTF8);
        //        string sigPath = filePath + ".sig";
        //        if (!File.Exists(sigPath))
        //        {
        //            Logger.LogWarning("Signature missing for project file.");
        //            return Project.Empty;
        //        }
        //        string sigHex = File.ReadAllText(sigPath, Encoding.UTF8).Trim();

        //        // Verify HMAC
        //        if (!Integrity.VerifyHmac(json, sigHex, hmacKey))
        //        {
        //            Logger.LogError("Project file signature mismatch.");
        //            return Project.Empty;
        //        }

        //        var options = new JsonSerializerOptions
        //        {
        //            // do NOT use ReferenceHandler.Preserve unless absolutely required
        //            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        //            MaxDepth = 64
        //        };
        //        Project? project = JsonSerializer.Deserialize<Project>(json, options);

        //        if (project == null)
        //        {
        //            Logger.LogWarning("Deserialized project was null.");
        //            return Project.Empty;
        //        }

        //        // Validate critical fields
        //        if (!Integrity.ValidateProject(project))
        //        {
        //            Logger.LogError("Project validation failed after deserialization.");
        //            return Project.Empty;
        //        }

        //        project.CreatedByUser = true;
        //        project.IsModified = false;
        //        return project;
        //    }
        //    catch (JsonException je)
        //    {
        //        Logger.LogError($"JSON parse error for {PathService.MaskPath(filePath)}: {je.Message}");
        //        return Project.Empty;
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.LogError($"Error reading project from file: {PathService.MaskPath(filePath)}, {e.Message}");
        //        return Project.Empty;
        //    }
        //}
    }
}
