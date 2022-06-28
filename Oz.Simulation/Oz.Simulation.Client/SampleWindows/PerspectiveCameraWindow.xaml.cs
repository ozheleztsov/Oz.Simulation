using Oz.Simulation.ClientLib;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.Client.SampleWindows;

public partial class PerspectiveCameraWindow : Window
{
    public PerspectiveCameraWindow()
    {
        InitializeComponent();
        SetMatrixCamera();
    }
    
    private void BtnApply_OnClick(object sender, RoutedEventArgs e) =>
        SetMatrixCamera();

    private void SetMatrixCamera()
    {
        var cameraPosition = Point3D.Parse(tbCameraPosition.Text);
        var lookDirection = Vector3D.Parse(tbLookDirection.Text);
        var upDirection = Vector3D.Parse(tbUpDirection.Text);
        var fov = double.Parse(tbFieldOfView.Text);
        var zn = double.Parse(tbNearPlane.Text);
        var zf = double.Parse(tbFarPlane.Text);
        var aspectRatio = 1.0;
        myCameraMatrix.ViewMatrix = Utils3d.SetViewMatrix(cameraPosition, lookDirection, upDirection);
        myCameraMatrix.ProjectionMatrix = Utils3d.SetPerspectiveFov(fov, aspectRatio, zn, zf);
    }
}