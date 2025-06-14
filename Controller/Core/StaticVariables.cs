using Capsa_Connector.Model;
using Capsa_Connector.View;
using Capsa_Connector.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Capsa_Connector.Core
{
    class StaticVariables
    {
        public static MainViewModel? mainViewModel { get; set; }
        public static string capsaUrl = "";

        public static bool uploadActive = false;

        // Date program info -> not using them yet
        // TODO: Dodělat dolní sekci vývojáře, kde se to bude dát přečíst
        public static DateTime pairCommandSent;
        public static DateTime lastPing;

        // Dashboard variables
        public static BlockingCollection<ActiveFile> activeFiles = new BlockingCollection<ActiveFile>();

        // Task threads - killing them is possible
        public static ObservableCollection<TaskThreadCommandsStructure> taskThreadCommands = new ObservableCollection<TaskThreadCommandsStructure>();

        public static string? appVersion;

        // Only works for manual locking (automatic locking is not here)
        public static List<ActiveFile> editWindowElements = new List<ActiveFile>();
        public static StartEditWindow? startEditWindow = null;
        public static UploadErrorWindow? uploadErrorWindow = null;
    }
}
