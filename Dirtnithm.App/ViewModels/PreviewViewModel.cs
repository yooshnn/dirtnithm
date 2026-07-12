using System.Windows;
using Dirtnithm.App.Mvvm;

namespace Dirtnithm.App.ViewModels;

public class PreviewViewModel : ViewModelBase
{
    public double PreviewHeight => 120;

    private int _thresholdPercent;
    public int ThresholdPercent
    {
        get => _thresholdPercent;
        set
        {
            if (!SetProperty(ref _thresholdPercent, value)) return;
            OnPropertyChanged(nameof(ThresholdY));
        }
    }

    public double ThresholdY => ThresholdPercent / 100.0 * PreviewHeight;

    private double _leftDotY;
    public double LeftDotY { get => _leftDotY; set => SetProperty(ref _leftDotY, value); }

    private double _rightDotY;
    public double RightDotY { get => _rightDotY; set => SetProperty(ref _rightDotY, value); }

    private Visibility _leftDotVisibility = Visibility.Collapsed;
    public Visibility LeftDotVisibility { get => _leftDotVisibility; set => SetProperty(ref _leftDotVisibility, value); }

    private Visibility _rightDotVisibility = Visibility.Collapsed;
    public Visibility RightDotVisibility { get => _rightDotVisibility; set => SetProperty(ref _rightDotVisibility, value); }

    public void UpdateFromCoordinates(double? left, double? right)
    {
        UpdateDot(left, PreviewHeight, y => LeftDotY = y, v => LeftDotVisibility = v);
        UpdateDot(right, PreviewHeight, y => RightDotY = y, v => RightDotVisibility = v);
    }

    private static void UpdateDot(double? value, double height,
        Action<double> setY, Action<Visibility> setVisibility)
    {
        if (value.HasValue)
        {
            setY(value.Value * height);
            setVisibility(Visibility.Visible);
        }
        else
        {
            setVisibility(Visibility.Collapsed);
        }
    }
}