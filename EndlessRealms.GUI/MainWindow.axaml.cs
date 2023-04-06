using Avalonia.Controls;
using EndlessRealms.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using EndlessRealms.Core.Utility;

using EndlessRealms.Gui.Services;
using EndlessRealms.Core;
using System.Threading.Tasks;
using Serilog;
using System.Collections.Generic;
using Avalonia.Threading;
using System.Linq;
using EndlessRealms.Gui.Component;
using EndlessRealms.Models;
using Avalonia.Media;

namespace EndlessRealms.Gui;
public partial class MainWindow : Window
{
    private ServiceProvider _serviceProvider = null!;
    private UiModel _uiModel = null!;    
    public MainWindow()
    {
        InitializeComponent();

        Initialize();
    }

    private void Initialize()
    {
        DataContext = _uiModel = new UiModel();
        _uiModel.SceneWindowInfo.CollectionChanged += SceneWindowInfo_CollectionChanged;
        _uiModel.PropertyChanged += _uiModel_PropertyChanged;

        var config = BuildConfig();

        var logger = new LoggerConfiguration()
           .ReadFrom.Configuration(config)
           .WriteTo.UiLogSink(_uiModel)
           .CreateLogger();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.Configure<ChatGptApiSetting>(config.GetSection("ChatGpt"));

        serviceCollection.LoadServices(TheAssembly.Assembly);
        serviceCollection.LoadServices(LocalEnv.TheAssembly.Assembly);

        serviceCollection.AddSingleton<ILogService>(new UILogger(logger));
        serviceCollection.AddSingleton<IRenderService>(new TextRenderService(_uiModel));  

        _serviceProvider = serviceCollection.BuildServiceProvider();

        SetupIoHandler();

        var statusManager = _serviceProvider.GetService<SystemStatusManager>()!;
        statusManager.StatusChanged += MainWindow_StatusChanged;
        statusManager.ErrorHappened += StatusManager_ErrorHappened;

        clearDataButton.Click += ClearDataButton_Click;
        
        Task.Run(()=>_serviceProvider.GetService<Game>()!.Start());                
    }

