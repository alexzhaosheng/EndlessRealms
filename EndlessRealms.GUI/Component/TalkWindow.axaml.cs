using Avalonia.Controls;
using EndlessRealms.Core;
using EndlessRealms.Core.Services;
using EndlessRealms.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EndlessRealms.Gui.Component;
public partial class TalkWindow : Window
{
    public string? Target { get; set; }
    public CharactorInfo? TargetChar { get; set; }

    public ServiceProvider ServiceProvider { get; set; } = null!;

    public TalkWindow()
    {
        InitializeComponent();
        
        this.inputBox.KeyUp += InputBox_KeyUp;                  
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        ServiceProvider.GetService<SystemStatusManager>()!.StatusChanged += StatusManager_StatusChanged;
        UpdateDisplay();
    }

    private void StatusManager_StatusChanged(object? sender, StatusChangedEventArg e)
    {
        this.inputBox.IsVisible = (e.TheCurrent != SystemStatus.Working);
    }

    private void InputBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            _ = DoInput();
        }
    }

    private void SendInputButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = DoInput();
    }

    private async Task DoInput()
    {
        var text = this.inputBox.Text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }
        this.inputBox.Text = null;

        AddQuestion(text);
        this.chatContent.Children.Last().BringIntoView();

        if (TargetChar == null)
        {            
            var response = await ServiceProvider.GetService<Game>()!.TalkToThing(Target!, text);
            if (response?.CharactorInfo != null)
            {
                Target = null;
                TargetChar = response.CharactorInfo;

                UpdateDisplay();
            }
            else
            {
                this.Close();
            }
        }
        else
        {
            var response = await ServiceProvider.GetService<Game>()!.TalkToCharactor(TargetChar, text);
            AddAnswer(response);

            this.chatContent.Children.Last().BringIntoView();
        }
    }

    private ChatHistory? _history;
    private void UpdateDisplay()
    {
        this.Title = $"Talk to {(Target ?? TargetChar!.FullName)}";

        if (TargetChar != null && _history == null)
        {
            _history = ServiceProvider.GetService<IPersistedDataProvider>()!.GetChatHistory(this.TargetChar!.Id);
            chatContent.Children.Clear();
            foreach (var item in _history.History)
            {
                if (item.Question != null)
                {
                    AddQuestion(item.Question);
                }

                AddAnswer(item.Answer);
            }

            this.chatContent.Children.Last().BringIntoView();
        }
    }

    private void AddAnswer(string answer)
    {
        chatContent.Children.Add(new TextBlock() 
        {
            Text = Target ?? TargetChar!.FullName,            
            Classes = new Classes("Responder")
        });
        chatContent.Children.Add(new TextBlock()
        {
            Text = answer,
            Classes = new Classes("Answer")
        });
    }

    private void AddQuestion(string question)
    {
        chatContent.Children.Add(new TextBlock()
        {
            Text = "You",
            Classes = new Classes("Asker")
        });
        chatContent.Children.Add(new TextBlock()
        {
            Text = question,
            Classes = new Classes("Question")
        });
    }  
}
