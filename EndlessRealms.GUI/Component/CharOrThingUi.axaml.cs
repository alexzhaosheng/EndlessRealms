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
            this.numberText.Text = this.Thing.Number.ToString();
            this.descriptionText.Text = $"{this.Thing.Name}";
            this.descriptionText.Foreground = Brushes.LightGreen;            
        }
        else
        {
            this.numberText.Text = "";
            this.descriptionText.Foreground = Brushes.Magenta;
            this.descriptionText.Text = this.Character!.FullName + (
                string.IsNullOrWhiteSpace(this.Character!.Title) 
                ? "" 
                : $"({this.Character.Title})"); 
        }
    }

    public void DoAction(object sender, RoutedEventArgs e) 
    {
        ActionOn?.Invoke(this, (Thing, Character));        
    }
    
    public event EventHandler<(Something?, CharacterInfo?)>? ActionOn;
}
