//---------------------------------------------------------------------------
//
// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Limited Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/limitedpermissivelicense.mspx
// All other rights reserved.
//
// This file is part of the 3D Tools for Windows Presentation Foundation
// project.  For more information, see:
// 
// http://CodePlex.com/Wiki/View.aspx?ProjectName=3DTools
//
// The following article discusses the mechanics behind this
// trackball implementation: http://viewport3d.com/trackball.htm
//
// Reading the article is not required to use this sample code,
// but skimming it might be useful.
//
//---------------------------------------------------------------------------

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.ClientLib.Tools;

/// <summary>
///     Trackport3D loads a Model3D from a xaml file and displays it.  The user
///     may rotate the view by dragging the mouse with the left mouse button.
///     Dragging with the right mouse button will zoom in and out.
///     Trackport3D is primarily an example of how to use the Trackball utility
///     class, but it may be used as a custom control in your own applications.
/// </summary>
public partial class Trackport3D : UserControl
{
    private readonly ScreenSpaceLines3D Wireframe = new();
    private Model3D? _model;
    private readonly Trackball _trackball = new();

    private ViewMode _viewMode;

    public Trackport3D()
    {
        InitializeComponent();

        Viewport.Children.Add(Wireframe);
        Camera.Transform = _trackball.Transform;
        Headlight.Transform = _trackball.Transform;
    }

    public Color HeadlightColor
    {
        get => Headlight.Color;
        set => Headlight.Color = value;
    }

    public Color AmbientLightColor
    {
        get => AmbientLight.Color;
        set => AmbientLight.Color = value;
    }

    public ViewMode ViewMode
    {
        get => _viewMode;
        set
        {
            _viewMode = value;
            SetupScene();
        }
    }

    /// <summary>
    ///     Loads and displays the given Xaml file.  Expects the root of
    ///     the Xaml file to be a Model3D.
    /// </summary>
    public void LoadModel(Stream fileStream)
    {
        _model = (Model3D)XamlReader.Load(fileStream);

        SetupScene();
    }

    private void SetupScene()
    {
        switch (ViewMode)
        {
            case ViewMode.Solid:
                Root.Content = _model;
                Wireframe.Points.Clear();
                break;

            case ViewMode.Wireframe:
                Root.Content = null;
                Wireframe.MakeWireframe(_model);
                break;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e) =>
        // Viewport3Ds only raise events when the mouse is over the rendered 3D geometry.
        // In order to capture events whenever the mouse is over the client are we use a
        // same sized transparent Border positioned on top of the Viewport3D.
        _trackball.EventSource = CaptureBorder;
}