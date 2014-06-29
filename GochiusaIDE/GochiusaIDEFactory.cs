using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace GochiusaIDE
{
    #region Adornment Factory
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class PurpleBoxAdornmentFactory : IWpfTextViewCreationListener
    {
        private GochiusaIDE gochiusaIDE;

        [Import(typeof(SVsServiceProvider))]
        internal SVsServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Defines the adornment layer for the scarlet adornment. This layer is ordered 
        /// after the selection layer in the Z-order
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("GochiusaIDE")]
        [Order(After = PredefinedAdornmentLayers.DifferenceSpace,Before = PredefinedAdornmentLayers.Outlining)]
        public AdornmentLayerDefinition editorAdornmentLayer = null;

        [Export(typeof(AdornmentLayerDefinition))]
        [Name("GochiusaIDE_Background")]
        [Order(Before = "GochiusaIDE")]
        public AdornmentLayerDefinition editorAdornmentLayer_background = null;

        [Export(typeof(AdornmentLayerDefinition))]
        [Name("GochiusaIDE_Build")]
        [Order(After = PredefinedAdornmentLayers.Caret)]
        public AdornmentLayerDefinition editorAdornmentLayer_build = null;


        private void buildDoneAction(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
        {
            if (this.gochiusaIDE != null)
            {
                this.gochiusaIDE.buildDoneAction(Success);
            }
        }
        /*
        private void buildBeginAction(string Project, string ProjectConfig, string Platform, string SolutionConfig)
        {
            if (this.gochiusaIDE != null)
            {
                this.gochiusaIDE.buildBeginAction();
            }
        }
        */
        private void buildBeginAction2(vsBuildScope Scope, vsBuildAction Action)
        {
            if (Action == vsBuildAction.vsBuildActionBuild || Action == vsBuildAction.vsBuildActionRebuildAll)
            {
                this.gochiusaIDE.buildBeginAction(false);
            }
            else if(Action == vsBuildAction.vsBuildActionClean)
            {
                this.gochiusaIDE.buildBeginAction(true);
            }
        }

        /// <summary>
        /// Instantiates a GochiusaIDE manager when a textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView)
        { 
            gochiusaIDE = new GochiusaIDE(textView);
            DTE service = (DTE)this.ServiceProvider.GetService(typeof(DTE));
            
            ComAwareEventInfo com = new ComAwareEventInfo(typeof(_dispBuildEvents_Event), "OnBuildProjConfigDone");
            com.AddEventHandler(service.Events.BuildEvents, new _dispBuildEvents_OnBuildProjConfigDoneEventHandler(this.buildDoneAction));
            
            //ComAwareEventInfo com2 = new ComAwareEventInfo(typeof(_dispBuildEvents_Event), "OnBuildProjConfigBegin");
            //com2.AddEventHandler(service.Events.BuildEvents, new _dispBuildEvents_OnBuildProjConfigBeginEventHandler(this.buildBeginAction));

            ComAwareEventInfo com2 = new ComAwareEventInfo(typeof(_dispBuildEvents_Event), "OnBuildBegin");
            com2.AddEventHandler(service.Events.BuildEvents, new _dispBuildEvents_OnBuildBeginEventHandler(this.buildBeginAction2));

        }

        

        
    }
    #endregion //Adornment Factory
}
