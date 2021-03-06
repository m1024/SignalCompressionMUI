﻿#pragma checksum "..\..\..\Views\OxyPlotView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "379CC2F7301BAC9304C3AF3A437EAF2B"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Converters;
using FirstFloor.ModernUI.Windows.Navigation;
using OxyPlot.Wpf;
using SignalCompressionMUI.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SignalCompressionMUI.Views {
    
    
    /// <summary>
    /// OxyPlotView
    /// </summary>
    public partial class OxyPlotView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 35 "..\..\..\Views\OxyPlotView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.Plot Plot;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\Views\OxyPlotView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries SourseLineSeries;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\Views\OxyPlotView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries NewLineSeries;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SignalCompressionMUI;component/views/oxyplotview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Views\OxyPlotView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 28 "..\..\..\Views\OxyPlotView.xaml"
            ((FirstFloor.ModernUI.Windows.Controls.ModernButton)(target)).Click += new System.Windows.RoutedEventHandler(this.ToPdf_OnClick);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 29 "..\..\..\Views\OxyPlotView.xaml"
            ((FirstFloor.ModernUI.Windows.Controls.ModernButton)(target)).Click += new System.Windows.RoutedEventHandler(this.ToPng_OnClick);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 30 "..\..\..\Views\OxyPlotView.xaml"
            ((FirstFloor.ModernUI.Windows.Controls.ModernButton)(target)).Click += new System.Windows.RoutedEventHandler(this.ToSvg_OnClick);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 31 "..\..\..\Views\OxyPlotView.xaml"
            ((FirstFloor.ModernUI.Windows.Controls.ModernButton)(target)).Click += new System.Windows.RoutedEventHandler(this.ToClipboard_OnClick);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 32 "..\..\..\Views\OxyPlotView.xaml"
            ((FirstFloor.ModernUI.Windows.Controls.ModernToggleButton)(target)).Click += new System.Windows.RoutedEventHandler(this.SourseGraph_OnClick);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 33 "..\..\..\Views\OxyPlotView.xaml"
            ((FirstFloor.ModernUI.Windows.Controls.ModernToggleButton)(target)).Click += new System.Windows.RoutedEventHandler(this.NewGraph_OnClick);
            
            #line default
            #line hidden
            return;
            case 7:
            this.Plot = ((OxyPlot.Wpf.Plot)(target));
            return;
            case 8:
            this.SourseLineSeries = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 9:
            this.NewLineSeries = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

