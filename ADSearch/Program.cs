using System;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using CommandLine;

namespace ADSearch {

    class Program {

        static void Main(string[] args) {
            var cmdOptions = Parser.Default.ParseArguments<Options>(args);
            cmdOptions.WithParsed(
                options => {
                    Entry(options);
                });
        }

        static void PrintBanner() {
            Console.WriteLine(@"
    ___    ____  _____                 __  
   /   |  / __ \/ ___/___  ____ ______/ /_ 
  / /| | / / / /\__ \/ _ \/ __ `/ ___/ __ \
 / ___ |/ /_/ /___/ /  __/ /_/ / /__/ / / /
/_/  |_/_____//____/\___/\__,_/\___/_/ /_/ 
                                           
Twitter: @tomcarver_
GitHub: @tomcarver16
            ");
        }

        static void Entry(Options options) {
            ADWrapper AD;
            ConsoleFileOutput cf = null;

            if (!options.SupressBanner) {
                PrintBanner();
            }

            if (options.Output != null) {
                cf = new ConsoleFileOutput(options.Output, Console.Out);
                Console.SetOut(cf);
            }

            if (options.Insecure) {
                options.Port = "389";
            }

            if (options.Hostname == null && options.Domain != null) {
                //No IP but domains set
                AD = new ADWrapper(options.Domain, options.Username, options.Password, options.Insecure, options.JsonOut);
            } else if (options.Hostname != null && options.Domain != null) {
                //This requires the domain so it can be converted into a valid LDAP URI
                AD = new ADWrapper(options.Domain, options.Hostname, options.Port, options.Username, options.Password, options.Insecure, options.JsonOut);
            } else {
                //When no domain is supplied it has to be done locally even if the ip is set otherwise the bind won't work
                OutputFormatting.PrintVerbose("No domain supplied. This PC's domain will be used instead");
                AD = new ADWrapper(options.JsonOut);
            }

            if (options.Attribtues != null && !options.Full) {
                AD.attributesToReturn = options.Attribtues.Split(',');
            }

            OutputFormatting.PrintVerbose(AD.LDAP_URI);

            if (options.Groups) {
                OutputFormatting.PrintVerbose("ALL GROUPS: ");
                AD.ListAllGroups(AD.GetAllGroups(), options.Full);
            }
            
            if (options.Users) {
                OutputFormatting.PrintVerbose("ALL USERS: ");
                AD.ListAllUsers(AD.GetAllUsers(), options.Full);
            }

            if (options.Computers) {
                OutputFormatting.PrintVerbose("ALL COMPUTERS: ");
                AD.ListAllComputers(AD.GetAllComputers(), options.Full);
            }

            if (options.Search != null) {
                OutputFormatting.PrintVerbose("CUSTOM SEARCH: ");
                AD.ListCustomSearch(AD.GetCustomSearch(options.Search), options.Full);
            }

            if (options.Spns) {
                OutputFormatting.PrintVerbose("ALL SPNS: ");
                AD.ListAllSpns(AD.GetAllSpns(), options.Full);
            }

            if (options.DomainAdmins) {
                OutputFormatting.PrintVerbose("ALL DOMAIN ADMINS: ");
                AD.ListAllDomainAdmins(AD.GetAllDomainAdmins(), options.Full);
            }

            if (options.Output != null) {
                //Close out file handle
                cf.Close();
            }
        }

        private static void GetHelp() {
            Console.WriteLine("Please enter valid arguments");
        }
    }
}
