﻿using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MadsKristensen.ToggleFeatures
{
    sealed class GraphProviderCommand
    {
        Package _package;
        const string _dword = "UseSolutionNavigatorGraphProvider";

        GraphProviderCommand(Package package)
        {
            ServiceProvider = _package = package;

            var service = (OleMenuCommandService)ServiceProvider.GetService(typeof(IMenuCommandService));

            var cmdId = new CommandID(PackageGuids.guidToggleFeaturesCmdSet, PackageIds.ToggleGraphProvider);
            var button = new OleMenuCommand(ToggleFeature, cmdId);
            button.BeforeQueryStatus += BeforeQueryStatus;
            service.AddCommand(button);
        }

        public static GraphProviderCommand Instance { get; private set; }

        IServiceProvider ServiceProvider { get; }

        public static void Initialize(Package package)
        {
            Instance = new GraphProviderCommand(package);
        }

        void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;

            var rawValue = _package.UserRegistryRoot.GetValue(_dword, 1);
            int value;

            int.TryParse(rawValue.ToString(), out value);

            button.Checked = value == 0;
        }

        void ToggleFeature(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;

            if (!UserWantsToProceed())
                return;

            if (!button.Checked) // User checked the button
            {
                _package.UserRegistryRoot.DeleteValue(_dword);
            }
            else
            {
                _package.UserRegistryRoot.SetValue(_dword, 0);
            }

            RestartVS();
        }

        void RestartVS()
        {
            IVsShell4 shell = (IVsShell4)ServiceProvider.GetService(typeof(SVsShell));
            shell.Restart((uint)__VSRESTARTTYPE.RESTART_Normal);
        }

        static bool UserWantsToProceed()
        {
            string text = "This will toggle the feature and restart Visual Studio.\r\rDo you wish to continue?";
            return MessageBox.Show(text, VSPackage.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}