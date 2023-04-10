using Avalonia.Controls;
using Avalonia.Interactivity;
using EndlessRealms.Models;
using System;

namespace EndlessRealms.Gui.Component;
public partial class DirectionUi : UserControl
{
    private DirectionInfo? _directionInfo = null;
    public DirectionUi()
    {
        InitializeComponent();
    }

    public void SetInfo(DirectionInfo directionInfo)
    {
        DataContext = _directionInfo = directionInfo;
    }

    public void OnClick(object sender, RoutedEventArgs e)
    {
        if (_directionInfo != null)
        {
            SetDirection?.Invoke(this, _directionInfo.Direction!.Value);
        }
    }

    public event EventHandler<Direction>? SetDirection;
}
