using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;

namespace RawInputDataSinkServer
{
    internal class Program
    {
        private const string HelpHeading = "RawInputDataSinkServer - Sends raw keyboard events from the system as an UDP broadcast message";
        private const string HelpCopyright = "Copyright (C) 2023 Calle Lindquist";
        private const string LicenseFolderName = "license";

        public static void Main(string[] args)
        {
            Parser parser = new(x =>
            {
                x.HelpWriter = null;
                x.AutoHelp = true;
                x.AutoVersion = true;
            });

            var result = parser.ParseArguments<RawInputDataSinkServerArguments>(args);
            result.WithParsed(RunProgram)
                  .WithNotParsed(err => RunErrorFlow(result, err));
        }

        private static void RunProgram(RawInputDataSinkServerArguments args)
        {
            if (args.PrintThirdPartyLicenses)
            {
                if (!TryGetLicenseFolder(out var licenseFolderPath))
                    return;

                var files = Directory.GetFiles(licenseFolderPath);
                if (files == default || !files.Any())
                {
                    Console.WriteLine("No third-party licenses found");
                    return;
                }

                foreach (var licenseFile in files)
                    PrintLicenseFile(licenseFile);
                return;
            }
            EntryPoint.RunProgram(args);
        }

        private static bool TryGetLicenseFolder(out string licenseFolderPath)
        {
            licenseFolderPath = string.Empty;
            var folder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? string.Empty);
            if (folder == default)
            {
                Console.WriteLine("[ERROR] Could not get folder name of executable");
                return false;
            }

            var directiories = Directory.GetDirectories(folder);
            if (directiories == default || directiories.Length == 0)
            {
                Console.WriteLine("[ERROR] Could not list folders in executable folder");
                return false;
            }

            licenseFolderPath = directiories.FirstOrDefault(x => Path.GetRelativePath(folder, x).Equals(LicenseFolderName, StringComparison.Ordinal)) ?? string.Empty;
            if (string.IsNullOrEmpty(licenseFolderPath))
                Console.WriteLine($"[ERROR] Could not find license folder (looking for '{LicenseFolderName}' in '{folder}')");
            return !string.IsNullOrEmpty(licenseFolderPath);
        }

        private static void PrintLicenseFile(string licenseFile)
        {
            var text = File.ReadAllText(licenseFile);
            Console.WriteLine($"{Path.GetFileNameWithoutExtension(licenseFile)}\n");
            Console.WriteLine(text);
            Console.WriteLine("-------------------------------------------------\n");
        }

        private static void RunErrorFlow(ParserResult<RawInputDataSinkServerArguments> result, IEnumerable<Error> errors)
        {
            var isVersionRequest = errors.FirstOrDefault(x => x.Tag == ErrorType.VersionRequestedError) != default;
            var isHelpRequest = errors.FirstOrDefault(x => x.Tag == ErrorType.HelpRequestedError) != default ||
                                errors.FirstOrDefault(x => x.Tag == ErrorType.HelpVerbRequestedError) != default;

            var output = string.Empty;
            if (isHelpRequest)
            {
                output = HelpText.AutoBuild(result,
                h =>
                {
                    h.Heading = HelpHeading;
                    h.Copyright = HelpCopyright;
                    return h;
                });
            }
            else if (isVersionRequest)
            {
                output = "Version 0.1";
            }
            else
            {
                output = errors.Count() > 1 ? "ERRORS:\n" : "ERROR:\n";
                foreach (var error in errors)
                    output += '\t' + GetErrorText(error) + '\n';
            }
            Console.WriteLine(output);
        }

        private static string GetErrorText(Error error)
        {
            return error switch
            {
                MissingValueOptionError missingValueError => $"Value for argument '{missingValueError.NameInfo.NameText}' is missing",
                UnknownOptionError unknownOptionError => $"Argument '{unknownOptionError.Token}' is unknown",
                MissingRequiredOptionError missingRequiredOption => $"A required option ('{missingRequiredOption.NameInfo.LongName}') is missing value",
                SetValueExceptionError setValueExceptionError => $"Could not set value for argument '{setValueExceptionError.NameInfo.NameText}': {setValueExceptionError.Exception.Message}",
                BadFormatConversionError badFormatConversionError => $"Argument '{badFormatConversionError.NameInfo.NameText}' has bad format",
                _ => $"Argument parsing failed: '{error.Tag}'"
            };
        }
    }
}