    private async void ClearDataButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await _serviceProvider.GetService<Game>()!.Stop();
        _serviceProvider.GetRequiredService<PlayerIoManager.Handler>().NotifyInput(InputType.SystemCommand, "reset_all");
        
    }

    private void SetupIoHandler()
    {
        var ioHandler = _serviceProvider.GetRequiredService<PlayerIoManager.Handler>();
        ioHandler.OutputMessage += OnOutputMessage;
        ioHandler.InputLisenterChanged += IoHandler_InputLisenterChanged;
        this.generalInputText.KeyUp += (s, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                DoGeneralInput();
            }
        };
        this.generalInputOkButton.Click += (s, e) => DoGeneralInput();
    }

    private void DoGeneralInput()
    {
        var handler = _serviceProvider.GetRequiredService<PlayerIoManager.Handler>();
        var text = this.generalInputText.Text;
        var validator = handler.GetInputValidator(InputType.GeneralInput);
        if(validator != null) 
        {
            var (valid, errMsg) = validator(text);
            if(valid)
            {
                generalErrorMessage.Text = null;
                handler.NotifyInput(InputType.GeneralInput, text);
                generalInputDialog.IsVisible = false;
            }
            else
            {
                generalErrorMessage.Text = errMsg!;
            }
        }
        
    }

    private void IoHandler_InputLisenterChanged(object? sender, InputType e)
    {
        Dispatcher.UIThread.InvokeAsync(() => 
        {
            if (e == InputType.GeneralInput)
            {
                generalErrorMessage.Text = null;
                generalInputMsgText.Text = _serviceProvider.GetRequiredService<PlayerIoManager.Handler>().GetInputMessage(InputType.GeneralInput);
                generalInputText.Text = null;
                generalInputDialog.IsVisible = true;

                generalInputText.Focus();
            }
        });        
    }

    private void OnOutputMessage(object? sender, (OutputType, string) e)
    {       
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var (type, msg) = e;
            switch(type)
            {
                case OutputType.UiMessage:
                    this._uiModel.InteractiveMessage = msg;
                    break;
                case OutputType.WorldMessage:
                    this._uiModel.WorldMessage = msg;
                    break;
                default:
                    throw new NotImplementedException();
            }
        });
    }

    private void StatusManager_ErrorHappened(object? sender, Exception e)
    {
        _ = ShowError(e);
    }

    private async Task ShowError(Exception err)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            this.errorMessage.Text = err.ToString();
            this.errorMessage.IsVisible= true;
        });

        await Task.Delay(1000 * 5);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            this.errorMessage.Text = null;
            this.errorMessage.IsVisible = false;
        });
    }

    private void _uiModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(UiModel.Scene))
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                UpdateScene();
            });            
        }
    }

    private void UpdateScene()
    {
        this.thingsAndCharPanel.Children.Clear(); 
        this.directionPanel.Children.Clear();
        

        if (_uiModel.Scene == null)
        {
            exitMessage.IsVisible = false;
            thingAndCharPmt.IsVisible = false;
            return;
        }

        exitMessage.IsVisible = _uiModel.Scene!.ConnectedScenes.Count() > 0;
        thingAndCharPmt.IsVisible = (_uiModel.Scene.Characters is { Count: > 0 } || _uiModel.Scene.Things is { Count: > 0 });

        foreach (var dInfo in _uiModel.Scene!.ConnectedScenes)
        {  
            var cs =  _uiModel.World!.Scenes.FirstOrDefault(s => s.Id == dInfo.Value);
            var dUi = new DirectionUi();
            directionPanel.Children.Add(dUi);
            dUi.SetInfo(new DirectionInfo() 
            { 
                Direction = dInfo.Key, 
                Description = $"{dInfo.Key} ({cs?.Name ?? "?"})"
            });
            dUi.SetDirection += DUi_SetDirection;
        }
        

        if(_uiModel.Scene.Characters is { Count: > 0})
        {            
            foreach (var c in _uiModel.Scene.Characters)
            {
                var u = new CharOrThingUi()
                {
                    Character = c
                };
                thingsAndCharPanel.Children.Add(u);
                u.ActionOn += OnActionOn;
            }
        }
        if(_uiModel.Scene.Things is { Count: > 0})
        {
            foreach (var c in _uiModel.Scene.Things)
            {
                var u = new CharOrThingUi()
                {
                    Thing = c
                };
                thingsAndCharPanel.Children.Add(u);
                u.ActionOn += OnActionOn;
            }
        }
        
    }

    private void OnActionOn(object? sender, (Something?, CharacterInfo?) e)
    {
        var (thing, charactor) = e;
        new TalkWindow()
        {
            TargetChar = charactor,
            ServiceProvider = this._serviceProvider
        }
        .ShowDialog(this);
    }

    private void DUi_SetDirection(object? sender, Direction e)
    {
        _serviceProvider.GetRequiredService<PlayerIoManager.Handler>().NotifyInput(InputType.DirectionInput, e.ToString());
    }
   

    private Dictionary<UiModel.Line, Control> _sceneWindowInfo = new Dictionary<UiModel.Line, Control>();
    private void SceneWindowInfo_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (var item in _sceneWindowInfo.Values)
                {
                    this.sceneContainer.Children.Remove(item);
                }
                _sceneWindowInfo.Clear();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (UiModel.Line item in e.NewItems!)
                {
                    _sceneWindowInfo[item] = new TextBlock()
                    {
                        Classes = new Classes("WithTextStyle"),
                        DataContext = item
                    };
                    this.sceneContainer.Children.Add(_sceneWindowInfo[item]);                                       
                }
            }
        });                        
    }


    private void MainWindow_StatusChanged(object? sender, StatusChangedEventArg e)
    {
        var statusMgr = _serviceProvider.GetService<SystemStatusManager>();
        if (statusMgr!.CurrentStatus == SystemStatus.Working)
        {
            _uiModel.InteractiveMessage = _uiModel.CurrentSystemStatus = statusMgr.CurrentStatusMessage;
        }
        else
        {
            _uiModel.CurrentSystemStatus = null;
        }
    }
 
    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
                .AddJsonFile("./appSettings.json", false, false)
                .AddJsonFile("./appSettings.dev.json", false, true)
                .Build();

}