namespace AnalysisExtension
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class CreateRuleToolWindow1Control : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRuleToolWindow1Control"/> class.
        /// </summary>

        public CreateRuleToolWindow1Control()
        {
            this.InitializeComponent();
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newWindowHeight = e.NewSize.Height;
            double newWindowWidth = e.NewSize.Width;
            int padding = 20;
            ruleCreateStackPanel.Width = newWindowWidth - 60;
            ruleCreateStackPanel.Height = newWindowHeight - padding;
            ruleBefore.Height = ruleCreateStackPanel.Height / 2 - padding;
            ruleBefore.Width = ruleCreateStackPanel.Width - padding;
            ruleAfter.Height = ruleCreateStackPanel.Height / 2 - padding;
            ruleAfter.Width = ruleCreateStackPanel.Width - padding;
        }

        private void RuleBefore_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            StaticValue.OnPreviewMouseWheelListener(sender,e);
        }

        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            StaticValue.CloseWindow(this);
        }
    }
}