using Oz.Simulation.ClientLib.Geometries;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.ClientLib.Objects;

public class Ellipsoid : UIElement3D
{
    private static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            nameof(Model), typeof(Model3D), typeof(Ellipsoid),
            new PropertyMetadata(ModelPropertyChanged));
    private static void ModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ellipsoid = (Ellipsoid)d;
        ellipsoid.Visual3DModel = (Model3D)e.NewValue;
    }
    private Model3D Model
    {
        get => (Model3D)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }
    
    public static readonly DependencyProperty MaterialProperty = DependencyProperty.Register(
            nameof(Material), typeof(Material), typeof(Ellipsoid),
            new PropertyMetadata(new DiffuseMaterial(Brushes.Blue), PropertyChanged));
    public Material Material
    {
        get => (Material)GetValue(MaterialProperty);
        set => SetValue(MaterialProperty, value);
    }
    
    public static readonly DependencyProperty XLengthProperty = DependencyProperty.Register(
            nameof(XLength), typeof(double), typeof(Ellipsoid),
            new PropertyMetadata(1.0, PropertyChanged));
    public double XLength
    {
        get => (double)GetValue(XLengthProperty);
        set => SetValue(XLengthProperty, value);
    }
    
    public static readonly DependencyProperty YLengthProperty = DependencyProperty.Register(
            nameof(YLength), typeof(double), typeof(Ellipsoid),
            new PropertyMetadata(1.0, PropertyChanged));
    public double YLength
    {
        get => (double)GetValue(YLengthProperty);
        set => SetValue(YLengthProperty, value);
    }
// The z length of the Ellipsoid:
    public static readonly DependencyProperty ZLengthProperty = DependencyProperty.Register(
            nameof(ZLength), typeof(double), typeof(Ellipsoid),
            new PropertyMetadata(1.0, PropertyChanged));
    
    public double ZLength
    {
        get => (double)GetValue(ZLengthProperty);
        set => SetValue(ZLengthProperty, value);
    }
    
    public static readonly DependencyProperty ThetaDivProperty = DependencyProperty.Register(
            nameof(ThetaDiv), typeof(int), typeof(Ellipsoid),
            new PropertyMetadata(20, PropertyChanged));
    public int ThetaDiv
    {
        get => (int)GetValue(ThetaDivProperty);
        set => SetValue(ThetaDivProperty, value);
    }

    public static readonly DependencyProperty PhiDivProperty = DependencyProperty.Register(
            nameof(PhiDiv), typeof(int), typeof(Ellipsoid),
            new PropertyMetadata(20, PropertyChanged));
    
    public int PhiDiv
    {
        get => (int)GetValue(PhiDivProperty);
        set => SetValue(PhiDivProperty, value);
    }

    public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            nameof(Center), typeof(Point3D), typeof(Ellipsoid),
            new PropertyMetadata(new Point3D(0.0, 0.0, 0.0),
                PropertyChanged));
    public Point3D Center
    {
        get => (Point3D)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }
    private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ellipsoid = (Ellipsoid)d;
        ellipsoid.InvalidateModel();
    }
    
    protected override void OnUpdateModel()
    {
        var model = new GeometryModel3D();
        EllipsoidGeometry geometry = new() {
            XLength = XLength, 
            YLength = YLength, 
            ZLength = ZLength, 
            ThetaDiv = ThetaDiv,
            PhiDiv = PhiDiv,
            Center = Center
        };
        model.Geometry = geometry.Mesh;
        model.Material = Material;
        Model = model;
    }
}