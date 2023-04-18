using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using EndlessRealms.Models;
using System;


namespace EndlessRealms.Gui.Component;
public partial class CharOrThingUi : UserControl
{
    private Something? _thing;
    public Something? Thing 
    {
        get => _thing;
        set
        {
            _thing = value;
            UpdateUi();
        }
    }

    private CharacterInfo? _charactor;
    public CharacterInfo? Character 
    {
        get => _charactor;
        set
        {
            _charactor = value;
            UpdateUi();
        }
    }
    
    
    public CharOrThingUi()
    {        
        InitializeComponent();
    }

    private void UpdateUi()
    {
        if(this.Thing != null) 
        {            
            this.nameText.Text = $"{this.Thing.Name}";
            this.nameText.Foreground = Brushes.LightGreen;

            this.descriptionText.Text = this.Thing.Description;
        }
        else
        {            
            this.nameText.Foreground = Brushes.Magenta;
            this.nameText.Text = this.Character!.FullName + (
                string.IsNullOrWhiteSpace(this.Character!.Title) 
                ? "" 
                : $"({this.Character.Title})");

            this.descriptionText.Text = this.Character.Appearance;
        }
    }

    public void DoAction(object sender, RoutedEventArgs e) 
    {
        ActionOn?.Invoke(this, (Thing, Character));        
    }
    
    public event EventHandler<(Something?, CharacterInfo?)>? ActionOn;
}
