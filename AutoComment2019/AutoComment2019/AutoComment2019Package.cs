﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace HKFY.AutoComment2019
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidAutoComment2019PkgString)]
    public sealed class AutoComment2019Package : Package
    {
        CommentFun _commentFun = null;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public AutoComment2019Package()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));

            _commentFun = new CommentFun();
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidAutoComment2019CmdSet, (int)PkgCmdIDList.ChangeDetail);
                MenuCommand menuItem = new MenuCommand(ChangeDetailCallBack, menuCommandID);
                mcs.AddCommand( menuItem );

                menuCommandID = new CommandID(GuidList.guidAutoComment2019CmdSet, (int)PkgCmdIDList.CppChangeHistory);
                menuItem = new MenuCommand(CppChangeHistoryCallBack, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidAutoComment2019CmdSet, (int)PkgCmdIDList.DocClass);
                menuItem = new MenuCommand(DocClassCallBack, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidAutoComment2019CmdSet, (int)PkgCmdIDList.DocFunction);
                menuItem = new MenuCommand(DocFunctionCallBack, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidAutoComment2019CmdSet, (int)PkgCmdIDList.DocMember);
                menuItem = new MenuCommand(DocMemberCallBack, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidAutoComment2019CmdSet, (int)PkgCmdIDList.DocGenList);
                menuItem = new MenuCommand(DocGenListCallBack, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidAutoComment2019CmdSet, (int)PkgCmdIDList.DocGenTable);
                menuItem = new MenuCommand(DocGenTableCallBack, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidAutoComment2019CmdSet, (int)PkgCmdIDList.DoNetChangeHistory);
                menuItem = new MenuCommand(DoNetChangeHistoryCallBack, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidAutoComment2019CmdSet, (int)PkgCmdIDList.DoNetFunction);
                menuItem = new MenuCommand(DoNetFunctionCallBack, menuCommandID);
                mcs.AddCommand(menuItem);
            }
        }
        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ChangeDetailCallBack(object sender, EventArgs e)
        {
            _commentFun.ChangeDetailCallBack();
        }

        private void CppChangeHistoryCallBack(object sender, EventArgs e)
        {
            _commentFun.CppChangeHistoryCallBack();
        }

        private void DocClassCallBack(object sender, EventArgs e)
        {
            _commentFun.DocClassCallBack();
        }

        private void DocFunctionCallBack(object sender, EventArgs e)
        {
            _commentFun.DocFunctionCallBack();
        }

        private void DocMemberCallBack(object sender, EventArgs e)
        {
            _commentFun.DocMemberCallBack();
        }

        private void DocGenListCallBack(object sender, EventArgs e)
        {
            _commentFun.DocGenListCallBack();
        }

        private void DocGenTableCallBack(object sender, EventArgs e)
        {
            _commentFun.DocGenTableCallBack();
        }

        private void DoNetChangeHistoryCallBack(object sender, EventArgs e)
        {
            _commentFun.DoNetChangeHistoryCallBack();
        }

        private void DoNetFunctionCallBack(object sender, EventArgs e)
        {
            _commentFun.DoNetFunctionCallBack();
        }

    }

}