﻿using Somtoday2MicrosoftSchoolDataSync.Helpers;
using Somtoday2MicrosoftSchoolDataSync.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Somtoday2MicrosoftSchoolDataSync
{
    class Program
    {
        static readonly bool booleanFilterBylocation = bool.Parse(ConfigurationManager.AppSettings["BooleanFilterBylocation"]);
        static readonly bool seperateOutputDirectoryForEachLocation = bool.Parse(ConfigurationManager.AppSettings["SeperateOutputDirectoryForEachLocation"]);
        static readonly string[] includedLocationCode = ConfigurationManager.AppSettings["IncludedLocationCode"].Split(';');
        static readonly string umServiceBrinNr = ConfigurationManager.AppSettings["umServiceBrinNr"];
        static readonly string umServiceUsername = ConfigurationManager.AppSettings["umServiceUsername"];
        static readonly string umServicePassword = ConfigurationManager.AppSettings["umServicePassword"];
        static readonly string OutputDirectory = ConfigurationManager.AppSettings["OutputDirectory"].EndsWith("\\") ? ConfigurationManager.AppSettings["OutputDirectory"] : ConfigurationManager.AppSettings["OutputDirectory"] + "\\";
        static readonly int StartYear = DateTime.Now.Month < 8 ? (DateTime.Now.Year - 1) : DateTime.Now.Year;
        static readonly int EndYear = DateTime.Now.Month < 8 ? (DateTime.Now.Year) : (DateTime.Now.Year + 1);
        static readonly string umServiceSchooljaar = StartYear + "/" + EndYear;
        static readonly bool enableGuardianSync = bool.Parse(ConfigurationManager.AppSettings["EnableGuardianSync"]);

        public static EventLogHelper eh = new EventLogHelper();
        public static ServiceHelper sh = new ServiceHelper(umServiceBrinNr, umServiceUsername, umServicePassword, umServiceSchooljaar);

        public static List<VestigingLesgroepModel> vestigingLesgroepen = new List<VestigingLesgroepModel>();
        public static List<UserLesgroepModel> leerlingLesgroepen = new List<UserLesgroepModel>();
        public static List<UserLesgroepModel> docentLesgroepen = new List<UserLesgroepModel>();
        public static List<UserLesgroepModel> ouderInformatie = new List<UserLesgroepModel>();
        public static List<VestigingSDSModel> vestigingSDSList = new List<VestigingSDSModel>();


        #region sluiten van app door user
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                default:
                    eh.WriteLog("Applicatie afgebroken", EventLogEntryType.Warning, 400);
                    return false;
            }
        }
        #endregion
        static void Main(string[] args)
        {

            DateTime buildDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;

            eh.CheckEventLog();
            //sluiten van app door user
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
            //sluiten van app door user

            //Beveiliging instellen van webservice
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3 | System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11;

            SettingsHelper sh = new SettingsHelper();
            if (sh.ValidateUsernameFormat())
            {
                eh.WriteLog("Sync gestart met applicatieversie: " + buildDate.ToString("o").Split('T')[0], EventLogEntryType.Information, 100);

                StartProgram();

                Console.WriteLine("======================================");
                eh.WriteLog("Sync voltooid", EventLogEntryType.Information, 100);
                Thread.Sleep(10000);
            }
        }
        private static void StartProgram()
        {
            bool succes = false;
            for (int attempt = 0; attempt < 10; attempt++)
            {
                if (GetSomtodayData())
                {
                    attempt = 10;
                    eh.WriteLog("Download alle gegevens is gelukt!", EventLogEntryType.Information, 100);
                    succes = true;
                }
                else
                {
                    eh.WriteLog(string.Format("Download gegevens is mislukt. Over 5 minuten nogmaals proberen. Poging {0}", attempt), EventLogEntryType.Warning, 100);
                    Thread.Sleep(300000);
                }
            }

            if (succes)
            {
                vestigingSDSList = GetVestigingSDS();
                SaveToDisk(vestigingSDSList);
            }
            else
            {
                vestigingSDSList = GetVestigingSDSFromDisk();
            }

            //SaveToCloud();

        }

        private static List<VestigingSDSModel> GetVestigingSDSFromDisk()
        {
            FileHelper fh = new FileHelper();

            vestigingSDSList = fh.GetVestigingSDSModels(OutputDirectory);
            return null;
        }

        private static bool GetSomtodayData()
        {
            bool success = true;

            List<UmService.wisVestiging> vestigingenList = sh.GetVestigingen(booleanFilterBylocation, includedLocationCode);
            if (vestigingenList?.Count == 0)
            {
                eh.WriteLog("Geen vestigingen kunnen downloaden", EventLogEntryType.Warning, 100);
                return false;
            }

            vestigingLesgroepen = new List<VestigingLesgroepModel>();
            vestigingLesgroepen = sh.GetLesgroepVestiging(vestigingenList);
            if (vestigingLesgroepen?.Count == 0)
            {
                eh.WriteLog("Geen lesgroepen kunnen downloaden", EventLogEntryType.Warning, 100);
                return false;
            }

            leerlingLesgroepen = new List<UserLesgroepModel>();
            leerlingLesgroepen = sh.GetStudentInfo(vestigingLesgroepen);
            if (leerlingLesgroepen?.Count == 0)
            {
                eh.WriteLog("Geen leerlinginformatie kunnen downloaden", EventLogEntryType.Warning, 100);
                return false;
            }

            docentLesgroepen = new List<UserLesgroepModel>();
            docentLesgroepen = sh.GetTeacherInfo(vestigingLesgroepen);
            if (docentLesgroepen?.Count == 0)
            {
                eh.WriteLog("Geen docentinformatie kunnen downloaden", EventLogEntryType.Warning, 100);
                return false;
            }

            ouderInformatie = new List<UserLesgroepModel>();
            ouderInformatie = GetOuderInformatie(vestigingLesgroepen);
            if (ouderInformatie?.Count == 0 && enableGuardianSync)
            {
                eh.WriteLog("Geen ouderinformatie kunnen downloaden", EventLogEntryType.Warning, 100);
                return false;
            }
            return success;
        }

        private static void SaveToDisk(List<VestigingSDSModel> vestigingSDSList)
        {
            FileHelper fh = new FileHelper();
            if (vestigingSDSList.Count == 1 && seperateOutputDirectoryForEachLocation)
            {
                fh.WriteSDStoFiles(OutputDirectory + vestigingSDSList[0].Vestigingsafkorting + "\\", vestigingSDSList[0].SDS);
            }
            else if (vestigingSDSList.Count == 1 && !seperateOutputDirectoryForEachLocation)
            {
                fh.WriteSDStoFiles(OutputDirectory, vestigingSDSList[0].SDS);
            }
            else
            {
                foreach (var vestigingSDS in vestigingSDSList)
                {
                    fh.WriteSDStoFiles(OutputDirectory + vestigingSDS.Vestigingsafkorting + "\\", vestigingSDS.SDS);
                }
            }
        }

        private static List<VestigingSDSModel> GetVestigingSDS()
        {
            List<VestigingSDSModel> vSDS = new List<VestigingSDSModel>();

            if (seperateOutputDirectoryForEachLocation)
            {
                foreach (VestigingLesgroepModel vestigingLesgroep in vestigingLesgroepen)
                {
                    SDScsvHelper lh = new SDScsvHelper(
                        vestigingLesgroepen.Where(v => v.Vestiging == vestigingLesgroep.Vestiging).ToList(),
                        docentLesgroepen.Where(v => v.VestigingLesgroep.Vestiging == vestigingLesgroep.Vestiging).ToList(),
                        leerlingLesgroepen.Where(v => v.VestigingLesgroep.Vestiging == vestigingLesgroep.Vestiging).ToList(),
                        ouderInformatie.Where(v => v.VestigingLesgroep.Vestiging == vestigingLesgroep.Vestiging).ToList());

                    vSDS.Add(new VestigingSDSModel
                    {
                        Vestigingsafkorting = vestigingLesgroep.Vestiging.afkorting,
                        SDS = lh.GetSDScsv()
                    });

                }
            }
            else
            {
                SDScsvHelper lh = new SDScsvHelper(vestigingLesgroepen, docentLesgroepen, leerlingLesgroepen, ouderInformatie);
                SDScsv schoolDataSyncCSV = lh.GetSDScsv();
                vSDS.Add(new VestigingSDSModel
                {
                    Vestigingsafkorting = null,
                    SDS = lh.GetSDScsv()
                });
            }
            return vSDS;
        }

        private static List<UserLesgroepModel> GetOuderInformatie(List<VestigingLesgroepModel> vestigingLesgroepen)
        {
            if (!enableGuardianSync)
            {
                return new List<UserLesgroepModel>();
            }
            return sh.GetGuardianInfo(vestigingLesgroepen);
        }
    }
}
