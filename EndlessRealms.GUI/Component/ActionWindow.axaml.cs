using Avalonia.Controls;
using EndlessRealms.Core;
using EndlessRealms.Core.Services;
using EndlessRealms.Core.Services.ActionResponseHandler;
using EndlessRealms.Core.Services.ChatGptDto;
using EndlessRealms.Models;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EndlessRealms.Gui.Component;
public partial class TalkWindow : Window
{
    public IActionTarget ActionTarget { get; set; } = null!;

    public ServiceProvider ServiceProvider { get; set; } = null!;

    private ActionSession _actionSession = null!;

    public TalkWindow()
    {
        InitializeComponent();

        this.inputBox.KeyUp += InputBox_KeyUp;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);


        ServiceProvider.GetService<SystemStatusManager>()!.StatusChanged += StatusManager_StatusChanged;
        Initialize();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        ServiceProvider.GetService<SystemStatusManager>()!.StatusChanged -= StatusManager_StatusChanged;
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
        var text = this.inputBox.Text?.Trim();
        if (string.IsNullOrEmpty(text) || text == "!")
        {
            return;
        }
        this.inputBox.Text = null;

        AddQuestion(text);
        this.chatContent.Children.Last().BringIntoView();

        string response;
        if (text.StartsWith("!"))
        {
            response = await _actionSession.Talk(text);
        }
        else
        {
            response = await _actionSession.Perform(text.Substring(1));
        }

        AddAnswer(response);
    }

    private async void Initialize()
    {
        this.Title = $"Interaction with {ActionTarget?.Name}";
        this.targetName.Text = ActionTarget.Name;
        this.targetDes.Text = ActionTarget.Description;

        _actionSession = ServiceProvider.GetService<ActionSession>()!;

        await _actionSession.Initialize(ActionTarget);

        var history = _actionSession.ActionHistory;
        chatContent.Children.Clear();
        foreach (var item in history.History)
        {
            if (item.Action != null)
            {
                AddQuestion(item.Action);
            }

            AddAnswer(item.Response);
        }
    }

    private void AddAnswer(string answer)
    {
        chatContent.Children.Add(new TextBlock()
        {
            Text = ActionTarget.Name,
            Classes = new Classes("Responder")
        });
        chatContent.Children.Add(new TextBlock()
        {
            Text = answer,
            Classes = new Classes("Answer")
        });

        if (this.chatContent.Children.Count > 0)
        {
            this.chatContent.Children.Last().BringIntoView();
        }
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


        if (this.chatContent.Children.Count > 0)
        {
            this.chatContent.Children.Last().BringIntoView();
        }
    }
}