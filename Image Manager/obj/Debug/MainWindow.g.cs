﻿#pragma checksum "..\..\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "1DF2A3A76D24B82FF873B9DAB08919D70D01A159"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Image_Manager;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
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


namespace Image_Manager {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 1 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Image_Manager.MainWindow ControlWindow;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imageViewer;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox textViewer;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox DirectoryTreeList;
        
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
            System.Uri resourceLocater = new System.Uri("/Image Manager;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\MainWindow.xaml"
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
            this.ControlWindow = ((Image_Manager.MainWindow)(target));
            
            #line 8 "..\..\MainWindow.xaml"
            this.ControlWindow.Drop += new System.Windows.DragEventHandler(this.ControlWindow_Drop);
            
            #line default
            #line hidden
            
            #line 8 "..\..\MainWindow.xaml"
            this.ControlWindow.KeyDown += new System.Windows.Input.KeyEventHandler(this.ControlWindow_KeyDown);
            
            #line default
            #line hidden
            
            #line 8 "..\..\MainWindow.xaml"
            this.ControlWindow.MouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.ControlWindow_MouseWheel);
            
            #line default
            #line hidden
            
            #line 8 "..\..\MainWindow.xaml"
            this.ControlWindow.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.ControlWindow_MouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 8 "..\..\MainWindow.xaml"
            this.ControlWindow.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.ControlWindow_MouseDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.imageViewer = ((System.Windows.Controls.Image)(target));
            return;
            case 3:
            this.textViewer = ((System.Windows.Controls.TextBox)(target));
            
            #line 15 "..\..\MainWindow.xaml"
            this.textViewer.Drop += new System.Windows.DragEventHandler(this.ControlWindow_Drop);
            
            #line default
            #line hidden
            
            #line 15 "..\..\MainWindow.xaml"
            this.textViewer.MouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.ControlWindow_MouseWheel);
            
            #line default
            #line hidden
            
            #line 15 "..\..\MainWindow.xaml"
            this.textViewer.KeyDown += new System.Windows.Input.KeyEventHandler(this.ControlWindow_KeyDown);
            
            #line default
            #line hidden
            return;
            case 4:
            this.DirectoryTreeList = ((System.Windows.Controls.ListBox)(target));
            
            #line 23 "..\..\MainWindow.xaml"
            this.DirectoryTreeList.PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.DirectoryTreeList_PreviewMouseRightButtonDown);
            
            #line default
            #line hidden
            
            #line 23 "..\..\MainWindow.xaml"
            this.DirectoryTreeList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DirectoryTreeList_SelectionChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

