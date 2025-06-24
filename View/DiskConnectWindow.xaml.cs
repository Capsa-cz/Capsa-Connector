using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Capsa_Connector.Controller.Core.Helpers;
using Capsa_Connector.Core;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.View
{
    public partial class DiskConnectWindow : Window
    {
        public string Email { get; private set; }
        public string Password { get; private set; }
        public string DiskLetter { get; private set; }
        
        public string WorkspaceName { get; private set; }
        
        public bool ButtonClicked { get; private set; }
        
        public DiskConnectWindow(string email, string worspaceName)
        {
            InitializeComponent();
            EmailTextBox.Text = email;
            WorkspaceNameBlock.Text = worspaceName;
            PopulateDiskLetters();
            ButtonClicked = false;
            PasswordBox.Focus();
        }

        private void PopulateDiskLetters()
        {
            var allDrives = Enumerable.Range('A', 26).Select(i => ((char)i) + "").ToList();
            var connectedDrives = DriveInfo.GetDrives()
                .Where(d => d.DriveType is DriveType.Fixed or DriveType.Network or DriveType.Removable or DriveType.CDRom) 
                .Select(d => d.Name.TrimEnd('\\'));
            //remove : from drive letters
            connectedDrives = connectedDrives.Select(d => d.TrimEnd(':'));
            
            var availableDrives = allDrives.Except(connectedDrives);
            
            List<String> drives = NetworkDriveHelper.GetMappedDriveLetters();
            { 
                availableDrives = availableDrives.Except(drives);
            }
            DiskLetterComboBox.ItemsSource = availableDrives;
            
            // First default selection will be S if available, otherwise first available drive
            if (availableDrives.Contains(Config.defaultWorkspaceLetter))
                DiskLetterComboBox.SelectedItem = Config.defaultWorkspaceLetter;
            else if (availableDrives.Any())
                DiskLetterComboBox.SelectedItem = availableDrives.First();
            else
                DiskLetterComboBox.SelectedItem = null;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Email = EmailTextBox.Text;
            Password = PasswordBox.Password;
            DiskLetter = DiskLetterComboBox.SelectedItem?.ToString();
            
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(DiskLetter))
            {
                MessageBox.Show("Prosíme vyplňte všechny údaje.");
                return;
            }
            
            ButtonClicked = true;
            DialogResult = true;
        }
    }
}
