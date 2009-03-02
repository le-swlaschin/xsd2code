using System;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;
using Xsd2Code.Library;

namespace Xsd2Code.Addin
{
    /// <summary>
    /// Connect class implements Xsd2Code Visual Studio add-in
    /// </summary>
    /// <remarks>
    /// Revision history:
    /// 
    ///     Modified 2009-02-20 by Ruslan Urban
    ///     Changed signature of the GeneratorFacade class constructor
    /// 
    /// </remarks>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        #region Fields

        /// <summary>
        /// interface AddIn
        /// </summary>
        private AddIn addInInstanceField;

        /// <summary>
        /// interface DTE2
        /// </summary>
        private DTE2 applicationObjectField;

        /// <summary>
        /// EnvDTE command
        /// </summary>
        private Command commandField;

        /// <summary>
        /// CommandBar command
        /// </summary>
        private CommandBar projectCmdBarField;

        #endregion

        #region IDTCommandTarget Members

        /// <summary>
        /// Execute Addin Command
        /// </summary>
        /// <param name="cmdName">Command name</param>
        /// <param name="executeOption">Execute options</param>
        /// <param name="variantIn">object variant in</param>
        /// <param name="variantOut">object variant out</param>
        /// <param name="handled">Handled true or false</param>
        public void Exec(string cmdName, vsCommandExecOption executeOption, ref object variantIn, ref object variantOut,
                         ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (cmdName == "Xsd2Code.Addin.Connect.Xsd2CodeAddin")
                {
                    UIHierarchy uIH = this.applicationObjectField.ToolWindows.SolutionExplorer;
                    var item = (UIHierarchyItem) ((Array) uIH.SelectedItems).GetValue(0);

#pragma warning disable 168
                    UIHierarchyItems items = item.UIHierarchyItems;
#pragma warning restore 168
                    
                    ProjectItem proitem = uIH.DTE.SelectedItems.Item(1).ProjectItem;
                    Project proj = proitem.ContainingProject;

#pragma warning disable 168
                    string lg = proj.CodeModel.Language;
#pragma warning restore 168

                    string fileName = proitem.get_FileNames(0);

                    try
                    {
                        proitem.Save(fileName);
                    }
                    catch (Exception)
                    {}

                    var frm = new FormOption();
                    frm.Init(fileName);
                    
                    var result = frm.ShowDialog();

                    var generatorParams = frm.GeneratorParams.Clone();
                    generatorParams.InputFilePath = fileName;
                    
                    var gen = new GeneratorFacade(generatorParams);

                    // Close file if open in IDE
                    ProjectItem projElmts;
                    bool found = this.FindInProject(proj.ProjectItems, gen.GeneratorParams.OutputFilePath, out projElmts);
                    if (found)
                    {
                        Window window = projElmts.Open(Constants.vsViewKindCode);
                        window.Close(vsSaveChanges.vsSaveChangesNo);
                    }

                    if (fileName.Length > 0)
                    {
                        if (result == DialogResult.OK)
                        {

                            var generateResult = gen.Generate();
                            var outputFileName = generateResult.Entity;

                            if (!generateResult.Success)
                                MessageBox.Show(generateResult.Messages.ToString(), "XSD2Code", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            else
                            {
                                if (!found)
                                {
                                    projElmts = proitem.Collection.AddFromFile(outputFileName);
                                }

                                Window window = projElmts.Open(Constants.vsViewKindCode);
                                window.Activate();
                                window.SetFocus();

                                try
                                {
                                    this.applicationObjectField.DTE.ExecuteCommand("Edit.RemoveAndSort", "");
                                    this.applicationObjectField.DTE.ExecuteCommand("Edit.FormatDocument", "");
                                }
                                catch (Exception)
                                {}
                            }
                        }
                    }

                    handled = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Returns the current status (enabled, disabled, hidden, and so forth) of the specified named command
        /// </summary>
        /// <param name="cmdName">Command Name</param>
        /// <param name="neededText">Constant specifying if information is returned from the check</param>
        /// <param name="statusOption">The current status of the command</param>
        /// <param name="commandText">Command text value</param>
        public void QueryStatus(string cmdName, vsCommandStatusTextWanted neededText, ref vsCommandStatus statusOption,
                                ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                statusOption = vsCommandStatus.vsCommandStatusUnsupported;
                if (cmdName == "Xsd2Code.Addin.Connect.Xsd2CodeAddin")
                {
                    UIHierarchy uIH = this.applicationObjectField.ToolWindows.SolutionExplorer;
                    var item = (UIHierarchyItem) ((Array) uIH.SelectedItems).GetValue(0);
                    if (item.Name.ToLower().EndsWith(".xsd"))
                    {
                        statusOption = vsCommandStatus.vsCommandStatusSupported;
                        statusOption |= vsCommandStatus.vsCommandStatusEnabled;
                    }
                }
            }
        }

        #endregion

        #region IDTExtensibility2 Members

        /// <summary>
        /// Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.
        /// </summary>
        /// <param name="application">Root object of the host application.</param>
        /// <param name="connectMode">Describes how the Add-in is being loaded.</param>
        /// <param name="addInInst">Object representing this Add-in.</param>
        /// <param name="custom">Array of custom params</param>
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            this.applicationObjectField = (DTE2) application;
            this.addInInstanceField = (AddIn) addInInst;


            // Only execute the startup code if the connection mode is a startup mode
            if (connectMode == ext_ConnectMode.ext_cm_Startup)
            {
                var contextGUIDS = new object[] {};

                try
                {
                    // Create a Command with name SolnExplContextMenuCS and then add it to the "Item" menubar for the SolutionExplorer
                    this.commandField = this.applicationObjectField.Commands.AddNamedCommand(this.addInInstanceField,
                                                                                             "Xsd2CodeAddin",
                                                                                             "Run Xsd2Code generation",
                                                                                             "Xsd2Code", true, 372,
                                                                                             ref contextGUIDS,
                                                                                             (int)
                                                                                             vsCommandStatus.
                                                                                                 vsCommandStatusSupported
                                                                                             +
                                                                                             (int)
                                                                                             vsCommandStatus.
                                                                                                 vsCommandStatusEnabled);
                    this.projectCmdBarField = ((CommandBars) this.applicationObjectField.CommandBars)["Item"];

                    if (this.projectCmdBarField == null)
                    {
                        MessageBox.Show("Cannot get the Project menubar", "Error", MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                    }
                    else
                        this.commandField.AddControl(this.projectCmdBarField, 1);
                }
                catch (Exception)
                {}
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param name='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param name='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {}

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param name='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {}

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param name='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {}

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param name='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {}

        #endregion

        /// <summary>
        /// Recursive search in project solution.
        /// </summary>
        /// <param name="projectItems">Root projectItems</param>
        /// <param name="filename">Full path of search element</param>
        /// <param name="item">output ProjectItem</param>
        /// <returns>true if found else false</returns>
        private bool FindInProject(ProjectItems projectItems, string filename, out ProjectItem item)
        {
            item = null;
            if (projectItems == null)
                return false;

            foreach (ProjectItem projElmts in projectItems)
            {
                if (projElmts.get_FileNames(0) == filename)
                {
                    item = projElmts;
                    return true;
                }
                
                if (this.FindInProject(projElmts.ProjectItems, filename, out item))
                    return true;
            }

            return false;
        }
    }
}