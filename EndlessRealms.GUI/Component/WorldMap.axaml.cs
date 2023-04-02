using Avalonia;
using Avalonia.Controls;
using EndlessRealms.Core.Services;
using EndlessRealms.Models;
using JetBrains.Annotations;
using System;

namespace EndlessRealms.Gui.Component;
public partial class WorldMap : UserControl
{
    private WorldService? _worldService;
    public WorldMap()
    {
        InitializeComponent();                
    }

    public void SetWorld(WorldService worldService)
    {
        _worldService = worldService;

        UpdateMap();
    }

    private void UpdateMap()
    {
        
    }

    protected override Size ArrangeOverride(Size finalSize)
    {        
        return base.ArrangeOverride(finalSize);
    }
}
